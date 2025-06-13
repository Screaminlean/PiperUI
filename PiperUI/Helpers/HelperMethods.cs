using System.IO;

namespace PiperUI.Helpers
{
    internal static class HelperMethods
    {
        internal static string CleanString(string input)
        {
            return input
                .Replace("&", "and")
                .Replace("|", "")
                .Replace("\"", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace("\r\n", " ")
                .Replace("\n", " ");
        }

        internal static string AppName => AppDomain.CurrentDomain.FriendlyName;

        internal static string customDir => "CustomVoices";

        internal static string outputDir => "GeneratedOutput";

        internal static string userDataDir => $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{AppName}";

        // This has onedrive issues C:\\Users\\{user}\\OneDrive\\Documents
        // internal static string userFilesDir => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{AppName}";

        internal static string userFilesDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal).Replace(@"OneDrive\", ""), AppName);
        internal static string appDataDir => $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\{AppName}";

        internal static string piperDownloadUrl => "https://github.com/rhasspy/piper/releases/download/2023.11.14-2/piper_windows_amd64.zip";

        internal static string voicesDownloadUrl => "https://huggingface.co/rhasspy/piper-voices/resolve/main/voices.json";

        internal static string CustomVoicesDirectory => Path.Combine(userFilesDir, customDir);

        internal static string OutputDirectory => Path.Combine(userFilesDir, customDir);

    }
}
