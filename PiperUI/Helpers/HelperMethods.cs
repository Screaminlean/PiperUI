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

        internal static string AppName => "PiperUI";

        internal static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        
    }
}
