namespace PiperUI.Interfaces
{
    public interface IDownloaderService
    {
        Task<bool> DownloadFileAsync(string url, string destinationPath, string fileName);
        bool DownloadFile(string url, string destinationPath, string fileName);
        bool IsInProgress { get; }
        bool IsCompleted { get; }
        int DownloadProgress { get; }
    }
}
