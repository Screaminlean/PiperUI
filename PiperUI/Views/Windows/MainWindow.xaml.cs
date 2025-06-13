using PiperUI.ViewModels.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using CommunityToolkit.Mvvm.Messaging;
using PiperUI.Messages;

namespace PiperUI.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }
        private SnackbarPresenter? _snackbarPresenter;
        private Snackbar? _snackbar;

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(RootNavigation);

            // Get SnackbarPresenter from XAML
            _snackbarPresenter = SnackbarPresenter;

            // Register for SnackbarMessage globally
            WeakReferenceMessenger.Default.Register<SnackbarMessage>(this, (r, m) =>
            {
                ShowSnackbar(m.Value.Message, m.Value.Appearance, m.Value.Timeout);
            });
        }

        private void ShowSnackbar(string message, ControlAppearance appearance = ControlAppearance.Primary, System.TimeSpan? timeout = null)
        {
            if (_snackbarPresenter == null)
                return;
            _snackbar = new Snackbar(_snackbarPresenter)
            {
                Content = message,
                Appearance = appearance
            };
            if (timeout.HasValue)
            {
                _snackbar.Timeout = timeout.Value;
            }
            _snackbar.Show();
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
