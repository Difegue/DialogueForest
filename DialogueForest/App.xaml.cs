using System;

using DialogueForest.Services;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.UI;
using DialogueForest.ViewModels;
using DialogueForest.Services;
using DialogueForest.Core.Services;

namespace DialogueForest
{
    public sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        public App()
        {
            // Initialize IoC
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);

            InitializeComponent();
            UnhandledException += OnAppUnhandledException;

            // Deferred execution until used. Check https://docs.microsoft.com/dotnet/api/system.lazy-1 for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));

            // Compact sizing
            var isCompactEnabled = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsCompactSizing));
            if (isCompactEnabled)
            {
                Resources.MergedDictionaries.Add(
                   new ResourceDictionary { Source = new Uri(@"ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml", UriKind.Absolute) });
            }

            // Analytics
            SystemInformation.Instance.TrackAppUse(args);
#if DEBUG
#else
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                // Initialize AppCenter
                AppCenter.Start("a8f16b52-8b3b-4159-8b54-96d412867493",
                    typeof(Analytics), typeof(Crashes));
            }
#endif

            var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
            viewTitleBar.ButtonInactiveForegroundColor = (Color)Resources["SystemBaseHighColor"];

            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        private void OnAppUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
#if DEBUG
#else
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("exception", e.Exception.ToString());
                Analytics.TrackEvent("UnhandledCrash", dict);
            }
#endif
            var notificationService = Ioc.Default.GetRequiredService<INotificationService>();
            notificationService.ShowErrorNotification(e.Exception);

            // Try to handle the exception in case it's not catastrophic
            e.Handled = true;
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(DialogueNodeViewModel), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<IDispatcherService, DispatcherService>();
            services.AddSingleton<IApplicationStorageService, ApplicationStorageService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IInteropService, InteropService>();
            services.AddSingleton<ForestDataService>();

            // Viewmodel Factories
            //services.AddSingleton<AlbumViewModelFactory>();
            //services.AddSingleton<TrackViewModelFactory>();
            //services.AddSingleton<FilePathViewModelFactory>();

            // Viewmodels
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<SettingsViewModel>();
            
            //services.AddSingleton<AlbumDetailViewModel>();
            //services.AddSingleton<FoldersViewModel>();
            //services.AddSingleton<PlaylistViewModel>();
            //services.AddSingleton<QueueViewModel>();
            //services.AddSingleton<SearchResultsViewModel>();
            //services.AddSingleton<LocalPlaybackViewModel>();

            services.AddTransient<DialogueNodeViewModel>();
            services.AddTransient<DialoguePartViewModel>();
            services.AddTransient<ReplyPromptViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
