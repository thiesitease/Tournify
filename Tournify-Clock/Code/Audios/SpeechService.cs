using Gemelo.Components.Common.Settings;

using Microsoft.CognitiveServices.Speech;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using AppSettings = Gemelo.Applications.Tournify.Clock.Properties.Settings;

namespace Gemelo.Applications.Tournify.Clock.Code.Audios;

/// <summary>
/// Erzeugt natürlich klingende Sprachausgabe über Azure Neural TTS.
/// Synthetisierte Sätze werden als 48-kHz-Mono-WAV auf Platte gecacht, damit
/// wiederkehrende Ansagen (Mannschafts-/Schiedsrichternamen, feste Phrasen)
/// nur einmal erzeugt werden und danach offline aus dem Cache laufen.
///
/// Konfiguration:
///   - Key:    Umgebungsvariable AZURE_SPEECH_KEY (Pflicht; Geheimnis, nicht ins Git)
///   - Region: Umgebungsvariable AZURE_SPEECH_REGION oder Setting AzureSpeechRegion
///   - Stimme: per Auswahldialog (Setting SelectedVoice), sonst AZURE_SPEECH_VOICE oder AzureSpeechVoice
///   - Tempo:  Umgebungsvariable AZURE_SPEECH_RATE oder Setting AzureSpeechRate (z.B. "-10%")
///   - Pause:  Umgebungsvariable AZURE_SPEECH_BREAK_MS oder Setting AzureSpeechBreakMs (ms)
///
/// Im Text setzt der Marker "||" eine Sprechpause (Länge = AzureSpeechBreakMs).
///
/// Stimme, Tempo und Pause werden bei JEDER Synthese frisch gelesen – eine im
/// Dialog gespeicherte Auswahl wirkt also sofort (kein Neustart nötig) und bleibt
/// dank User-Setting auch nach einem Neustart erhalten.
///
/// Ist kein Key gesetzt, ist <see cref="IsConfigured"/> false und der
/// <see cref="AudioController"/> fällt auf System.Speech zurück.
/// </summary>
public class SpeechService
{
    private readonly string m_Key;
    private readonly string m_Region;
    private readonly DirectoryInfo m_CacheDir;

    public SpeechService()
    {
        m_Key = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY") ?? string.Empty;

        m_Region = FirstNonEmpty(
            Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION"),
            AppSettings.Default.AzureSpeechRegion,
            "westeurope");

        m_CacheDir = Directories.GetDirectoryInApplicationDirectory(@"Data\Sounds\tts-cache");
        Directory.CreateDirectory(m_CacheDir.FullName);
    }

    /// <summary>True, wenn ein Azure-Key vorhanden ist und TTS genutzt werden kann.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(m_Key);

    /// <summary>Vollständiger Pfad des Cache-Ordners für synthetisierte WAVs.</summary>
    public string CacheDirectoryPath => m_CacheDir.FullName;

    /// <summary>Die aktuell wirksame Stimme (für Vorauswahl im Dialog).</summary>
    public string CurrentVoice => ResolveVoice(null);

    /// <summary>Das aktuell wirksame Sprechtempo, z.B. "-10%" (für Vorauswahl im Dialog).</summary>
    public string CurrentRate => ResolveRate(null);

    /// <summary>
    /// Favoriten-Stimmen aus dem Setting <c>AzureSpeechFavoriteVoices</c> (komma-/semikolongetrennt).
    /// Werden im Auswahldialog zuerst angezeigt.
    /// </summary>
    public IReadOnlyList<string> FavoriteVoices
    {
        get
        {
            string raw = AppSettings.Default.AzureSpeechFavoriteVoices ?? string.Empty;
            return raw
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }
    }

    /// <summary>Speichert die gewählte Stimme dauerhaft (User-Setting), damit sie nach Neustart wieder aktiv ist.</summary>
    public void SaveSelectedVoice(string voice)
    {
        AppSettings.Default.SelectedVoice = voice ?? string.Empty;
        AppSettings.Default.Save();
    }

    /// <summary>Speichert das gewählte Sprechtempo dauerhaft (User-Setting), z.B. "-10%".</summary>
    public void SaveSelectedRate(string rate)
    {
        AppSettings.Default.SelectedRate = rate ?? string.Empty;
        AppSettings.Default.Save();
    }

    /// <summary>Speichert Stimme und Tempo gemeinsam in einem Rutsch.</summary>
    public void SaveSelection(string voice, string rate)
    {
        AppSettings.Default.SelectedVoice = voice ?? string.Empty;
        AppSettings.Default.SelectedRate = rate ?? string.Empty;
        AppSettings.Default.Save();
    }

    /// <summary>
    /// Liest die verfügbaren deutschsprachigen Stimmen (de-*) direkt von Azure aus.
    /// Liefert eine leere Liste, wenn nicht konfiguriert oder offline.
    /// </summary>
    public async Task<IReadOnlyList<VoiceInfo>> GetGermanVoicesAsync()
    {
        if (!IsConfigured) return Array.Empty<VoiceInfo>();

        try
        {
            var config = SpeechConfig.FromSubscription(m_Key, m_Region);
            using var synthesizer = new SpeechSynthesizer(config, null);
            using var result = await synthesizer.GetVoicesAsync();

            if (result.Reason == ResultReason.VoicesListRetrieved)
            {
                return result.Voices
                    .Where(v => v.Locale.StartsWith("de", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(v => v.Locale, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(v => v.ShortName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            Debug.WriteLine($"[SpeechService] Stimmenliste nicht abrufbar: {result.Reason} / {result.ErrorDetails}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SpeechService] Fehler beim Abruf der Stimmen: {ex.Message}");
        }

        return Array.Empty<VoiceInfo>();
    }

    /// <summary>
    /// Löscht alle gecachten Sprach-WAVs (und übrig gebliebene .tmp-Reste).
    /// Gibt die Anzahl gelöschter WAV-Dateien zurück.
    /// </summary>
    public int ClearCache()
    {
        int deleted = 0;
        if (!Directory.Exists(m_CacheDir.FullName)) return deleted;

        foreach (string file in Directory.EnumerateFiles(m_CacheDir.FullName, "*.wav"))
        {
            try
            {
                File.Delete(file);
                deleted++;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SpeechService] Konnte '{file}' nicht löschen: {ex.Message}");
            }
        }

        foreach (string tmp in Directory.EnumerateFiles(m_CacheDir.FullName, "*.tmp"))
        {
            try { File.Delete(tmp); } catch { /* unkritisch */ }
        }

        return deleted;
    }

    /// <summary>
    /// Liefert den Pfad zu einer WAV-Datei mit der gesprochenen Fassung von <paramref name="text"/>.
    /// Kommt aus dem Cache, falls vorhanden, sonst wird sie von Azure erzeugt und gecacht.
    /// Mit <paramref name="voiceOverride"/> kann eine bestimmte Stimme erzwungen werden (z.B. zum Testen).
    /// Gibt null zurück, wenn nicht konfiguriert oder die Synthese fehlschlägt.
    /// </summary>
    public async Task<string?> GetWavPathAsync(string text, string? voiceOverride = null, string? rateOverride = null)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(text)) return null;

        string voice = ResolveVoice(voiceOverride);
        string rate = ResolveRate(rateOverride);
        int breakMs = ResolveBreakMs();

        // SSML steuert Stimme, Tempo und Pausen. Es ist gleichzeitig der Cache-Schlüssel:
        // ändert sich Stimme/Tempo/Pause/Text, entsteht automatisch eine neue WAV.
        string ssml = BuildSsml(text, voice, rate, breakMs);
        string path = GetCachePathFor(ssml);
        if (File.Exists(path)) return path;

        try
        {
            var config = SpeechConfig.FromSubscription(m_Key, m_Region);
            config.SpeechSynthesisVoiceName = voice;
            // 48 kHz mono passt zur Sample-Rate des NAudio-Mixers (AudioPlaybackEngine).
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff48Khz16BitMonoPcm);

            // null-AudioConfig => kein Direkt-Playback, Audio landet im Ergebnis.
            using var synthesizer = new SpeechSynthesizer(config, null);
            using var result = await synthesizer.SpeakSsmlAsync(ssml);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                // Erst in temporäre Datei schreiben, dann atomar umbenennen,
                // damit ein paralleler Lauf nie eine halb geschriebene WAV liest.
                string tmp = path + ".tmp";
                await File.WriteAllBytesAsync(tmp, result.AudioData);
                File.Move(tmp, path, overwrite: true);
                return path;
            }

            if (result.Reason == ResultReason.Canceled)
            {
                var details = SpeechSynthesisCancellationDetails.FromResult(result);
                Debug.WriteLine($"[SpeechService] Synthese abgebrochen: {details.Reason} / {details.ErrorDetails}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SpeechService] Fehler bei der Synthese: {ex.Message}");
        }

        return null;
    }

    // --- Konfigurationsauflösung (bei jedem Aufruf frisch) ---

    private static string ResolveVoice(string? voiceOverride)
        => FirstNonEmpty(
            voiceOverride,
            AppSettings.Default.SelectedVoice,                       // im Dialog gespeicherte Auswahl
            Environment.GetEnvironmentVariable("AZURE_SPEECH_VOICE"),
            AppSettings.Default.AzureSpeechVoice,
            "de-DE-GiselaNeural");

    private static string ResolveRate(string? rateOverride)
        => FirstNonEmpty(
            rateOverride,
            AppSettings.Default.SelectedRate,                        // im Dialog gespeichertes Tempo
            Environment.GetEnvironmentVariable("AZURE_SPEECH_RATE"),
            AppSettings.Default.AzureSpeechRate,
            "-10%");

    private static int ResolveBreakMs()
    {
        if (int.TryParse(Environment.GetEnvironmentVariable("AZURE_SPEECH_BREAK_MS"), out int envMs) && envMs >= 0)
            return envMs;
        int settingMs = AppSettings.Default.AzureSpeechBreakMs;
        return settingMs >= 0 ? settingMs : 350;
    }

    /// <summary>
    /// Baut das SSML: setzt Stimme und Sprechtempo (prosody rate) und wandelt den
    /// Marker "||" im Text in eine echte Pause (break) um.
    /// </summary>
    private static string BuildSsml(string text, string voice, string rate, int breakMs)
    {
        // Erst XML-escapen (Namen können &, < o.ä. enthalten), dann den Marker einsetzen.
        string body = System.Security.SecurityElement.Escape(text) ?? string.Empty;
        body = body.Replace("||", $"<break time=\"{breakMs}ms\"/>");

        return "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"de-DE\">"
             + $"<voice name=\"{voice}\"><prosody rate=\"{rate}\">{body}</prosody></voice></speak>";
    }

    private string GetCachePathFor(string ssml)
    {
        // Das vollständige SSML als Schlüssel: deckt Stimme, Tempo, Pausen und Text ab.
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(ssml));
        string fileName = Convert.ToHexString(hash) + ".wav";
        return Path.Combine(m_CacheDir.FullName, fileName);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v)) return v!;
        }
        return string.Empty;
    }
}
