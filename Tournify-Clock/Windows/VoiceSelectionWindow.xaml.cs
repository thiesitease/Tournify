using Gemelo.Applications.Tournify.Clock.Code.Audios;

using Microsoft.CognitiveServices.Speech;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gemelo.Applications.Tournify.Clock
{
    /// <summary>
    /// Dialog zur Auswahl der KI-Stimme. Liest die deutschen Stimmen automatisch von Azure aus
    /// (Favoriten zuerst), bietet eine Hörprobe und speichert die Auswahl dauerhaft.
    /// </summary>
    public partial class VoiceSelectionWindow : Window
    {
        private readonly SpeechService m_Speech = AudioController.Default.SpeechService;

        public VoiceSelectionWindow()
        {
            InitializeComponent();
            Loaded += async (_, __) => await LoadVoicesAsync();
        }

        /// <summary>Listeneintrag: technischer Name + Anzeigetext.</summary>
        private sealed class VoiceItem
        {
            public string ShortName { get; init; } = string.Empty;
            public string Display { get; init; } = string.Empty;
        }

        private async Task LoadVoicesAsync()
        {
            m_ListVoices.IsEnabled = false;
            m_TxtInfo.Text = "Verfügbare deutsche Stimmen werden geladen …";

            IReadOnlyList<string> favorites = m_Speech.FavoriteVoices;
            var items = new List<VoiceItem>();

            IReadOnlyList<VoiceInfo> voices = await m_Speech.GetGermanVoicesAsync();

            if (voices.Count > 0)
            {
                var byName = voices
                    .GroupBy(v => v.ShortName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 1) Favoriten zuerst (nur die, die es wirklich gibt)
                foreach (string fav in favorites)
                {
                    if (byName.TryGetValue(fav, out VoiceInfo? vi) && seen.Add(vi.ShortName))
                        items.Add(ToItem(vi, isFavorite: true));
                }

                // 2) Alle übrigen Stimmen
                foreach (VoiceInfo vi in voices)
                {
                    if (seen.Add(vi.ShortName))
                        items.Add(ToItem(vi, isFavorite: false));
                }

                m_TxtInfo.Text = $"{voices.Count} deutsche Stimmen gefunden. Favoriten (★) stehen oben.";
            }
            else
            {
                // Fallback: kein Key/Internet -> nur die Favoriten aus dem Setting anbieten.
                foreach (string fav in favorites)
                    items.Add(new VoiceItem { ShortName = fav, Display = "★  " + fav });

                m_TxtInfo.Text = "Automatisches Auslesen nicht möglich (kein Key oder kein Internet). "
                               + "Es werden die Favoriten aus dem Setting 'AzureSpeechFavoriteVoices' angezeigt.";
            }

            m_ListVoices.ItemsSource = items;
            m_ListVoices.IsEnabled = items.Count > 0;

            // Aktuell wirksame Stimme vorauswählen.
            string current = m_Speech.CurrentVoice;
            VoiceItem? preselect = items.FirstOrDefault(i => string.Equals(i.ShortName, current, StringComparison.OrdinalIgnoreCase));
            m_ListVoices.SelectedItem = preselect ?? items.FirstOrDefault();
            m_ListVoices.ScrollIntoView(m_ListVoices.SelectedItem);

            // Aktuell wirksames Tempo in den Regler übernehmen.
            int currentRate = ParseRatePercent(m_Speech.CurrentRate);
            m_SliderRate.Value = Math.Clamp(currentRate, (int)m_SliderRate.Minimum, (int)m_SliderRate.Maximum);
            UpdateRateLabel();

            UpdateButtons();
        }

        // --- Tempo (Geschwindigkeit) ---

        /// <summary>Aktuell im Regler eingestelltes Tempo als Azure-Wert, z.B. "-10%".</summary>
        private string RateString => FormatRate((int)Math.Round(m_SliderRate.Value));

        private void SliderRate_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
            => UpdateRateLabel();

        private void UpdateRateLabel()
        {
            if (m_TxtRate == null) return; // kann beim Laden vor dem Label feuern
            m_TxtRate.Text = FormatRate((int)Math.Round(m_SliderRate.Value));
        }

        private static string FormatRate(int percent)
        {
            if (percent > 0) return $"+{percent}%";
            return $"{percent}%"; // negativ enthält bereits "-", 0 => "0%"
        }

        private static int ParseRatePercent(string? rate)
        {
            if (string.IsNullOrWhiteSpace(rate)) return 0;
            string digits = new string(rate.Where(c => char.IsDigit(c) || c == '-' || c == '+').ToArray());
            return int.TryParse(digits, out int value) ? value : 0;
        }

        private static VoiceItem ToItem(VoiceInfo v, bool isFavorite)
        {
            string gender = v.Gender switch
            {
                SynthesisVoiceGender.Female => "weiblich",
                SynthesisVoiceGender.Male => "männlich",
                _ => "neutral",
            };
            string prefix = isFavorite ? "★  " : "     ";
            return new VoiceItem
            {
                ShortName = v.ShortName,
                Display = $"{prefix}{v.LocalName} ({gender}, {v.Locale}) – {v.ShortName}",
            };
        }

        private string? SelectedVoiceName => m_ListVoices.SelectedValue as string;

        private void UpdateButtons()
        {
            bool hasSelection = !string.IsNullOrEmpty(SelectedVoiceName);
            m_BtnOk.IsEnabled = hasSelection;
            m_BtnTest.IsEnabled = hasSelection;
        }

        private void ListVoices_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateButtons();

        private void ListVoices_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedVoiceName)) ApplyAndClose();
        }

        private async void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            string? voice = SelectedVoiceName;
            if (string.IsNullOrEmpty(voice)) return;

            m_BtnTest.IsEnabled = false;
            try
            {
                await AudioController.Default.SpeakTestAsync(m_TxtTest.Text, voice, RateString);
            }
            finally
            {
                m_BtnTest.IsEnabled = true;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) => ApplyAndClose();

        private void ApplyAndClose()
        {
            string? voice = SelectedVoiceName;
            if (string.IsNullOrWhiteSpace(voice))
            {
                DialogResult = false;
                return;
            }

            m_Speech.SaveSelection(voice, RateString);
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
