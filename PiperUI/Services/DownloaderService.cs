using PiperUI.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace PiperUI.Services
{
    public partial class DownloaderService : ObservableObject, IDownloaderService
    {
        [ObservableProperty]
        private bool isInProgress = false;

        [ObservableProperty]
        private bool isCompleted = false;

        [ObservableProperty]
        private int downloadProgress = 0;

        public async Task<bool> DownloadFileAsync(string url, string destinationPath, string fileName)
        {
            IsInProgress = true;
            IsCompleted = false;
            var httpClient = new HttpClient();
            string filePath = Path.Combine(destinationPath, fileName);

            // Check for internet connection by sending a HEAD request
            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                using var headResponse = await httpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);
                if (!headResponse.IsSuccessStatusCode)
                {
                    Trace.WriteLine($"Cannot reach the download URL: {url}");
                    await ResetAfterErrorAsync(); // Reset the state after an error
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"No internet connection or cannot reach the download URL: {ex.Message}");
                await ResetAfterErrorAsync(); // Reset the state after an error
                return false;
            }

            try
            {
                using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using var downloadStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                DownloadProgress = 0;
                while ((read = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;
                    if (canReportProgress)
                    {
                        DownloadProgress = (int)((totalRead * 100L) / totalBytes);
                    }
                }
                await fileStream.FlushAsync();
                fileStream.Close();
                DownloadProgress = 100;
                await ResetAfterSuccessAsync(); // Reset the state after a successful download
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error downloading file: {ex.Message}");
                await ResetAfterErrorAsync(); // Reset the state after an error
                return false;
            }
        }

        public bool DownloadFile(string url, string destinationPath, string fileName)
        {
            IsInProgress = true;
            IsCompleted = false;
            var httpClient = new HttpClient();
            string filePath = Path.Combine(destinationPath, fileName);

            // Check for internet connection by sending a HEAD request
            try
            {
                var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
                var headResponse = httpClient.Send(headRequest, HttpCompletionOption.ResponseHeadersRead);
                if (!headResponse.IsSuccessStatusCode)
                {
                    Trace.WriteLine($"Cannot reach the download URL: {url}");
                    ResetAfterError(); // Reset the state after an error
                    return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"No internet connection or cannot reach the download URL: {ex.Message}");
                ResetAfterError(); // Reset the state after an error
                return false;
            }

            try
            {
                var response = httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using var downloadStream = response.Content.ReadAsStream();
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                DownloadProgress = 0;
                while ((read = downloadStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, read);
                    totalRead += read;
                    if (canReportProgress)
                    {
                        DownloadProgress = (int)((totalRead * 100L) / totalBytes);
                    }
                }
                fileStream.Flush();
                fileStream.Close();
                DownloadProgress = 100;
                ResetAfterSuccess(); // Reset the state after a successful download
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error downloading file: {ex.Message}");
                ResetAfterError(); // Reset the state after an error
                return false;
            }
        }

        private async Task ResetAfterErrorAsync()
        {
            IsInProgress = false;
            IsCompleted = false;
            DownloadProgress = 0;
            await Task.Run(() => Trace.WriteLine("Download failed, state reset."));
        }

        private async Task ResetAfterSuccessAsync()
        {
            IsInProgress = false;
            IsCompleted = true;
            await Task.Run(() => Trace.WriteLine("Download completed successfully, state reset."));
        }

        private void ResetAfterError()
        {
            IsInProgress = false;
            IsCompleted = false;
            DownloadProgress = 0;
            Trace.WriteLine("Download failed, state reset.");
        }

        private void ResetAfterSuccess()
        {
            IsInProgress = false;
            IsCompleted = true;
            Trace.WriteLine("Download completed successfully, state reset.");
        }
    }
}
