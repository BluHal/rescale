using System.ComponentModel;
using System.Windows;
using Rescale.ViewModels;
using Rescale.Views;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Rescale;

/// <summary>Main application window with NavigationView sidebar and minimize-to-tray behavior.</summary>
public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();

        NavigationView.SetServiceProvider(new ViewServiceProvider(viewModel));
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        NavigationView.Navigate(typeof(PresetListPage));
    }

    /// <summary>Intercepts window close to minimize to tray instead of exiting.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}

/// <summary>Simple service provider that creates page instances with their required view models.</summary>
internal sealed class ViewServiceProvider(MainViewModel mainVm) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(PresetListPage))
            return new PresetListPage(mainVm);
        if (serviceType == typeof(SettingsPage))
            return new SettingsPage(mainVm.SettingsVm);
        return null;
    }
}
