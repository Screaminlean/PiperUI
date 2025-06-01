using PiperUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace PiperUI.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        public virtual Task OnNavigatedToAsync()
        {
            return Task.CompletedTask; // Return a completed task if no navigation logic is needed
        }
    }
}
