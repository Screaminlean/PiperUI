using Microsoft.Win32;
using PiperUI.Helpers;
using PiperUI.Interfaces;
using System.IO;
using System.Text.Json.Nodes;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using System.Windows;
using PiperUI.Views.Pages;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using PiperUI.Messages;

namespace PiperUI.ViewModels.Pages
{
    // ViewModel for the Settings page, handles theme, directory, and configuration management
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        // Services and state fields
        private readonly IConfigurationService _configurationService;
        private bool _isInitialized = false;
        private ApplicationTheme _originalTheme = ApplicationTheme.Unknown;
        private JsonObject _appConfig;
        private JsonObject _userConfig;

        // Observable properties for binding to the UI
        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty]
        private bool _isDirty = false;

        [ObservableProperty]
        private string _piperDownloadUrl = HelperMethods.piperDownloadUrl;

        [ObservableProperty]
        private string _voicesDownloadUrl = HelperMethods.voicesDownloadUrl;

        [ObservableProperty]
        private string _userDataDir = HelperMethods.userDataDir;

        [ObservableProperty]
        private string _appDataDir = HelperMethods.appDataDir;

        [ObservableProperty]
        private string _customVoicesDir = HelperMethods.CustomVoicesDirectory;

        [ObservableProperty]
        private string _outputDir = HelperMethods.OutputDirectory;

        // Constructor: initializes configuration and loads settings
        public SettingsViewModel(IConfigurationService configuration)
        {
            _configurationService = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Set download URLs and directories from application configuration if available
            _appConfig = _configurationService.ApplicationConfiguration as JsonObject ?? new JsonObject();
            _userConfig = _configurationService.UserConfiguration as JsonObject ?? new JsonObject();
            PiperDownloadUrl = _appConfig?["PiperDownloadUrl"]?.ToString() ?? HelperMethods.piperDownloadUrl;
            VoicesDownloadUrl = _appConfig?["VoicesDownloadUrl"]?.ToString() ?? HelperMethods.voicesDownloadUrl;
            CustomVoicesDir = _userConfig?["CustomVoicesDirectory"]?.ToString() ?? HelperMethods.CustomVoicesDirectory;
            OutputDir = _userConfig?["OutputDirectory"]?.ToString() ?? HelperMethods.OutputDirectory;
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
        }

        // Called when navigated to the settings page
        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        // Called when navigated away from the settings page
        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        // Initializes the ViewModel state and loads user settings
        private void InitializeViewModel()
        {
            _configurationService.LoadUserConfiguration();
            LoadUserSettings();
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            _isInitialized = true;
        }

        // Loads user settings from configuration, sets defaults if missing
        private void LoadUserSettings()
        {
            if (_userConfig is not null && _userConfig["Theme"] is JsonNode themeNode && Enum.TryParse(themeNode.ToString(), out ApplicationTheme theme))
            {
                CurrentTheme = theme;
                _originalTheme = theme;
            }
            else
            {
                CurrentTheme = ApplicationTheme.Dark;
                _originalTheme = ApplicationTheme.Dark;
                SaveSettings(); // Ensure default settings are saved
            }
            IsDirty = false;
        }

        // Saves both application and user settings
        private void SaveSettings()
        {
            SaveApplicationSettings();
            SaveUserSettings();
            IsDirty = false;
        }

        // Saves application-level settings (download URLs)
        private void SaveApplicationSettings()
        {
            _appConfig["PiperDownloadUrl"] = PiperDownloadUrl;
            _appConfig["VoicesDownloadUrl"] = VoicesDownloadUrl;
            _configurationService.ApplicationConfiguration = _appConfig;
            _configurationService.SaveApplicationConfiguration();
        }

        // Saves user-level settings (theme, directories)
        private void SaveUserSettings()
        {
            _userConfig["Theme"] = CurrentTheme.ToString();
            _userConfig["CustomVoicesDirectory"] = CustomVoicesDir;
            _userConfig["OutputDirectory"] = OutputDir;
            _configurationService.UserConfiguration = _userConfig;
            _configurationService.SaveUserConfiguration();
        }

        // Command: Save settings when triggered from the UI
        [RelayCommand]
        private void SaveSettingsCommand()
        {
            bool success = false;
            try
            {
                SaveSettings();
                success = true;
            }
            catch
            {
                success = false;
            }

            // Send snackbar message via Messenger with timeout
            var message = success ? "Settings saved successfully!" : "Failed to save settings.";
            var appearance = success ? Wpf.Ui.Controls.ControlAppearance.Success : Wpf.Ui.Controls.ControlAppearance.Danger;
            WeakReferenceMessenger.Default.Send(new SnackbarMessage(message, appearance));
        }

        // Helper to find the SettingsPage in the visual tree
        private SettingsPage FindSettingsPage(DependencyObject parent)
        {
            if (parent is SettingsPage page)
                return page;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                var result = FindSettingsPage(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        // Called when the theme changes, updates dirty state
        partial void OnCurrentThemeChanged(ApplicationTheme value)
        {
            IsDirty = value != _originalTheme;
        }

        // Command: Change the application theme based on user selection
        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;
                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;
                    break;
                case "theme_dark":
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;
                    break;
                case "theme_highcontrast":
                    if (CurrentTheme == ApplicationTheme.HighContrast)
                        break;
                    ApplicationThemeManager.Apply(ApplicationTheme.HighContrast);
                    CurrentTheme = ApplicationTheme.HighContrast;
                    break;
                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;
                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;
                    break;
            }
        }

        // Command: Open a directory in the file explorer
        [RelayCommand]
        private void OpenDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = directory,
                        UseShellExecute = true
                    });
                }
                else
                {
                    throw new DirectoryNotFoundException($"The directory '{directory}' does not exist.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them or show a message to the user)
                Console.WriteLine($"Error opening directory: {ex.Message}");
            }
        }

        // Command: Show a folder picker dialog and set the selected directory property
        [RelayCommand]
        private void SelectDirectory(string propertyName)
        {
            // Use reflection to get the property by name
            var property = GetType().GetProperty(propertyName);

            if (property == null) 
            { 
                return;
            }

            var folderDialog = new OpenFolderDialog
            {
                Title = $"Select {propertyName} Folder",
                InitialDirectory = property.GetValue(this)?.ToString() ?? string.Empty,
            };

            if (folderDialog.ShowDialog() == true)
            {
                // Set the property value if writable
                if (property != null && property.CanWrite)
                {
                    property.SetValue(this, folderDialog.FolderName);
                }
            }
        }
    }
}
