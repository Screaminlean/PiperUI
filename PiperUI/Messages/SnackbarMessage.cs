using CommunityToolkit.Mvvm.Messaging.Messages;
using Wpf.Ui.Controls;

namespace PiperUI.Messages
{
    public class SnackbarMessage : ValueChangedMessage<(string Message, ControlAppearance Appearance)>
    {
        public SnackbarMessage(string message, ControlAppearance appearance)
            : base((message, appearance))
        {
        }
    }
}
