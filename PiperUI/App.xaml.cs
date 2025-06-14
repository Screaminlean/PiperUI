using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PiperUI.Interfaces;
using PiperUI.Services;
using PiperUI.ViewModels.Pages;
using PiperUI.ViewModels.Windows;
using PiperUI.Views.Pages;
using PiperUI.Views.Windows;
using System.IO;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.DependencyInjection;

namespace PiperUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private IConfigurationService _configurationService = null!;

        public App()
        {
            _configurationService = (IConfigurationService)Services.GetService(typeof(IConfigurationService));
            _configurationService?.LoadUserConfiguration();
        }

        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
            .ConfigureServices((context, services) =>
            {
                services.AddNavigationViewPageProvider();

                services.AddHostedService<ApplicationHostService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Register DownloaderService as a singleton for IDownloader
                services.AddSingleton<IDownloaderService, DownloaderService>();
                // Register ConfigurationService as a singleton for IConfigurationService
                services.AddSingleton<IConfigurationService, ConfigurationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<InfoPage>();
                services.AddSingleton<InfoViewModel>();


            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            // Set theme from user config before showing any UI
            
            var userConfig = _configurationService?.UserConfiguration as System.Text.Json.Nodes.JsonObject;
            if (userConfig != null && userConfig["Theme"] != null)
            {
                if (Enum.TryParse(userConfig["Theme"].ToString(), out Wpf.Ui.Appearance.ApplicationTheme theme))
                {
                    ApplicationThemeManager.Apply(theme);
                }
            }
            await _host.StartAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
