using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Rescale.Controls;

/// <summary>Flyout grid for selecting a preset icon from categorized glyph groups.</summary>
public partial class IconPickerControl : UserControl
{
    public static readonly DependencyProperty SelectedIconProperty =
        DependencyProperty.Register(nameof(SelectedIcon), typeof(string),
            typeof(IconPickerControl),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly RoutedEvent IconSelectedEvent =
        EventManager.RegisterRoutedEvent(nameof(IconSelected), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(IconPickerControl));

    public static readonly RoutedEvent CloseRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(CloseRequested), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(IconPickerControl));

    public string SelectedIcon
    {
        get => (string)GetValue(SelectedIconProperty);
        set => SetValue(SelectedIconProperty, value);
    }

    public event RoutedEventHandler IconSelected
    {
        add => AddHandler(IconSelectedEvent, value);
        remove => RemoveHandler(IconSelectedEvent, value);
    }

    public event RoutedEventHandler CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    private static readonly (string name, SymbolRegular symbol)[] ActivityIcons =
    [
        ("Game", SymbolRegular.XboxController24),
        ("Briefcase", SymbolRegular.Briefcase24),
        ("Film", SymbolRegular.MoviesAndTv24),
        ("Live", SymbolRegular.Live24),
        ("Photo", SymbolRegular.Image24),
        ("Code", SymbolRegular.Code24),
    ];

    private static readonly (string name, SymbolRegular symbol)[] GeneralIcons =
    [
        ("Monitor", SymbolRegular.Desktop24),
        ("Desktop", SymbolRegular.DesktopMac24),
        ("Present", SymbolRegular.Presenter24),
        ("Moon", SymbolRegular.WeatherMoon24),
        ("Sun", SymbolRegular.WeatherSunny24),
        ("Home", SymbolRegular.Home24),
    ];

    private static readonly (string name, SymbolRegular symbol)[] SymbolIcons =
    [
        ("Star", SymbolRegular.Star24),
        ("Bolt", SymbolRegular.Flash24),
        ("Eye", SymbolRegular.Eye24),
        ("Gear", SymbolRegular.Settings24),
        ("Palette", SymbolRegular.PaintBrush24),
        ("Pin", SymbolRegular.Pin24),
    ];

    public IconPickerControl()
    {
        InitializeComponent();
        PopulateIcons(ActivityPanel, ActivityIcons);
        PopulateIcons(GeneralPanel, GeneralIcons);
        PopulateIcons(SymbolsPanel, SymbolIcons);
    }

    private void PopulateIcons(WrapPanel panel, (string name, SymbolRegular symbol)[] icons)
    {
        foreach (var (name, symbol) in icons)
        {
            var btn = new Border
            {
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(6),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(2),
                Tag = name,
                Child = new SymbolIcon { Symbol = symbol, FontSize = 18, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
            };

            btn.MouseEnter += (s, _) =>
            {
                if (s is Border b)
                    b.Background = (Brush)FindResource("ControlFillColorDefaultBrush");
            };
            btn.MouseLeave += (s, _) =>
            {
                if (s is Border b && b.Tag is string n && n != SelectedIcon)
                    b.Background = Brushes.Transparent;
            };
            btn.MouseLeftButtonDown += (s, _) =>
            {
                if (s is Border b && b.Tag is string n)
                {
                    SelectedIcon = n;
                    RaiseEvent(new RoutedEventArgs(IconSelectedEvent));
                }
            };

            panel.Children.Add(btn);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
    }
}
