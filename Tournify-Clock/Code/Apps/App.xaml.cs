using Gemelo.Applications.Tournify.Clock.Code.Connectors;
using Gemelo.Applications.Tournify.Clock.Properties;
using Gemelo.Applications.Tournify.Clock.Windows;

using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media.Animation;

namespace Gemelo.Applications.Tournify.Clock.Apps
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static new App Current { get; private set; }

        public WebWindow WebWindow { get; private set; }

        #region Member and Properties

        public TournifyConnector Connector { get; } = new TournifyConnector(new Uri(Settings.Default.TournifyUrl));

        #endregion Member and Properties

        public App()
        {
            Current = this;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow=new MainWindow();
            MainWindow.Show();

            WebWindow = new WebWindow();
            WebWindow.Show();

        }
    }

}
