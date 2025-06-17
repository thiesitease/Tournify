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

namespace Tournify_Clock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member and Properties

        public TournifyConnector Connector { get; } = new TournifyConnector(
            new Uri(Settings.Default.TournifyUrl));

        #endregion Member and Properties

        public MainWindow()
        {
            InitializeComponent();

            m_Web.Loaded += Web_Loaded;
        }

        private void Web_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #region EventHandler

        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            _ = Parse();
        }

        private async Task Parse()
        {
            string html = await DumpHtml();
            await Connector.ReadContent(html);
        }

        #endregion EventHandler

        private void ButtonOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            _ = Open();
        }

        private async Task Open()
        {
            await m_Web.EnsureCoreWebView2Async(null);
            m_Web.Source = new(Settings.Default.TournifyUrl);
        }

        private async Task<string> DumpHtml()
        {
            string html = await m_Web.ExecuteScriptAsync("document.documentElement.outerHTML");

            // WebView2 gibt einen JSON-String zurück (mit Anführungszeichen)
            string decodedHtml = System.Text.Json.JsonSerializer.Deserialize<string>(html);

            // Jetzt kannst du den HTML-Quelltext verwenden
            Console.WriteLine(decodedHtml);

            return decodedHtml;
        }
    }
}