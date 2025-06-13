using PiperUI.Interfaces;
using PiperUI.Helpers;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PiperUI.Services
{
    public partial class ConfigurationService : ObservableObject, IConfigurationService
    {
        // Config File Paths
        private readonly string appConfigPath;
        private readonly string userConfigPath;

        #region Fields

        [ObservableProperty]
        private JsonNode? _applicationConfiguration;
        [ObservableProperty]
        private JsonNode? _userConfiguration;

        #endregion

        #region Methods

        public void LoadApplicationConfiguration()
        {
            string jsonString = File.ReadAllText(appConfigPath);
            ApplicationConfiguration = JsonNode.Parse(jsonString)!;
        }

        public void LoadUserConfiguration()
        {
            string jsonString = File.ReadAllText(userConfigPath);
            UserConfiguration = JsonNode.Parse(jsonString)!;
        }

        public void ReloadApplicationConfiguration()
        {
            throw new NotImplementedException();
        }

        public void ReloadUserConfiguration()
        {
            throw new NotImplementedException();
        }

        public void SaveApplicationConfiguration()
        {
            if (ApplicationConfiguration != null)
            {
                SaveConfiguration(ApplicationConfiguration, appConfigPath);
            }
        }

        public void SaveUserConfiguration()
        {
            if (UserConfiguration != null)
            {
                SaveConfiguration(UserConfiguration, userConfigPath);
            }
        }

        private void SaveConfiguration(object obj, string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(path, jsonString);
        }

        #endregion

        public ConfigurationService()
        {
            userConfigPath = $"{HelperMethods.userDataDir}\\userconfig.json";
            appConfigPath = $"{HelperMethods.appDataDir}\\appconfig.json";

            if (!Directory.Exists(HelperMethods.userDataDir))
            {
                Directory.CreateDirectory(HelperMethods.userDataDir);
            }

            if (!Directory.Exists(HelperMethods.userFilesDir))
            {
                Directory.CreateDirectory(HelperMethods.userFilesDir);
            }

            if (!Directory.Exists(HelperMethods.CustomVoicesDirectory))
            {
                Directory.CreateDirectory(HelperMethods.CustomVoicesDirectory);
            }

            if (!Directory.Exists(HelperMethods.OutputDirectory))
            {
                Directory.CreateDirectory(HelperMethods.OutputDirectory);
            }

            if (!File.Exists(userConfigPath))
            {
                // Initialize with sensible defaults if not already set
                if (UserConfiguration == null)
                {
                    UserConfiguration = new JsonObject
                    {
                        ["Theme"] = "Dark",
                        ["CustomVoicesDirectory"] = HelperMethods.CustomVoicesDirectory,
                        ["OutputDirectory"] = Path.Combine(HelperMethods.userFilesDir, HelperMethods.outputDir)
                    };
                }
                SaveUserConfiguration();
            }

            if (File.Exists(userConfigPath))
            {
                LoadUserConfiguration();
            }

            if (!Directory.Exists(HelperMethods.appDataDir))
            {
                Directory.CreateDirectory(HelperMethods.appDataDir);
            }

            if (!File.Exists(appConfigPath))
            {
                // Initialize with sensible defaults if not already set
                if (ApplicationConfiguration == null)
                {
                    ApplicationConfiguration = new JsonObject
                    {
                        ["VoicesDownloadUrl"] = HelperMethods.voicesDownloadUrl,
                        ["PiperDownloadUrl"] = HelperMethods.piperDownloadUrl,
                        ["ModelsDirectory"] = Path.Combine(HelperMethods.appDataDir, "models"),
                        ["PiperDirectory"] = HelperMethods.appDataDir
                    };
                }
                SaveApplicationConfiguration();
            }

            if (File.Exists(appConfigPath))
            {
                LoadApplicationConfiguration();
            }
        }
    }
}
