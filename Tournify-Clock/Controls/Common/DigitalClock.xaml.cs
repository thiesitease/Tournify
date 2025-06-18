using Grpc.Net.Client.Balancer;

using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Gemelo.Applications.Tournify.Clock.Controls.Common;
/// <summary>
/// Interaktionslogik für DigitalClock.xaml
/// </summary>
public partial class DigitalClock : UserControl
{
    private DispatcherTimer m_Timer;
    private TimeSpan m_Time = TimeSpan.Zero;

    public TimeSpan Time
    {
        get { return m_Time; }
        set
        {
            if (value != m_Time)
            {
                m_Time = value;
                UpdateUi();
            }
        }
    }


    private bool m_ShowCurrentTime = false;

    public bool ShowCurrentTime
    {
        get { return m_ShowCurrentTime; }
        set
        {
            if (value != m_ShowCurrentTime)
            {
                m_ShowCurrentTime = value;

            }
        }
    }



    public DigitalClock()
    {
        InitializeComponent();
        m_Timer = new DispatcherTimer();
        m_Timer.Interval = TimeSpan.FromSeconds(1);
        m_Timer.Tick += Timer_Tick;
        m_Timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (ShowCurrentTime)
        {
            Time = DateTime.Now.TimeOfDay;
        }
    }

    private void UpdateUi()
    {
        m_TxtHourBig.Text = $"{Time.Hours / 10}";
        m_TxtHourSmall.Text = $"{Time.Hours % 10}";
        m_TxtMinuteBig.Text = $"{Time.Minutes / 10}";
        m_TxtMinuteSmall.Text = $"{Time.Minutes % 10}";
        m_TxtSecondBig.Text = $"{Time.Seconds / 10}";
        m_TxtSecondSmall.Text = $"{Time.Seconds % 10}";
    }

}
