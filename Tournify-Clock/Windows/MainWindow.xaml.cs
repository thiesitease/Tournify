using Gemelo.Applications.Tournify.Clock.Apps;
using Gemelo.Applications.Tournify.Clock.Code.Connectors;
using Gemelo.Applications.Tournify.Clock.Code.Models;
using Gemelo.Applications.Tournify.Clock.Properties;

using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gemelo.Applications.Tournify.Clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member and Properties

        private DispatcherTimer m_Timer;

        #endregion Member and Properties

        public MainWindow()
        {
            InitializeComponent();

            m_Timer = new DispatcherTimer();
            m_Timer.Tick += Timer_Tick;
            m_Timer.Interval = TimeSpan.FromSeconds(1);
#if (DEBUG)
            m_Timer.Interval = TimeSpan.FromSeconds(1) / App.Current.FakeTimeFactor;

#endif

            if (App.Current != null)
            {
                App.Current.Connector.ModelChanged += Connector_ModelChanged;
                m_Timer.Start();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            AnalyseMatches();
        }

        #region EventHandler

        private void Connector_ModelChanged(object? sender, EventArgs e)
        {
            m_DigitalClock2.Time = App.Current.Connector.TournifyModel.CurrentTime;

            AnalyseMatches();
        }

        private void AnalyseMatches()
        {
            var allMatchesSorted = App.Current.Connector.TournifyModel.MatchModels.OrderBy(x => x.StartTime);

            List<MatchModel> current = new List<MatchModel>();
            List<MatchModel> prepare = new List<MatchModel>();
            List<MatchModel> upcoming = new List<MatchModel>();
            foreach (var match in allMatchesSorted)
            {
                if (IsCurrentlyPlayingMatch(match)) current.Add(match);
                else if (IsReadyToPrepare(match)) prepare.Add(match);
                else if (IsUpcomingMatch(match) && upcoming.Count < Settings.Default.DisplayNextMatchesCount) upcoming.Add(match);
            }

            if (m_MatchesDisplayReadyToPrepare.Matches != null && m_MatchesDisplayReadyToPrepare.Matches.Count == 0 && prepare.Count > 0)
            {
                if (prepare.Count == 1)
                {
                    _ = App.Current.SpeechSynthesizer.SpeakAsync($"Es machen sich bitte bereit die Mannschaften {prepare[0].Team1} gegen {prepare[0].Team2} auf dem {prepare[0].FieldName} gepfiffen von {prepare[0].MatchReferee.Replace("&amp;", "und")}.");
                }
                else if (prepare.Count >= 2)
                {
                    _ = App.Current.SpeechSynthesizer.SpeakAsync($"Es machen sich bitte bereit die Mannschaften {prepare[0].Team1} gegen {prepare[0].Team2} auf dem {prepare[0].FieldName} gepfiffen von {prepare[0].MatchReferee.Replace("&amp;", "und")} und die Mannschaften {prepare[1].Team1} gegen {prepare[1].Team2} auf dem {prepare[1].FieldName} gepfiffen von {prepare[1].MatchReferee.Replace("&amp;", "und")}.");
                }
            }

            if (m_MatchesDisplayCurrent.Matches != null && m_MatchesDisplayCurrent.Matches.Count == 0 && current.Count >= 2)
            {
                _ = App.Current.SpeechSynthesizer.SpeakAsync($"Anpfiff!");
            }
            if (m_MatchesDisplayCurrent.Matches != null && m_MatchesDisplayCurrent.Matches.Count > 0 && current.Count == 0)
            {
                _ = App.Current.SpeechSynthesizer.SpeakAsync($"Abpfiff!");
            }


            m_MatchesDisplayCurrent.Matches = current;
            m_MatchesDisplayReadyToPrepare.Matches = prepare;
            m_MatchesDisplayUpcoming.Matches = upcoming;

            m_MatchesDisplayDebug.Matches = allMatchesSorted.ToList();

            m_DigitalClockRemainingPlay.Matches = current;
            if (current.Count == 0) m_DigitalClockRemainingBreak.Matches = prepare;
            else m_DigitalClockRemainingBreak.Matches = null;
        }

        /// <summary>
        /// Spiele, die gerade laufen. Now ist zwischen Start und Ende
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private bool IsCurrentlyPlayingMatch(MatchModel match)
        {
            DateTime now = App.Current.Now;

            return now > match.StartTime && now < match.EndTime;

            //if (match.MatchColumnScoreDate != now.Date) return false; // anderer Tag
            //else if (match.MatchColumnScoreTime > now.TimeOfDay) return false; // Spiel liegt in der Zukunft
            //else if (match.MatchColumnScoreTime.Add(App.Current.MatchDuration) < now.TimeOfDay) return false; // Spiel liegt länger in der Vergangenheit
            //else return true;
        }

        /// <summary>
        /// Spiele, die vorbereitet werden müssen. Now 
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private bool IsReadyToPrepare(MatchModel match)
        {
            DateTime now = App.Current.Now;

            return now > (match.StartTime - App.Current.MatchPrepareBeforeNextStart) && now < match.StartTime;

        }

        private bool IsUpcomingMatch(MatchModel match)
        {
            DateTime now = App.Current.Now;

            return now < match.StartTime;

            if (match.StartTime < now.Add(App.Current.MatchDuration)) return false; // Spiel liegt länger in der Vergangenheit
            else return true;
        }

        #endregion EventHandler

    }
}