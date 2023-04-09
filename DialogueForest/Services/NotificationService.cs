using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommunityToolkit.WinUI.Notifications;
using DialogueForest.Core.Interfaces;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;

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
                .AddHeroImage(new Uri("ms-appx:///Assets/StoreLogo.png"))
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
            // Create the toast content
            var content = new ToastContent()
            {
                // More about the Launch property at https://docs.microsoft.com/dotnet/api/CommunityToolkit.WinUI.notifications.toastcontent
                Launch = "ToastContentActivationParams",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },

                            new AdaptiveText()
                            {
                                 Text = description
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        // More about Toast Buttons at https://docs.microsoft.com/dotnet/api/CommunityToolkit.WinUI.notifications.toastbutton
                        new ToastButton("OK", "ToastButtonActivationArguments")
                        {
                            ActivationType = ToastActivationType.Foreground
                        },

                        new ToastButtonDismiss("Cancel")
                    }
                }
            };

            // Add the content to the toast
            var toast = new ToastNotification(content.GetXml());

            // And show the toast
            ShowToastNotification(toast);
        }

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
            }
            catch (Exception e)
            {
                ShowInAppNotification("Error showing notification: " + e, false);
            }
        }

    }
}
