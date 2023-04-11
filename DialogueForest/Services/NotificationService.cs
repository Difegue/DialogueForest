using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommunityToolkit.WinUI.Notifications;
using DialogueForest.Core.Interfaces;
using RtfPipe.Tokens;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using static System.Net.Mime.MediaTypeNames;

namespace DialogueForest.Services
{
    public class NotificationService : NotificationServiceBase
    {

        public NotificationService()
        {
        }

        public override void ShowInAppNotification(string notification, bool autoHide)
        {
            //TODO: check for compact mode
            InvokeInAppNotificationRequested(new InAppNotificationRequestedEventArgs { NotificationText = notification, NotificationTime = autoHide ? 1500 : 0});
        }

        public override void ScheduleNotification(string title, string text, DateTime day, TimeSpan notificationTime)
        {
            var assetUri = AppDomain.CurrentDomain.BaseDirectory + "Assets";

            // Don't schedule if date is in the past
            if (day.Add(notificationTime) < DateTime.Now)
            {
                return;
            }

            new ToastContentBuilder()
                //.AddArgument("action", "viewItemsDueToday")
                .AddInlineImage(new Uri("file:///" + assetUri + "/Icon_Exclamation.png"))
                .AddText(title)
                .AddText(text)
                //.AddProgressBar()
                .Schedule(day.Add(notificationTime), toast =>
                {
                    toast.Id = day.ToString("yyyy-MM-dd");
                });
        }

        public override void RemoveScheduledNotifications(bool onlyRemoveToday = false)
        {
            // Create the toast notifier
            ToastNotifierCompat notifier = ToastNotificationManagerCompat.CreateToastNotifier();

            // Get the list of scheduled toasts that haven't appeared yet
            IReadOnlyList<ScheduledToastNotification> scheduledToasts = notifier.GetScheduledToastNotifications();

            foreach (var toRemove in scheduledToasts) {

                if (!onlyRemoveToday || toRemove.Id == DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    notifier.RemoveFromSchedule(toRemove);
                }
            }
        }

        public override void ShowBasicToastNotification(string title, string description)
        {
            var assetUri = AppDomain.CurrentDomain.BaseDirectory + "Assets";

            new ToastContentBuilder()
                .AddInlineImage(new Uri("file:///" + assetUri + "/Icon_Nice.png"))
                .AddText(title)
                .AddText(description)
                .Show();
        }

    }
}
