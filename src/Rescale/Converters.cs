using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rescale;

/// <summary>Converts null to <see cref="Visibility.Collapsed"/> and non-null to <see cref="Visibility.Visible"/>.</summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value != null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Converts true/non-zero to <see cref="Visibility.Collapsed"/> and false/zero to <see cref="Visibility.Visible"/>.</summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        if (value is bool b)
            return b ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Returns Primary appearance when the bound theme matches the parameter, Secondary otherwise.</summary>
public class ThemeButtonAppearanceConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var current = value as string ?? "";
        var target = parameter as string ?? "";
        return string.Equals(current, target, StringComparison.OrdinalIgnoreCase)
            ? Wpf.Ui.Controls.ControlAppearance.Primary
            : Wpf.Ui.Controls.ControlAppearance.Secondary;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
