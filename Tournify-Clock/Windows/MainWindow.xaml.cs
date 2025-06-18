using Gemelo.Applications.Tournify.Clock.Apps;
using Gemelo.Applications.Tournify.Clock.Code.Connectors;
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

namespace Gemelo.Applications.Tournify.Clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member and Properties

        #endregion Member and Properties

        public MainWindow()
        {
            InitializeComponent();

            if(App.Current!=null)
            {
                App.Current.Connector.ModelChanged += Connector_ModelChanged;
            }
        }

        #region EventHandler

        private void Connector_ModelChanged(object? sender, EventArgs e)
        {
            m_DigitalClock2.Time=App.Current.Connector.TournifyModel.CurrentTime;
        }

        #endregion EventHandler

    }
}