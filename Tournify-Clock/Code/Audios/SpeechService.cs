using Gemelo.Components.Common.Settings;

using Microsoft.CognitiveServices.Speech;

using System;
using System.Diagnostics;
using System.IO;
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
///   - Stimme: Umgebungsvariable AZURE_SPEECH_VOICE oder Setting AzureSpeechVoice
///   - Tempo:  Umgebungsvariable AZURE_SPEECH_RATE oder Setting AzureSpeechRate (z.B. "-10%")
///   - Pause:  Umgebungsvariable AZURE_SPEECH_BREAK_MS oder Setting AzureSpeechBreakMs (ms)
///
/// Im Text setzt der Marker "||" eine Sprechpause (Länge = AzureSpeechBreakMs).
///
/// Ist kein Key gesetzt, ist <see cref="IsConfigured"/> false und der
/// <see cref="AudioController"/> fällt auf System.Speech zurück.
/// </summary>
public class SpeechService
{
    private readonly string m_Key;
    private readonly string m_Region;
    private readonly string m_Voice;
    private readonly string m_Rate;
    private readonly int m_BreakMs;
    private readonly DirectoryInfo m_CacheDir;

    public SpeechService()
    {
        m_Key = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY") ?? string.Empty;

        m_Region = FirstNonEmpty(
            Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION"),
            AppSettings.Default.AzureSpeechRegion,
            "westeurope");

        m_Voice = FirstNonEmpty(
            Environment.GetEnvironmentVariable("AZURE_SPEECH_VOICE"),
            AppSettings.Default.AzureSpeechVoice,
            "de-DE-GiselaNeural");
            //"de-DE-FlorianMultilingualNeural");

        // Sprechtempo, z.B. "-10%" (langsamer), "0%", "slow". Steuert das "Runterrattern".
        m_Rate = FirstNonEmpty(
            Environment.GetEnvironmentVariable("AZURE_SPEECH_RATE"),
            AppSettings.Default.AzureSpeechRate,
            "-10%");

        // Länge der Pause für den Marker "||" im Text (in Millisekunden).
        m_BreakMs = ResolveBreakMs();

        m_CacheDir = Directories.GetDirectoryInApplicationDirectory(@"Data\Sounds\tts-cache");
        Directory.CreateDirectory(m_CacheDir.FullName);
    }

    private static int ResolveBreakMs()
    {
        if (int.TryParse(Environment.GetEnvironmentVariable("AZURE_SPEECH_BREAK_MS"), out int envMs) && envMs >= 0)
            return envMs;
        int settingMs = AppSettings.Default.AzureSpeechBreakMs;
        return settingMs >= 0 ? settingMs : 350;
    }

    /// <summary>True, wenn ein Azure-Key vorhanden ist und TTS genutzt werden kann.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(m_Key);

    /// <summary>Vollständiger Pfad des Cache-Ordners für synthetisierte WAVs.</summary>
    public string CacheDirectoryPath => m_CacheDir.FullName;

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
    /// Gibt null zurück, wenn nicht konfiguriert oder die Synthese fehlschlägt.
    /// </summary>
    public async Task<string?> GetWavPathAsync(string text)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(text)) return null;

        // SSML steuert Stimme, Tempo und Pausen. Es ist gleichzeitig der Cache-Schlüssel:
        // ändert sich Stimme/Tempo/Pause/Text, entsteht automatisch eine neue WAV.
        string ssml = BuildSsml(text);
        string path = GetCachePathFor(ssml);
        if (File.Exists(path)) return path;

        try
        {
            var config = SpeechConfig.FromSubscription(m_Key, m_Region);
            config.SpeechSynthesisVoiceName = m_Voice;
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

    /// <summary>
    /// Baut das SSML: setzt Stimme und Sprechtempo (prosody rate) und wandelt den
    /// Marker "||" im Text in eine echte Pause (break) um. So lässt sich das
    /// "Runterrattern" bremsen und an gewünschten Stellen eine Sprechpause setzen.
    /// </summary>
    private string BuildSsml(string text)
    {
        // Erst XML-escapen (Namen können &, < o.ä. enthalten), dann den Marker einsetzen.
        string body = System.Security.SecurityElement.Escape(text) ?? string.Empty;
        body = body.Replace("||", $"<break time=\"{m_BreakMs}ms\"/>");

        return "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"de-DE\">"
             + $"<voice name=\"{m_Voice}\"><prosody rate=\"{m_Rate}\">{body}</prosody></voice></speak>";
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
