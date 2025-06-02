using PiperUI.Interfaces;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PiperUI.Services
{
    public partial class ConfigurationService : ObservableObject, IConfigurationService
    {
        private readonly string appName = AppDomain.CurrentDomain.FriendlyName;
        // Application AppData Directory
        private readonly string appDataDir;
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
            SaveConfiguration(ApplicationConfiguration, appConfigPath);
        }

        public void SaveUserConfiguration()
        {
            SaveConfiguration(UserConfiguration, userConfigPath);
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
            appDataDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{appName}";
            userConfigPath = $"{appDataDir}\\userconfig.json";
            appConfigPath = $"{appDataDir}\\appconfig.json";

            if (!Directory.Exists(appDataDir))
            {
                Directory.CreateDirectory(appDataDir);
            }

            if (!File.Exists(userConfigPath))
            {
                SaveUserConfiguration();
            }

            if (File.Exists(userConfigPath))
            {
                LoadUserConfiguration();
            }

            if (!File.Exists(appConfigPath))
            {
                SaveApplicationConfiguration();
            }

            if (File.Exists(appConfigPath))
            {
                LoadApplicationConfiguration();
            }
        }
    }
}
