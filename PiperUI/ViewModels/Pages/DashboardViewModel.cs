using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using PiperUI.Helpers;
using PiperUI.Interfaces;

namespace PiperUI.ViewModels.Pages
{
    /// <summary>
    /// ViewModel for the Dashboard page, manages Piper and voice data, and user selections
    /// </summary>
    public partial class DashboardViewModel : ObservableObject
    {
        // Services for configuration and downloading
        private readonly IConfigurationService _configurationService;
        private readonly IDownloaderService _downloaderService;

        /// <summary>
        /// Constructor: injects configuration and downloader services
        /// </summary>
        /// <param name="configuration">Configuration service</param>
        /// <param name="downloader">Downloader service</param>
        public DashboardViewModel(IConfigurationService configuration, IDownloaderService downloader)
        {
            _configurationService = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _downloaderService = downloader ?? throw new ArgumentNullException(nameof(downloader));
        }

        // Observable properties for UI binding
        /// <summary>
        /// Indicates if voice data is currently being loaded
        /// </summary>
        [ObservableProperty]
        private bool _isLoadingVoiceData = false;

        /// <summary>
        /// Status text to display in the UI
        /// </summary>
        [ObservableProperty]
        private string _statusText = "Loading";

        /// <summary>
        /// Dictionary containing all loaded voice data
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, JsonElement> _voiceData = new Dictionary<string, JsonElement>();

        /// <summary>
        /// List of available languages
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _languages = new ObservableCollection<string>();

        /// <summary>
        /// Currently selected language
        /// </summary>
        [ObservableProperty]
        private string _selectedLanguage = string.Empty;

        /// <summary>
        /// List of available voices for the selected language
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _voices = new ObservableCollection<string>();

        /// <summary>
        /// Currently selected voice
        /// </summary>
        [ObservableProperty]
        private string _selectedVoice = string.Empty;

        /// <summary>
        /// List of available qualities for the selected voice
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _qualities = new ObservableCollection<string>();

        /// <summary>
        /// Currently selected quality
        /// </summary>
        [ObservableProperty]
        private string _selectedQuality = string.Empty;

        /// <summary>
        /// List of custom voices available
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _customVoices = new ObservableCollection<string>();

        /// <summary>
        /// Currently selected custom voice (default is "None")
        /// </summary>
        [ObservableProperty]
        private string _selectedCustomVoice = "None";

        /// <summary>
        /// Playback speed for generated audio
        /// </summary>
        [ObservableProperty]
        private double _playbackSpeed = 1.0;

        /// <summary>
        /// Updates playback speed description when changing speed
        /// </summary>
        /// <param name="value">New playback speed value</param>
        partial void OnPlaybackSpeedChanging(double value)
        {
            SetPlaybackSpeeDescriptiondAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Description of the current playback speed
        /// </summary>
        [ObservableProperty]
        private string _playbackSpeedDescription = "Normal (1.0x)";

        /// <summary>
        /// The text prompt to be synthesized
        /// </summary>
        [ObservableProperty]
        private string _promptText = string.Empty;

        /// <summary>
        /// Initializes the view model, downloads Piper if needed, and loads voice data
        /// </summary>
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

            await LoadCustomVoicesAsync(); // Load custom voices

            StatusText = "Loaded....:)";
        }

        /// <summary>
        /// Downloads and extracts Piper if not present
        /// </summary>
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

        /// <summary>
        /// Determines if voice data can be loaded (always true for now)
        /// </summary>
        private bool CanLoadVoiceData()
        {
            return true;
        }

        /// <summary>
        /// Loads voice data, downloads voices.json if missing
        /// </summary>
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

        /// <summary>
        /// Determines if custom voices can be loaded (always true)
        /// </summary>
        private bool CanLoadCustomVoices()
        {
            return true; // Always allow loading custom voices
        }

        /// <summary>
        /// Loads custom voices from the configured directory
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanLoadCustomVoices))]
        private async Task LoadCustomVoicesAsync()
        {
            CustomVoices.Clear();
            CustomVoices.Add("None"); // Add default option

            // Get the custom voices directory from user configuration or use the default
            string customVoicesDirectory = _configurationService.UserConfiguration?["CustomVoicesDirectory"]?.ToString() ?? HelperMethods.CustomVoicesDirectory;

            // Check if the folder exists
            if (Directory.Exists(customVoicesDirectory))
            {
                // Get all files with the ".onnx" extension in the folder
                string[] onnxFiles = await Task.Run(() => Directory.GetFiles(customVoicesDirectory, "*.onnx"));

                // Iterate through the files and add their names to the ComboBox
                foreach(string filePath in onnxFiles)
                {
                    // Get the file name without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                    // Add the file name to the ComboBox
                    CustomVoices.Add(fileNameWithoutExtension);
                }
            }
        }

        /// <summary>
        /// Determines if voices can be loaded (not loading and language selected)
        /// </summary>
        private bool CanLoadVoices()
        {
            if (!IsLoadingVoiceData || !string.IsNullOrWhiteSpace(SelectedLanguage))
            {                 
                return true;
            }

            return false;
        }

        /// <summary>
        /// Loads voices for the selected language
        /// </summary>
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

        /// <summary>
        /// Loads qualities for the selected voice
        /// </summary>
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

        /// <summary>
        /// Updates playback speed description based on the current speed
        /// </summary>
        [RelayCommand]
        public async Task SetPlaybackSpeeDescriptiondAsync()
        {
            double speed = Math.Round(PlaybackSpeed, 1);
            string speedDescription = speed == 1 ? "Normal" : speed < 1 ? "Fast" : "Slow";
            PlaybackSpeedDescription = $"{speedDescription} ({speed:F1}x)";

            await Task.Run(()=> Trace.WriteLine($"Playback speed set to {speed:F1} - {PlaybackSpeedDescription}."));
        }

        /// <summary>
        /// Determines if the Generate command can execute
        /// </summary>
        private bool CanGenerate()
        {
            // Enable if not loading voice data and all required selections are made
            if (!IsLoadingVoiceData || 
                !string.IsNullOrWhiteSpace(SelectedLanguage) || 
                !string.IsNullOrWhiteSpace(SelectedVoice) || 
                !string.IsNullOrWhiteSpace(PromptText))
            {
               return true;
            }

            return false;
        }

        /// <summary>
        /// Command: Generate output (downloads required model files for the selected voice)
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanGenerate))]
        public async Task GenerateAsync()
        {
            if (!string.IsNullOrWhiteSpace(PromptText) && SelectedCustomVoice != "None")
            {

               // await Task.Run(() =>
               // {
               //     CallPiper("custom", customVoice + ".onnx");
               // });
                await Task.Delay(100);
            }
            else if (!string.IsNullOrWhiteSpace(PromptText) && SelectedLanguage != null && SelectedVoice != null && SelectedQuality != null)
            {
                string countryCode = GetCountryCode(SelectedVoice, SelectedQuality);
                // string onnxFile = "";

                if (!string.IsNullOrWhiteSpace(countryCode) && VoiceData.ContainsKey(countryCode))
                {
                    // Get the files property from the voice data
                    var filesProperty = VoiceData[countryCode].GetProperty("files");
                    foreach (var fileProp in filesProperty.EnumerateObject())
                    {
                        string fileName = fileProp.Name;
                        if (fileName.EndsWith(".onnx") || fileName.EndsWith(".json"))
                        {
                            //TODO: move url to settings
                            // Download the file (async)
                            string url = $"https://huggingface.co/rhasspy/piper-voices/resolve/main/{fileName}";
                            // Determine destination directory for model files
                            string modelsDir = _configurationService.ApplicationConfiguration?["ModelsDirectory"]?.ToString();
                            string destDir = string.IsNullOrWhiteSpace(modelsDir)
                                ? Path.Combine(HelperMethods.appDataDir, "models")
                                : modelsDir;
                            // Ensure the directory exists
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }
                            string destFile = Path.GetFileName(fileName);
                            bool downloadSuccess = await _downloaderService.DownloadFileAsync(url, destDir, destFile);
                            if (!downloadSuccess)
                            {
                                StatusText = $"Failed to download {destFile}.";
                                MessageBox.Show($"Failed to download {destFile}.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                break;
                            }
                        }
                    }
                }

            }
            else
            {
                MessageBox.Show("Make valid selections and enter a prompt");
            }

                string promt = await Task.Run(() => HelperMethods.CleanString(PromptText));
            await Task.Run(()=>Trace.WriteLine("Generate command executed."));
        }

        /// <summary>
        /// Determines if the prompt text can be cleared
        /// </summary>
        private bool CanClearPromptText()
        {
            return !string.IsNullOrWhiteSpace(PromptText);
        }

        /// <summary>
        /// Clears the prompt text
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanClearPromptText))]
        public async Task ClearPromptTextAsync()
        {
            PromptText = string.Empty;
            await Task.Run(()=> Trace.WriteLine("Prompt text cleared."));
        }

        /// <summary>
        /// Gets the country code key for the selected voice and quality
        /// </summary>
        /// <param name="selectedVoiceName">Selected voice name</param>
        /// <param name="selectedQuality">Selected quality</param>
        /// <returns>Country code string or null if not found</returns>
        private string GetCountryCode(string selectedVoiceName, string selectedQuality)
        {
            foreach (var countryCode in VoiceData.Keys)
            {
                if (countryCode.Contains(selectedVoiceName + "-" + selectedQuality))
                    return countryCode;
            }

            return string.Empty; // Handle the case where the country code is not found
        }
    }
}
