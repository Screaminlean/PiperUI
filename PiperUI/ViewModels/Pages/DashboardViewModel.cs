using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using PiperUI.Helpers;
using PiperUI.Interfaces;

namespace PiperUI.ViewModels.Pages
{
    public partial class InfoViewModel : ObservableObject
    {
        private readonly IConfigurationService _configurationService;

        private readonly IDownloaderService _downloaderService;

        public InfoViewModel(IConfigurationService configuration, IDownloaderService downloader)
        {
            _configurationService = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _downloaderService = downloader ?? throw new ArgumentNullException(nameof(downloader));
        }

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

        partial void OnPlaybackSpeedChanging(double value)
        {
            SetPlaybackSpeeDescriptiondAsync().ConfigureAwait(false); // Update playback speed description when changing speed
        }

        [ObservableProperty]
        private string _playbackSpeedDescription = "Normal (1.0x)";

        [ObservableProperty]
        private string _promtText = string.Empty;


        [RelayCommand]
        public async Task InitializeAsync()
        {

            string piperExecutable = Path.Combine(HelperMethods.appDataDir, "piper", "piper.exe"); // Path to the piper executable
            // Check if the directory exists and if the executable file exists
            if (!Directory.Exists(Path.GetDirectoryName(piperExecutable)) || !File.Exists(piperExecutable))
            {
                StatusText = "Downloading Piper..."; // Update status text
                Trace.WriteLine($"The Piper executable does not exist at {piperExecutable}. Downloading Piper.", piperExecutable);
                await DownloadPiperAsync(); // Download and extract Piper if the directory does not exist
            }

            await LoadVoiceDataAsync(); // Load voice data when the view model is initialized

            StatusText = "Loaded....:)";
        }

        private async Task DownloadPiperAsync()
        {
            // Get the Piper download URL from ApplicationConfiguration
            string url = _configurationService.ApplicationConfiguration?["PiperDownloadUrl"]?.ToString() ??
                HelperMethods.piperDownloadUrl;
            string zipFileName = url.Split("/").Last(); // Extract the file name from the URL
            string filePath = Path.Combine(HelperMethods.appDataDir, zipFileName); // Ensure the path is correct for the application data folder
            try
            {
                StatusText = "Downloading Piper..."; // Update status text
                Trace.WriteLine("Starting download of Piper from: " + url);
                bool downloadSuccess = await _downloaderService.DownloadFileAsync(url, HelperMethods.appDataDir, zipFileName);
                if (downloadSuccess)
                {
                    StatusText = "Piper downloaded successfully."; // Update status text
                    Trace.WriteLine("Piper downloaded successfully.");
                    ZipFile.ExtractToDirectory(filePath, HelperMethods.appDataDir, true);
                    File.Delete(filePath); // Delete the zip file after extraction
                    Trace.WriteLine("Piper extracted successfully.");
                }
                else
                {
                    StatusText = "Failed to download Piper."; // Update status text
                    Trace.WriteLine("Failed to download Piper from: " + url);
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error downloading Piper: {ex.Message}"; // Update status text
                Trace.WriteLine($"Error downloading Piper: {ex.Message}");
            }
        }

        private bool CanLoadVoiceData()
        {
            return true; // You can add conditions to enable/disable loading voice data
        }

        [RelayCommand(CanExecute = nameof(CanLoadVoiceData))]
        public async Task LoadVoiceDataAsync()
        {
            IsLoadingVoiceData = true; // Set loading state to true

            try
            {
                string fileName = Path.Combine(HelperMethods.appDataDir, "voices.json"); // Ensure the path is correct for the application data folder
                if (!File.Exists(fileName))
                {
                    StatusText = "Downloading voice data..."; // Update status text
                    Trace.WriteLine("The voices.json file does not exist.", fileName);
                    // Get the Piper download URL from ApplicationConfiguration
                    string url = _configurationService.ApplicationConfiguration?["VoicesDownloadUrl"]?.ToString() ??
                        HelperMethods.voicesDownloadUrl;
                    var downloadSuccess = await _downloaderService.DownloadFileAsync(url, HelperMethods.appDataDir, "voices.json"); // Download the file if it does not exist
                    if (!downloadSuccess || !File.Exists(fileName))
                    {
                        StatusText = "Failed to download voice data."; // Update status text
                        Trace.WriteLine("Failed to download voices.json or file still does not exist.", fileName);
                        IsLoadingVoiceData = false;
                        return;
                    }

                    StatusText = "Voice data downloaded successfully."; // Update status text
                }

                using FileStream openStream = File.OpenRead(fileName);
                if (openStream.Length == 0)
                {
                    StatusText = "The voices.json file is empty or not found."; // Update status text
                    Trace.WriteLine("The voices.json file is empty or not found.", fileName);
                    IsLoadingVoiceData = false;
                    return; // Exit if the file is empty
                }

                VoiceData = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(openStream);

                if (VoiceData == null || VoiceData.Count == 0)
                {
                    StatusText = "No voice data available."; // Update status text
                    Trace.WriteLine("The voices.json file is empty or not found.", fileName);
                    IsLoadingVoiceData = false;
                    return; // Exit if the deserialized data is null or empty
                }

                foreach (var voiceKey in VoiceData.Keys)
                {
                    var languageElement = VoiceData[voiceKey].GetProperty("language");
                    var languageName = languageElement.GetProperty("name_native").GetString();
                    if (languageName is not null && !Languages.Contains(languageName)) // Check if the language is not already in the list
                    {
                        Languages.Add(languageName);
                    }
                }

                if (Languages is not null && Languages.Count > 0)
                {
                    SelectedLanguage = Languages.FirstOrDefault(""); // Set the first language as selected by default
                }
                else
                {
                    StatusText = "No languages found in the voice data."; // Update status text
                    Trace.WriteLine("No languages found in the voices.json file.", fileName);
                }

            }
            catch (Exception ex)
            {
                StatusText = $"Error loading voice data: {ex.Message}"; // Update status text
                // Handle exceptions (e.g., file not found, JSON parsing errors)
                Console.WriteLine($"Error loading voice data: {ex.Message}");
            }
            finally
            {
                IsLoadingVoiceData = false; // Set loading state to false
                await LoadVoicesAsync(); // Load voices based on the selected language
            }
        }

        private bool CanLoadVoices()
        {
            if (!IsLoadingVoiceData || !string.IsNullOrWhiteSpace(SelectedLanguage))
            {                 
                return true; // Enable if not loading voice data and a language is selected
            }

            return false; // Disable otherwise
        }

        [RelayCommand(CanExecute = nameof(CanLoadVoices))]
        public async Task LoadVoicesAsync()
        {
            try
            {
                StatusText = "Loading voices..."; // Update status text
                Voices.Clear(); // Clear existing voices
                if (VoiceData is not null && VoiceData.Count > 0)
                {
                    foreach (var voiceKey in VoiceData.Keys)
                    {
                        var languageElement = VoiceData[voiceKey].GetProperty("language");
                        var languageName = languageElement.GetProperty("name_native").GetString();
                        var voiceName = VoiceData[voiceKey].GetProperty("name").GetString();
                        if (languageName == SelectedLanguage && !Voices.Contains(voiceName))
                        {
                            Voices.Add(voiceName); // Add voice key to the list if it matches the selected language
                        }
                    }
                    SelectedVoice = Voices.FirstOrDefault(""); // Set the first voice as selected by default
                    await LoadQualitiesAsync(); // Load qualities after loading voices
                }
                else
                {
                    StatusText = "No voice data available to load voices."; // Update status text
                    Trace.WriteLine("No voice data available to load voices.");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading voices: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task LoadQualitiesAsync()
        {
            Qualities.Clear(); // Clear existing qualities

            foreach (var voiceKey in VoiceData.Keys)
            {
                var voiceName = VoiceData[voiceKey].GetProperty("name").GetString();
                var quality = VoiceData[voiceKey].GetProperty("quality").GetString();

                if (string.IsNullOrWhiteSpace(quality))
                {
                    continue; // Skip if quality is null or empty
                }

                if (voiceName == SelectedVoice && !Qualities.Contains(quality))
                {
                    Qualities.Add(quality);
                }
            }

            SelectedQuality = Qualities.FirstOrDefault(""); // Set the first quality as selected by default
            await Task.Run(()=> Trace.WriteLine($"Loaded {Qualities.Count} qualities for the selected voice: {SelectedVoice}"));
        }

        [RelayCommand]
        public async Task SetPlaybackSpeeDescriptiondAsync()
        {

            double speed = Math.Round(PlaybackSpeed, 1);
            string speedDescription = speed == 1 ? "Normal" : speed < 1 ? "Fast" : "Slow";
            PlaybackSpeedDescription = $"{speedDescription} ({speed:F1}x)";

            await Task.Run(()=> Trace.WriteLine($"Playback speed set to {speed:F1} - {PlaybackSpeedDescription}."));
        }

        private bool CanGenerate()
        {
            
            //return true; // Enable the Generate command for now, you can add conditions later
            if (!IsLoadingVoiceData || 
                !string.IsNullOrWhiteSpace(SelectedLanguage) || 
                !string.IsNullOrWhiteSpace(SelectedVoice) || 
                !string.IsNullOrWhiteSpace(PromtText))
            {
               return true; // Enable if not loading voice data and a language is selected
            }

            return false; // Disable otherwise
        }

        [RelayCommand(CanExecute = nameof(CanGenerate))]
        public async Task GenerateAsync()
        {
            string promt = await Task.Run(() => HelperMethods.CleanString(PromtText)); // Clean the prompt text using the helper method
            await Task.Run(()=>Trace.WriteLine("Generate command executed."));
        }

        private bool CanClearPromptText()
        {
            return !string.IsNullOrWhiteSpace(PromtText); // Enable if prompt text is not empty
        }

        [RelayCommand(CanExecute = nameof(CanClearPromptText))]
        public async Task ClearPromptTextAsync()
        {
            PromtText = string.Empty; // Clear the prompt text
            await Task.Run(()=> Trace.WriteLine("Prompt text cleared."));
        }
    }
}
