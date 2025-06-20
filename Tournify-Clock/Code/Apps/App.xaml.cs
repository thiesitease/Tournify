using Gemelo.Applications.Tournify.Clock.Code.Connectors;
using Gemelo.Applications.Tournify.Clock.Properties;
using Gemelo.Applications.Tournify.Clock.Windows;

using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Media.Animation;

namespace Gemelo.Applications.Tournify.Clock.Apps
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const bool UseFaketime = false;

#if (DEBUG)

        public readonly DateTime FakeNow = UseFaketime ? new DateTime(2025, 06, 21, 09, 10, 00) : DateTime.Now;
        public readonly double FakeTimeFactor = UseFaketime ? 25.0 : 1.0;


#endif

        public Stopwatch Elapsed { get; } = new Stopwatch();

        public static new App Current { get; private set; }

        public WebWindow WebWindow { get; private set; }

        public TimeSpan MatchDuration { get; private set; }
        public TimeSpan MatchPrepareBeforeNextStart { get; private set; }

        public SpeechSynthesizer SpeechSynthesizer { get; }


        public DateTime Now
        {
            get
            {
#if(DEBUG)
                if (UseFaketime)
                    return FakeNow + FakeTimeFactor * Elapsed.Elapsed;
                else
                    return DateTime.Now;
#else
                return DateTime.Now;
#endif
            }
        }

        #region Member and Properties

        public TournifyConnector Connector { get; }

        #endregion Member and Properties

        public App()
        {
            Current = this;
            Elapsed.Start();
#if (DEBUG)
            //string demoUrl = "https://www.tournify.de/live/develop-test/present";
            //Connector = new TournifyConnector(new Uri(demoUrl));
            //MatchDuration = TimeSpan.FromMinutes(2);
            //MatchPrepareBeforeEnd = TimeSpan.FromMinutes(6);

            Connector = new TournifyConnector(new Uri(Settings.Default.TournifyUrl));
            MatchDuration = Settings.Default.MatchDuration;
            MatchPrepareBeforeNextStart = TimeSpan.FromMinutes(15);
#else
            Connector = new TournifyConnector(new Uri(Settings.Default.TournifyUrl));
            MatchDuration = Settings.Default.MatchDuration;
            MatchPrepareBeforeNextStart = TimeSpan.FromMinutes(15);
#endif

            SpeechSynthesizer = new SpeechSynthesizer();

            var voices = SpeechSynthesizer.GetInstalledVoices();
            foreach (var voice in voices)
            {
                Debug.WriteLine(voice.ToString());

            }
            SpeechSynthesizer.SelectVoice(voices[0].VoiceInfo.Name);
            //SpeechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Senior);
            SpeechSynthesizer.SetOutputToDefaultAudioDevice();

            SpeechSynthesizer.Speak("Willkommen beim Kiwi Cup");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();

            WebWindow = new WebWindow();
            WebWindow.Show();

        }
    }

}
