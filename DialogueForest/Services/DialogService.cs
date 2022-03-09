using System;
using System.Threading.Tasks;

using DialogueForest.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Strings = DialogueForest.Localization.Strings.Resources;
using Windows.Services.Store;
using Windows.UI.Xaml.Media;
using System.Linq;

namespace DialogueForest.Services
{
    public class DialogService: IDialogService
    {
        private IDispatcherService _dispatcherService;
        private IApplicationStorageService _storageService;
        private INavigationService _navigationService;
        private INotificationService _notificationService;

        public DialogService(IDispatcherService dispatcherService, INavigationService navigationService, IApplicationStorageService storageService, INotificationService notificationService)
        {
            _dispatcherService = dispatcherService;
            _navigationService = navigationService;
            _storageService = storageService;
            _notificationService = notificationService;
        }

        private bool shownFirstRun = false;
        private bool shownWhatsNew = false;
        public async Task ShowFirstRunDialogIfAppropriateAsync()
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(async () =>
                {
                    if (SystemInformation.Instance.IsFirstRun && !shownFirstRun)
                    {
                        shownFirstRun = true;
                        var dialog = new FirstRunDialog();
                        await dialog.ShowAsync();
                        _navigationService.Navigate<SettingsViewModel>();
                    }
                });
        }

        public async Task ShowWhatsNewDialogIfAppropriateAsync(bool forceShow)
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(async () =>
                {
                    if (forceShow || (SystemInformation.Instance.IsAppUpdated && !shownWhatsNew))
                    {
                        shownWhatsNew = true;
                        var dialog = new WhatsNewDialog();
                        await dialog.ShowAsync();
                    }
                });
        }

        public async Task ShowRateAppDialogIfAppropriateAsync()
        {
            var storeContext = StoreContext.GetDefault();
            await _dispatcherService.ExecuteOnUIThreadAsync(async () =>
            {
                if (SystemInformation.Instance.LaunchCount >=4 && !_storageService.GetValue<bool>("HasSeenRateAppPrompt"))
                {
                    if (await ShowConfirmDialogAsync(Strings.RateAppPromptTitle, Strings.RateAppPromptText, Strings.ButtonYesText, Strings.ButtonNoText))
                    {
                        var rateResult = await PromptUserToRateAppAsync(storeContext);

                        if (rateResult.HasValue)
                            _storageService.SetValue("HasSeenRateAppPrompt", true);
                    }
                    else
                    {
                        _storageService.SetValue("HasSeenRateAppPrompt", true);
                    }   
                }
            });
        }
        private async Task<bool?> PromptUserToRateAppAsync(StoreContext storeContext)
        {
            StoreRateAndReviewResult result = await
                storeContext.RequestRateAndReviewAppAsync();

            // Check status
            switch (result.Status)
            {
                case StoreRateAndReviewStatus.Succeeded:
                    return true;

                case StoreRateAndReviewStatus.CanceledByUser:
                    // Keep track that we prompted user and don’t prompt again for a while
                    return false;

                case StoreRateAndReviewStatus.NetworkError:
                    // User is probably not connected, so we’ll try again, but keep track so we don’t try too often
                    return null;

                // Something else went wrong
                case StoreRateAndReviewStatus.Error:
                default:
                    // Log error
                    _notificationService.ShowErrorNotification(result.ExtendedError);
                    return null;
            }
        }

        public async Task<string> ShowTreeNameDialogAsync()
        {
            var dialog = new TreeNameDialog();
            var result = await _dispatcherService.EnqueueAsync(async () => await dialog.ShowAsync());

            // Return new playlist name if checked, selected playlist otherwise
            return result == ContentDialogResult.Primary ? dialog.TreeName : null;
        }

        public async Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText = null, string cancelButtonText = null)
        {
            // If a ContentDialog is already open, stop here and return false
            if (VisualTreeHelper.GetOpenPopups(Window.Current).Where(p => p.Child is ContentDialog).Any())
                return false;

            ContentDialog confirmDialog = new ContentDialog
            {
                Title = title,
                Content = text,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = cancelButtonText
            };

            var theme = _storageService.GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out ElementTheme elementTheme); // We can parse the enum directly to ElementTheme since the names are the same!

            confirmDialog.RequestedTheme = elementTheme;

            ContentDialogResult result = await confirmDialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

    }
}
