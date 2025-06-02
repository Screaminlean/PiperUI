using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace PiperUI.Interfaces
{
    public interface IConfigurationService
    {
        public string AppDataPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); } }
        public string AppName { get { return AppDomain.CurrentDomain.FriendlyName; } }

        public JsonNode ApplicationConfiguration { get; set; }

        public JsonNode UserConfiguration { get; set; }

        /// <summary>
        /// Loads the application appconfig.json in the executable directory.
        /// </summary>
        void LoadApplicationConfiguration();
        /// <summary>
        /// Reloads the application appconfig.json in the executable directory.
        /// </summary>
        void ReloadApplicationConfiguration();
        /// <summary>
        /// Saves the application appconfig.json in the executable directory.
        /// </summary>
        void SaveApplicationConfiguration();
        /// <summary>
        /// Loads the user userconfig.json in the user\%AppData% directory.
        /// </summary>
        void LoadUserConfiguration();
        /// <summary>
        /// Reloads the user userconfig.json user\%AppData% directory.
        /// </summary>
        void ReloadUserConfiguration();
        /// <summary>
        /// Saves the user userconfig.json in the user\%AppData% directory.
        /// </summary>
        void SaveUserConfiguration();

    }
}
