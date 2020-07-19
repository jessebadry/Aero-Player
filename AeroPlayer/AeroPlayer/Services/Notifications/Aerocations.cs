using System;
using System.Collections.Generic;
using System.Text;
using ToastNotifications;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace AeroPlayer.Services.Notifications
{
    static class Aerocations
    {
        public static Notifier notifier { get; private set; }

        static Aerocations()
        {
            notifier = App.MakeNotifier();

        }
        public static void ShowInfoNotification(string msg)
        {
            notifier.ShowInformation(msg);
        }
        public static void ShowWarningNotification(string msg)
        {
            notifier.ShowWarning(msg);
        }
        public static void ShowErrorNotification(string msg)
        {
            notifier.ShowError(msg);
        }
        public static void ShowSuccessNotification(string msg)
        {
            notifier.ShowSuccess(msg);
        }


    }
}
