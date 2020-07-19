using AeroPlayer.Services.YoutubeParser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
namespace AeroPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Notifier notifier { get; private set; }


        public App()
        {


        }
        public static Notifier MakeNotifier()
        {

            var notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Current.Dispatcher;
            });

            return notifier;
        }
        public static void ShowNotification(string title, string msg)
        {
            notifier.ShowInformation(msg);
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Console.WriteLine("Closing..");


        }
    }
}
