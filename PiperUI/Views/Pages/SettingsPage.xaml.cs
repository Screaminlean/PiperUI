using PiperUI.ViewModels.Pages;
// Removed INavigableView usage for compatibility

namespace PiperUI.Views.Pages
{
    public partial class SettingsPage
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            // Snackbar logic removed, now handled globally in MainWindow
        }
    }
}
