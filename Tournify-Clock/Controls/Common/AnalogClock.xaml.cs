using Gemelo.Applications.Tournify.Clock.Apps;

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

using static Grpc.Core.Metadata;

namespace Gemelo.Applications.Tournify.Clock.Controls.Common;
/// <summary>
/// Interaktionslogik für AnalogClock.xaml
/// </summary>
public partial class AnalogClock : UserControl
{
    RotateTransform m_SecondTransform;
    RotateTransform m_MinuteTransform;
    RotateTransform m_HourTransform;

    DispatcherTimer m_Timer;

    const double CenterX = 200;
    const double CenterY = 200;
    const double Radius = 190;

    public AnalogClock()
    {
        InitializeComponent();

        m_SecondTransform = new RotateTransform(0, 200, 200);
        m_MinuteTransform = new RotateTransform(0, 200, 200);
        m_HourTransform = new RotateTransform(0, 200, 200);

        DrawClockFace();

        secondHand.RenderTransform = m_SecondTransform;
        minuteHand.RenderTransform = m_MinuteTransform;
        hourHand.RenderTransform = m_HourTransform;

        if (App.Current != null)
        {
            m_Timer = new DispatcherTimer();
            m_Timer.Interval = TimeSpan.FromSeconds(1);
#if (DEBUG)
            m_Timer.Interval = TimeSpan.FromSeconds(1) / App.Current.FakeTimeFactor;
#endif
            m_Timer.Tick += Timer_Tick;
            m_Timer.Start();
            UpdateClock(); // initial
        }

    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        UpdateClock();
    }

    private void UpdateClock()
    {
        DateTime now = App.Current.Now;

        double secondAngle = now.Second * 6;
        double minuteAngle = now.Minute * 6 + now.Second * 0.1;
        double hourAngle = (now.Hour % 12) * 30 + now.Minute * 0.5;

        m_SecondTransform.Angle = secondAngle;
        m_MinuteTransform.Angle = minuteAngle;
        m_HourTransform.Angle = hourAngle;

    }

    private void DrawClockFace()
    {
        for (int i = 0; i < 60; i++)
        {
            double angle = i * 6;
            double rad = angle * Math.PI / 180;

            double inner = (i % 5 == 0) ? Radius - 20 : Radius - 10;
            double outer = Radius;

            double x1 = CenterX + inner * Math.Sin(rad);
            double y1 = CenterY - inner * Math.Cos(rad);
            double x2 = CenterX + outer * Math.Sin(rad);
            double y2 = CenterY - outer * Math.Cos(rad);

            Line tick = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Black,
                StrokeThickness = (i % 5 == 0) ? 2 : 1
            };

            ClockCanvas.Children.Add(tick);
        }

        for (int i = 1; i <= 12; i++)
        {
            double angle = i * 30;
            double rad = angle * Math.PI / 180;

            double r = Radius - 35;
            double x = CenterX + r * Math.Sin(rad);
            double y = CenterY - r * Math.Cos(rad);

            TextBlock number = new TextBlock
            {
                Text = i.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };

            number.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size size = number.DesiredSize;

            Canvas.SetLeft(number, x - size.Width / 2);
            Canvas.SetTop(number, y - size.Height / 2);

            ClockCanvas.Children.Add(number);
        }
    }
}
