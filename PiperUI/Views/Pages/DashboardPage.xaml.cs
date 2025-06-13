using PiperUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace PiperUI.Views.Pages
{
    public partial class DashboardPage : INavigableView<InfoViewModel>
    {
        public InfoViewModel ViewModel { get; }

        public DashboardPage(InfoViewModel viewModel)
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
