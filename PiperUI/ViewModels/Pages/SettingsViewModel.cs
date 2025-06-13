using Microsoft.Win32;
using PiperUI.Helpers;
using PiperUI.Interfaces;
using System.IO;
using System.Text.Json.Nodes;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace PiperUI.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private readonly IConfigurationService _configurationService;
        private bool _isInitialized = false;
        private ApplicationTheme _originalTheme = ApplicationTheme.Unknown;
        private JsonObject _appConfig;
        private JsonObject _userConfig;

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
        }

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            _configurationService.LoadUserConfiguration();
            LoadUserSettings();
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            _isInitialized = true;
        }

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

        private void SaveSettings()
        {
            SaveApplicationSettings();
            SaveUserSettings();
            IsDirty = false;
        }

        private void SaveApplicationSettings()
        {
            _appConfig["PiperDownloadUrl"] = PiperDownloadUrl;
            _appConfig["VoicesDownloadUrl"] = VoicesDownloadUrl;
            _configurationService.ApplicationConfiguration = _appConfig;
            _configurationService.SaveApplicationConfiguration();
        }

        private void SaveUserSettings()
        {
            _userConfig["Theme"] = CurrentTheme.ToString();
            _userConfig["CustomVoicesDirectory"] = CustomVoicesDir;
            _userConfig["OutputDirectory"] = OutputDir;
            _configurationService.UserConfiguration = _userConfig;
            _configurationService.SaveUserConfiguration();
        }

        [RelayCommand]
        private void SaveSettingsCommand()
        {
            SaveSettings();
        }

        partial void OnCurrentThemeChanged(ApplicationTheme value)
        {
            IsDirty = value != _originalTheme;
        }

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

        [RelayCommand]
        private void SelectDirectory(string propertyName)
        {
            
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
                
                if (property != null && property.CanWrite)
                {
                    property.SetValue(this, folderDialog.FolderName);
                }
            }
        }
    }
}
