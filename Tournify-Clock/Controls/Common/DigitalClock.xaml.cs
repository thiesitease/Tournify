using Gemelo.Applications.Tournify.Clock.Apps;
using Gemelo.Applications.Tournify.Clock.Code.Models;

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

    public bool IsRemainingPlayingClock { get; set; }
    public bool IsRemainingBreakClock { get; set; }

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


    private List<MatchModel> m_Matches = null;

    public List<MatchModel>? Matches
    {
        get { return m_Matches; }
        set
        {
            if (value != m_Matches)
            {
                m_Matches = value;
            }
        }
    }



    public DigitalClock()
    {
        InitializeComponent();
        m_Timer = new DispatcherTimer();
        m_Timer.Interval = TimeSpan.FromSeconds(1);

        Time = TimeSpan.Zero;

        if (App.Current != null)
        {
#if (DEBUG)
            m_Timer.Interval = TimeSpan.FromSeconds(1) / App.Current.FakeTimeFactor;

#endif
            m_Timer.Tick += Timer_Tick;
            m_Timer.Start();
        }


    }

    private bool m_HasSpoken;

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (ShowCurrentTime)
        {
            Time = App.Current.Now.TimeOfDay;
        }
        else if (Matches?.Count > 0)
        {
            FontSize = 80;
            MatchModel match = Matches[0];

            DateTime remaining;

            if (IsRemainingPlayingClock)
            {
                remaining = match.EndTime - App.Current.Now.TimeOfDay;

                if (remaining.TimeOfDay < TimeSpan.FromMinutes(1))
                {
                    if (!m_HasSpoken)
                    {
                        m_HasSpoken = true;
                        App.Current.SpeechSynthesizer.SpeakAsync("Noch eine Minute zu Spielen!");
                    }
                }
                else
                {
                    m_HasSpoken = false;
                }
            }
            else if (IsRemainingBreakClock)
            {
                remaining = match.StartTime - App.Current.Now.TimeOfDay;

                if (remaining.TimeOfDay < TimeSpan.FromMinutes(1))
                {
                    if (!m_HasSpoken)
                    {
                        m_HasSpoken = true;
                        App.Current.SpeechSynthesizer.SpeakAsync("Noch eine Minute bis zum Start!");
                    }
                }
                else
                {
                    m_HasSpoken = false;
                }
            }
            else
            {
                remaining = DateTime.Now;
            }
            Time = remaining.TimeOfDay;
        }
        else
        {
            FontSize = 50;
            Time = TimeSpan.Zero;
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
