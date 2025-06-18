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
    RotateTransform secondTransform;
    RotateTransform minuteTransform;
    RotateTransform hourTransform;

    DispatcherTimer timer = new DispatcherTimer();

    const double centerX = 200;
    const double centerY = 200;
    const double radius = 190;

    public AnalogClock()
    {
        InitializeComponent();

        secondTransform = new RotateTransform(0, 200, 200);
        minuteTransform = new RotateTransform(0, 200, 200);
        hourTransform = new RotateTransform(0, 200, 200);

        DrawClockFace();

        secondHand.RenderTransform = secondTransform;
        minuteHand.RenderTransform = minuteTransform;
        hourHand.RenderTransform = hourTransform;

        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();

        UpdateClock(); // initial
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        UpdateClock();
    }

    private void UpdateClock()
    {
        DateTime now = DateTime.Now;

        double secondAngle = now.Second * 6;
        double minuteAngle = now.Minute * 6 + now.Second * 0.1;
        double hourAngle = (now.Hour % 12) * 30 + now.Minute * 0.5;

        secondTransform.Angle = secondAngle;
        minuteTransform.Angle = minuteAngle;
        hourTransform.Angle = hourAngle;

    }

    private void DrawClockFace()
    {
        for (int i = 0; i < 60; i++)
        {
            double angle = i * 6;
            double rad = angle * Math.PI / 180;

            double inner = (i % 5 == 0) ? radius - 20 : radius - 10;
            double outer = radius;

            double x1 = centerX + inner * Math.Sin(rad);
            double y1 = centerY - inner * Math.Cos(rad);
            double x2 = centerX + outer * Math.Sin(rad);
            double y2 = centerY - outer * Math.Cos(rad);

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

            double r = radius - 35;
            double x = centerX + r * Math.Sin(rad);
            double y = centerY - r * Math.Cos(rad);

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
