using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Wpf.Ui.Controls;

namespace PiperUI.Messages
{
    public class SnackbarMessage : ValueChangedMessage<(string Message, ControlAppearance Appearance, TimeSpan Timeout)>
    {
        public SnackbarMessage(string message, ControlAppearance appearance, TimeSpan? timeout = null)
            : base((message, appearance, timeout ?? TimeSpan.FromSeconds(2)))
        {
        }

        public string Message => Value.Message;
        public ControlAppearance Appearance => Value.Appearance;
        public TimeSpan Timeout => Value.Timeout;
    }
}
