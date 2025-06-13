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
        private string _applicationTitle = HelperMethods.AppName;

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
            },
            new NavigationViewItem()
            {
                Content = "Info",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Info24 },
                TargetPageType = typeof(Views.Pages.InfoPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<Wpf.Ui.Controls.MenuItem> _trayMenuItems = new()
        {
            new Wpf.Ui.Controls.MenuItem { Header = "Home", Tag = "tray_home" }
        };

        public MainWindowViewModel(IConfigurationService configuration, IDownloaderService downloader)
        {
            
        }
        
    }
}
