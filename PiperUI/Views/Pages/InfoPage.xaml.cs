using PiperUI.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace PiperUI.Views.Pages
{
    public partial class InfoPage : INavigableView<InfoViewModel>
    {
        public InfoViewModel ViewModel { get; }

        public InfoPage(InfoViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
