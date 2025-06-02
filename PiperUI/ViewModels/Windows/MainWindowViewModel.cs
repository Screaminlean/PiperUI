using System.Collections.ObjectModel;
using System.IO;
using PiperUI.Helpers;
using PiperUI.Interfaces;
using Wpf.Ui.Controls;

namespace PiperUI.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WinPiper";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Piper",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(Views.Pages.DashboardPage)
            },
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<Wpf.Ui.Controls.MenuItem> _trayMenuItems = new()
        {
            new Wpf.Ui.Controls.MenuItem { Header = "Home", Tag = "tray_home" }
        };

        public MainWindowViewModel(IConfigurationService configuration, IDownloaderService downloader)
        {
            //_downloader = downloader;
            // Initialize the application data folder for storing models or output files
            string modelsFolder = Path.Combine(HelperMethods.AppDataPath, "models"); // Folder for models
            string outputFolder = Path.Combine(HelperMethods.AppDataPath, "output"); // Folder for output files
            string customFolder = Path.Combine(HelperMethods.AppDataPath, "custom"); // Folder for custom models
            // Create the directories if they do not exist
            Directory.CreateDirectory(modelsFolder); // Create models folder
            Directory.CreateDirectory(outputFolder); // Create output folder
            Directory.CreateDirectory(customFolder); // Create custom folder
        }
        
    }
}
