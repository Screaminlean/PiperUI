using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using PiperUI.Helpers;
using PiperUI.Interfaces;

namespace PiperUI.ViewModels.Pages
{
    // ViewModel for the Info/Dashboard page, manages Piper and voice data, and user selections
    public partial class DashboardViewModel : ObservableObject
    {
        // Services for configuration and downloading
        private readonly IConfigurationService _configurationService;
        private readonly IDownloaderService _downloaderService;

        // Constructor: injects configuration and downloader services
        public DashboardViewModel(IConfigurationService configuration, IDownloaderService downloader)
        {
            _configurationService = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _downloaderService = downloader ?? throw new ArgumentNullException(nameof(downloader));
        }

        // Observable properties for UI binding
        [ObservableProperty]
        private bool _isLoadingVoiceData = false;

        [ObservableProperty]
        private string _statusText = "Loading"; // Status text to display in the UI

        [ObservableProperty]
        private Dictionary<string, JsonElement> _voiceData = new Dictionary<string, JsonElement>();

        [ObservableProperty]
        private ObservableCollection<string> _languages = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedLanguage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _voices = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedVoice = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _qualities = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedQuality = string.Empty;

        [ObservableProperty]
        private double _playbackSpeed = 1.0;

        // Update playback speed description when changing speed
        partial void OnPlaybackSpeedChanging(double value)
        {
            SetPlaybackSpeeDescriptiondAsync().ConfigureAwait(false);
        }

        [ObservableProperty]
        private string _playbackSpeedDescription = "Normal (1.0x)";

        [ObservableProperty]
        private string _promtText = string.Empty;

        // Command: Initialize the view model, download Piper if needed, and load voice data
        [RelayCommand]
        public async Task InitializeAsync()
        {
            string piperExecutable = Path.Combine(HelperMethods.appDataDir, "piper", "piper.exe");
            // Check if the directory exists and if the executable file exists
            if (!Directory.Exists(Path.GetDirectoryName(piperExecutable)) || !File.Exists(piperExecutable))
            {
                StatusText = "Downloading Piper...";
                Trace.WriteLine($"The Piper executable does not exist at {piperExecutable}. Downloading Piper.", piperExecutable);
                await DownloadPiperAsync(); // Download and extract Piper if the directory does not exist
            }

            await LoadVoiceDataAsync(); // Load voice data when the view model is initialized

            StatusText = "Loaded....:)";
        }

        // Downloads and extracts Piper if not present
        private async Task DownloadPiperAsync()
        {
            // Get the Piper download URL from ApplicationConfiguration
            string url = _configurationService.ApplicationConfiguration?["PiperDownloadUrl"]?.ToString() ??
                HelperMethods.piperDownloadUrl;
            string zipFileName = url.Split("/").Last(); // Extract the file name from the URL
            string filePath = Path.Combine(HelperMethods.appDataDir, zipFileName); // Path for the downloaded zip
            try
            {
                StatusText = "Downloading Piper...";
                Trace.WriteLine("Starting download of Piper from: " + url);
                bool downloadSuccess = await _downloaderService.DownloadFileAsync(url, HelperMethods.appDataDir, zipFileName);
                if (downloadSuccess)
                {
                    StatusText = "Piper downloaded successfully.";
                    Trace.WriteLine("Piper downloaded successfully.");
                    ZipFile.ExtractToDirectory(filePath, HelperMethods.appDataDir, true);
                    File.Delete(filePath); // Delete the zip file after extraction
                    Trace.WriteLine("Piper extracted successfully.");
                }
                else
                {
                    StatusText = "Failed to download Piper.";
                    Trace.WriteLine("Failed to download Piper from: " + url);
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error downloading Piper: {ex.Message}";
                Trace.WriteLine($"Error downloading Piper: {ex.Message}");
            }
        }

        // Determines if voice data can be loaded (always true for now)
        private bool CanLoadVoiceData()
        {
            return true;
        }

        // Command: Load voice data, download voices.json if missing
        [RelayCommand(CanExecute = nameof(CanLoadVoiceData))]
        public async Task LoadVoiceDataAsync()
        {
            IsLoadingVoiceData = true;

            try
            {
                string fileName = Path.Combine(HelperMethods.appDataDir, "voices.json");
                if (!File.Exists(fileName))
                {
                    StatusText = "Downloading voice data...";
                    Trace.WriteLine("The voices.json file does not exist.", fileName);
                    // Get the voices download URL from ApplicationConfiguration
                    string url = _configurationService.ApplicationConfiguration?["VoicesDownloadUrl"]?.ToString() ??
                        HelperMethods.voicesDownloadUrl;
                    var downloadSuccess = await _downloaderService.DownloadFileAsync(url, HelperMethods.appDataDir, "voices.json");
                    if (!downloadSuccess || !File.Exists(fileName))
                    {
                        StatusText = "Failed to download voice data.";
                        Trace.WriteLine("Failed to download voices.json or file still does not exist.", fileName);
                        IsLoadingVoiceData = false;
                        return;
                    }

                    StatusText = "Voice data downloaded successfully.";
                }

                using FileStream openStream = File.OpenRead(fileName);
                if (openStream.Length == 0)
                {
                    StatusText = "The voices.json file is empty or not found.";
                    Trace.WriteLine("The voices.json file is empty or not found.", fileName);
                    IsLoadingVoiceData = false;
                    return;
                }

                VoiceData = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(openStream);

                if (VoiceData == null || VoiceData.Count == 0)
                {
                    StatusText = "No voice data available.";
                    Trace.WriteLine("The voices.json file is empty or not found.", fileName);
                    IsLoadingVoiceData = false;
                    return;
                }

                // Populate the Languages collection from the voice data
                foreach (var voiceKey in VoiceData.Keys)
                {
                    var languageElement = VoiceData[voiceKey].GetProperty("language");
                    var languageName = languageElement.GetProperty("name_native").GetString();
                    if (languageName is not null && !Languages.Contains(languageName))
                    {
                        Languages.Add(languageName);
                    }
                }

                if (Languages is not null && Languages.Count > 0)
                {
                    SelectedLanguage = Languages.FirstOrDefault("");
                }
                else
                {
                    StatusText = "No languages found in the voice data.";
                    Trace.WriteLine("No languages found in the voices.json file.", fileName);
                }

            }
            catch (Exception ex)
            {
                StatusText = $"Error loading voice data: {ex.Message}";
                // Handle exceptions (e.g., file not found, JSON parsing errors)
                Console.WriteLine($"Error loading voice data: {ex.Message}");
            }
            finally
            {
                IsLoadingVoiceData = false;
                await LoadVoicesAsync(); // Load voices based on the selected language
            }
        }

        // Determines if voices can be loaded (not loading and language selected)
        private bool CanLoadVoices()
        {
            if (!IsLoadingVoiceData || !string.IsNullOrWhiteSpace(SelectedLanguage))
            {                 
                return true;
            }

            return false;
        }

        // Command: Load voices for the selected language
        [RelayCommand(CanExecute = nameof(CanLoadVoices))]
        public async Task LoadVoicesAsync()
        {
            try
            {
                StatusText = "Loading voices...";
                Voices.Clear();
                if (VoiceData is not null && VoiceData.Count > 0)
                {
                    foreach (var voiceKey in VoiceData.Keys)
                    {
                        var languageElement = VoiceData[voiceKey].GetProperty("language");
                        var languageName = languageElement.GetProperty("name_native").GetString();
                        var voiceName = VoiceData[voiceKey].GetProperty("name").GetString();
                        if (languageName == SelectedLanguage && !Voices.Contains(voiceName))
                        {
                            Voices.Add(voiceName);
                        }
                    }
                    SelectedVoice = Voices.FirstOrDefault("");
                    await LoadQualitiesAsync(); // Load qualities after loading voices
                }
                else
                {
                    StatusText = "No voice data available to load voices.";
                    Trace.WriteLine("No voice data available to load voices.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading voices: {ex.Message}");
            }
        }

        // Command: Load qualities for the selected voice
        [RelayCommand]
        public async Task LoadQualitiesAsync()
        {
            Qualities.Clear();

            foreach (var voiceKey in VoiceData.Keys)
            {
                var voiceName = VoiceData[voiceKey].GetProperty("name").GetString();
                var quality = VoiceData[voiceKey].GetProperty("quality").GetString();

                if (string.IsNullOrWhiteSpace(quality))
                {
                    continue;
                }

                if (voiceName == SelectedVoice && !Qualities.Contains(quality))
                {
                    Qualities.Add(quality);
                }
            }

            SelectedQuality = Qualities.FirstOrDefault("");
            await Task.Run(()=> Trace.WriteLine($"Loaded {Qualities.Count} qualities for the selected voice: {SelectedVoice}"));
        }

        // Command: Update playback speed description based on the current speed
        [RelayCommand]
        public async Task SetPlaybackSpeeDescriptiondAsync()
        {
            double speed = Math.Round(PlaybackSpeed, 1);
            string speedDescription = speed == 1 ? "Normal" : speed < 1 ? "Fast" : "Slow";
            PlaybackSpeedDescription = $"{speedDescription} ({speed:F1}x)";

            await Task.Run(()=> Trace.WriteLine($"Playback speed set to {speed:F1} - {PlaybackSpeedDescription}."));
        }

        // Determines if the Generate command can execute
        private bool CanGenerate()
        {
            // Enable if not loading voice data and all required selections are made
            if (!IsLoadingVoiceData || 
                !string.IsNullOrWhiteSpace(SelectedLanguage) || 
                !string.IsNullOrWhiteSpace(SelectedVoice) || 
                !string.IsNullOrWhiteSpace(PromtText))
            {
               return true;
            }

            return false;
        }

        // Command: Generate output (placeholder for future implementation)
        [RelayCommand(CanExecute = nameof(CanGenerate))]
        public async Task GenerateAsync()
        {
            string promt = await Task.Run(() => HelperMethods.CleanString(PromtText));
            await Task.Run(()=>Trace.WriteLine("Generate command executed."));
        }

        // Determines if the prompt text can be cleared
        private bool CanClearPromptText()
        {
            return !string.IsNullOrWhiteSpace(PromtText);
        }

        // Command: Clear the prompt text
        [RelayCommand(CanExecute = nameof(CanClearPromptText))]
        public async Task ClearPromptTextAsync()
        {
            PromtText = string.Empty;
            await Task.Run(()=> Trace.WriteLine("Prompt text cleared."));
        }
    }
}
