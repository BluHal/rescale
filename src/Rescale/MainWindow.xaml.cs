using System.ComponentModel;
using System.Windows;
using Rescale.Services;
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
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Loaded += OnLoaded;
        LogService.Info("MainWindow created");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            NavigationView.Navigate(typeof(PresetListPage));
            LogService.Info("Initial navigation to PresetListPage");
        }
        catch (Exception ex)
        {
            LogService.Error("Failed initial navigation", ex);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.IsDetailView)) return;

        try
        {
            if (ViewModel.IsDetailView)
            {
                LogService.Info($"Navigating to PresetDetailPage, PresetDetailVm is {(ViewModel.PresetDetailVm != null ? "set" : "NULL")}");
                NavigationView.Navigate(typeof(PresetDetailPage));
            }
            else
            {
                LogService.Info("Navigating back to PresetListPage");
                NavigationView.Navigate(typeof(PresetListPage));
            }
        }
        catch (Exception ex)
        {
            LogService.Error("Navigation failed", ex);
        }
    }

    /// <summary>Intercepts window close to minimize to tray instead of exiting. Shift+click closes for real.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            return;

        e.Cancel = true;
        Hide();
    }
}

/// <summary>Simple service provider that creates page instances with their required view models.</summary>
internal sealed class ViewServiceProvider(MainViewModel mainVm) : IServiceProvider
{
    public object? GetService(Type serviceType)
    {
        try
        {
            LogService.Info($"GetService requested: {serviceType.Name}");

            if (serviceType == typeof(PresetListPage))
                return new PresetListPage(mainVm);

            if (serviceType == typeof(PresetDetailPage))
            {
                if (mainVm.PresetDetailVm == null)
                {
                    LogService.Error("PresetDetailVm is null when trying to create PresetDetailPage");
                    return null;
                }
                return new PresetDetailPage(mainVm.PresetDetailVm);
            }

            if (serviceType == typeof(SettingsPage))
                return new SettingsPage(mainVm.SettingsVm);

            LogService.Info($"No handler for {serviceType.Name}");
            return null;
        }
        catch (Exception ex)
        {
            LogService.Error($"GetService failed for {serviceType.Name}", ex);
            return null;
        }
    }
}
