using Wpf.Ui.Abstractions.Controls;

namespace PiperUI.ViewModels.Pages
{
    public partial class InfoViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;
        [ObservableProperty]
        private string _appVersion = String.Empty;

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            AppVersion = $"Version: {GetAssemblyVersion()}";
            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }
    }
}
