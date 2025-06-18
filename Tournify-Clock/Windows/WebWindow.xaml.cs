using Gemelo.Applications.Tournify.Clock.Apps;
using Gemelo.Applications.Tournify.Clock.Properties;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gemelo.Applications.Tournify.Clock.Windows;
/// <summary>
/// Interaktionslogik für WebWindow.xaml
/// </summary>
public partial class WebWindow : Window
{
    private DispatcherTimer m_Timer;

    public WebWindow()
    {
        InitializeComponent();
        Loaded += WebWindow_Loaded;

        m_Timer=new DispatcherTimer();
        m_Timer.Tick += Timer_Tick;
        m_Timer.Interval = TimeSpan.FromSeconds(5);
        m_Timer.Start();
    }

    private void WebWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _ = Open();
    }

    #region EventHandler

    private void ButtonRead_Click(object sender, RoutedEventArgs e)
    {
        _ = Parse();
    }

    private async Task Parse()
    {
        try
        {
            string html = await DumpHtml();
            await App.Current.Connector.ReadContent(html);
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }
    }

    #endregion EventHandler

    private void ButtonOpenWeb_Click(object sender, RoutedEventArgs e)
    {
        _ = Open();
    }

    private async Task Open()
    {
        try
        {
            await m_Web.EnsureCoreWebView2Async(null);
            m_Web.Source = new(Settings.Default.TournifyUrl);
        }
        catch
        {
            Debugger.Break();
        }
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

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _=Parse();
    }
}
