using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rescale.ViewModels;

namespace Rescale.Views;

/// <summary>Page for application settings: theme, autostart, and cycle hotkey.</summary>
public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    private void HotkeyTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.IsRecordingHotkey = true;
        HotkeyTextBox.Text = "Press a key combo...";
    }

    private void HotkeyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.IsRecordingHotkey = false;
            if (string.IsNullOrEmpty(vm.CycleHotkey))
                HotkeyTextBox.Text = "Click to set";
        }
    }

    private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin)
            return;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.CycleHotkey = null;
                HotkeyTextBox.Text = "Click to set";
            }
            Keyboard.ClearFocus();
            return;
        }

        var modifiers = Keyboard.Modifiers;
        if (modifiers == ModifierKeys.None)
            return;

        var parts = new System.Collections.Generic.List<string>();
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        parts.Add(key.ToString());

        var hotkey = string.Join("+", parts);

        if (DataContext is SettingsViewModel settingsVm)
        {
            settingsVm.CycleHotkey = hotkey;
            HotkeyTextBox.Text = hotkey;
        }

        Keyboard.ClearFocus();
    }
}
