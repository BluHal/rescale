using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rescale.Controls;

/// <summary>Flyout that captures keyboard input to record a hotkey combination.</summary>
public partial class HotkeyPickerControl : UserControl
{
    public static readonly DependencyProperty HotkeyProperty =
        DependencyProperty.Register(nameof(Hotkey), typeof(string),
            typeof(HotkeyPickerControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly RoutedEvent HotkeySavedEvent =
        EventManager.RegisterRoutedEvent(nameof(HotkeySaved), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(HotkeyPickerControl));

    public static readonly RoutedEvent CloseRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(CloseRequested), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(HotkeyPickerControl));

    public string? Hotkey
    {
        get => (string?)GetValue(HotkeyProperty);
        set => SetValue(HotkeyProperty, value);
    }

    public event RoutedEventHandler HotkeySaved
    {
        add => AddHandler(HotkeySavedEvent, value);
        remove => RemoveHandler(HotkeySavedEvent, value);
    }

    public event RoutedEventHandler CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    private readonly HashSet<Key> _pressedModifiers = [];
    private Key _mainKey = Key.None;

    public HotkeyPickerControl()
    {
        InitializeComponent();
        Loaded += (_, _) => Focus();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (e.Key == Key.Escape)
        {
            RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            _pressedModifiers.Add(key);
            UpdateDisplay();
            return;
        }

        _mainKey = key;
        UpdateDisplay();
        SaveButton.IsEnabled = true;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        _pressedModifiers.Remove(key);
    }

    private void UpdateDisplay()
    {
        var parts = new List<string>();
        if (_pressedModifiers.Any(k => k is Key.LeftCtrl or Key.RightCtrl)) parts.Add("Ctrl");
        if (_pressedModifiers.Any(k => k is Key.LeftAlt or Key.RightAlt)) parts.Add("Alt");
        if (_pressedModifiers.Any(k => k is Key.LeftShift or Key.RightShift)) parts.Add("Shift");
        if (_pressedModifiers.Any(k => k is Key.LWin or Key.RWin)) parts.Add("Win");

        if (_mainKey != Key.None)
            parts.Add(_mainKey.ToString());

        RecordingText.Text = parts.Count > 0 ? string.Join(" + ", parts) : "Press a key combination...";

        ModifierPanel.Children.Clear();
        foreach (var mod in parts.Where(p => p is "Ctrl" or "Alt" or "Shift" or "Win"))
        {
            var badge = new TextBlock
            {
                Text = mod,
                FontFamily = new System.Windows.Media.FontFamily("Cascadia Mono"),
                FontSize = 12,
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(0, 0, 6, 0),
            };
            ModifierPanel.Children.Add(badge);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var parts = new List<string>();
        if (_pressedModifiers.Any(k => k is Key.LeftCtrl or Key.RightCtrl)) parts.Add("Ctrl");
        if (_pressedModifiers.Any(k => k is Key.LeftAlt or Key.RightAlt)) parts.Add("Alt");
        if (_pressedModifiers.Any(k => k is Key.LeftShift or Key.RightShift)) parts.Add("Shift");
        if (_mainKey != Key.None) parts.Add(_mainKey.ToString());

        Hotkey = string.Join("+", parts);
        RaiseEvent(new RoutedEventArgs(HotkeySavedEvent));
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _pressedModifiers.Clear();
        _mainKey = Key.None;
        SaveButton.IsEnabled = false;
        RecordingText.Text = "Press a key combination...";
        ModifierPanel.Children.Clear();
        Hotkey = null;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CloseRequestedEvent));
    }
}
