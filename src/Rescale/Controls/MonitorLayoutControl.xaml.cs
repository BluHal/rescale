using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Rescale.Models;

namespace Rescale.Controls;

/// <summary>Spatial preview of monitor arrangement, with selection and disconnected-monitor indicators.</summary>
public partial class MonitorLayoutControl : UserControl
{
    public static readonly DependencyProperty MonitorsProperty =
        DependencyProperty.Register(nameof(Monitors), typeof(ObservableCollection<MonitorConfig>),
            typeof(MonitorLayoutControl), new PropertyMetadata(null, OnMonitorsChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
            typeof(MonitorLayoutControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

    public static readonly DependencyProperty ConnectedDevicePathsProperty =
        DependencyProperty.Register(nameof(ConnectedDevicePaths), typeof(HashSet<string>),
            typeof(MonitorLayoutControl), new PropertyMetadata(null, OnMonitorsChanged));

    public ObservableCollection<MonitorConfig>? Monitors
    {
        get => (ObservableCollection<MonitorConfig>?)GetValue(MonitorsProperty);
        set => SetValue(MonitorsProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public HashSet<string>? ConnectedDevicePaths
    {
        get => (HashSet<string>?)GetValue(ConnectedDevicePathsProperty);
        set => SetValue(ConnectedDevicePathsProperty, value);
    }

    public MonitorLayoutControl()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Render();
    }

    private static void OnMonitorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((MonitorLayoutControl)d).Render();

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((MonitorLayoutControl)d).Render();

    private void Render()
    {
        MonitorCanvas.Children.Clear();
        var monitors = Monitors;
        if (monitors == null || monitors.Count == 0) return;

        double canvasW = MonitorCanvas.ActualWidth;
        double canvasH = MonitorCanvas.ActualHeight;
        if (canvasW <= 0 || canvasH <= 0) return;

        double totalW = monitors.Max(m => m.Width);
        double totalH = monitors.Max(m => m.Height);
        double totalMonW = monitors.Sum(m => m.Width);

        double pad = 16;
        double availW = canvasW - pad * 2;
        double availH = canvasH - pad * 2;

        double scale = Math.Min(availW / totalMonW, availH / totalH) * 0.85;
        double offsetX = pad;

        for (int i = 0; i < monitors.Count; i++)
        {
            var mon = monitors[i];
            double w = mon.Width * scale;
            double h = mon.Height * scale;
            double x = offsetX;
            double y = pad + (availH - h) / 2;

            bool selected = i == SelectedIndex;
            var accentColor = (Color)ColorConverter.ConvertFromString("#0067C0");

            bool connected = ConnectedDevicePaths == null
                || string.IsNullOrEmpty(mon.DevicePath)
                || ConnectedDevicePaths.Contains(mon.DevicePath);

            var border = new Border
            {
                Width = w,
                Height = h,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(selected ? 2 : 1.5),
                BorderBrush = selected
                    ? new SolidColorBrush(accentColor)
                    : (Brush)FindResource("ControlStrokeColorDefaultBrush"),
                Background = connected
                    ? (Brush)FindResource("CardBackgroundFillColorDefaultBrush")
                    : new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80)),
                Cursor = Cursors.Hand,
                Tag = i,
                Opacity = connected ? 1.0 : 0.55,
            };

            border.MouseLeftButtonDown += (s, _) =>
            {
                if (s is Border b && b.Tag is int idx)
                    SelectedIndex = idx;
            };

            var stack = new StackPanel { Margin = new Thickness(8) };

            var numText = new TextBlock
            {
                Text = (i + 1).ToString(),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = selected
                    ? new SolidColorBrush(accentColor)
                    : (Brush)FindResource("TextFillColorPrimaryBrush"),
            };
            stack.Children.Add(numText);

            if (h > 60)
            {
                var nameText = new TextBlock
                {
                    Text = $"{mon.Width}x{mon.Height}",
                    FontSize = 10,
                    Foreground = (Brush)FindResource("TextFillColorSecondaryBrush"),
                    Margin = new Thickness(0, 4, 0, 0),
                };
                stack.Children.Add(nameText);

                if (!connected)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = "Disconnected",
                        FontSize = 9,
                        FontStyle = FontStyles.Italic,
                        Foreground = new SolidColorBrush(Colors.Gray),
                        Margin = new Thickness(0, 2, 0, 0),
                    });
                }
            }

            border.Child = stack;

            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y);
            MonitorCanvas.Children.Add(border);

            offsetX += w + 10;
        }
    }
}
