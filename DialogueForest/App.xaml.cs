using System;

using DialogueForest.Services;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using Microsoft.UI.Xaml;
using CommunityToolkit.WinUI.Helpers;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.UI;
using DialogueForest.ViewModels;
using DialogueForest.Core.Services;
using Microsoft.UI;
using WinUIEx;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.UI.Windowing;

namespace DialogueForest
{
    public sealed partial class App : Application
    {
        private WindowEx _window;
        public WindowEx Window => _window;
        public XamlRoot XamlRoot { get; private set; }

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
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Ioc.Default.GetRequiredService<IDispatcherService>().Initialize();

            // Compact sizing
            var isCompactEnabled = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.IsCompactSizing));
            if (isCompactEnabled)
            {
                Resources.MergedDictionaries.Add(
                   new ResourceDictionary { Source = new Uri(@"ms-appx:///Microsoft.UI.Xaml/DensityStyles/Compact.xaml", UriKind.Absolute) });
            }

            // Analytics
            //SystemInformation.Instance.TrackAppUse(args);
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

            _window = new WindowEx()
            {
                MinHeight = 500,
                MinWidth = 500,
                Title = "DialogueForest",
                PersistenceId = "MainWindow",
                ExtendsContentIntoTitleBar = !AppWindowTitleBar.IsCustomizationSupported(), 
                Backdrop = new MicaSystemBackdrop(),
            };
            

            var theme = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out Theme elementTheme);
            await Ioc.Default.GetRequiredService<IInteropService>().SetThemeAsync(elementTheme);

            _ = Task.Run(async () =>
            {
                Thread.Sleep(60000);
                await Ioc.Default.GetRequiredService<IDialogService>().ShowRateAppDialogIfAppropriateAsync();
            });

            var shell = new Views.ShellPage();

            shell.Loaded += async (s, e) =>
            {
                XamlRoot = shell.XamlRoot;
                await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();
            };

            _window.WindowContent = shell;
            _window.Activate();
        }

        private void OnAppUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
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
            services.AddSingleton<OpenedNodesViewModel>();
            services.AddSingleton<WelcomeViewModel>();
            services.AddSingleton<ExportViewModel>();
            //services.AddSingleton<QueueViewModel>();
            //services.AddSingleton<SearchResultsViewModel>();
            //services.AddSingleton<LocalPlaybackViewModel>();

            services.AddTransient<DialogueTreeViewModel>();
            services.AddTransient<DialogueNodeViewModel>();
            services.AddTransient<DialoguePartViewModel>();
            services.AddTransient<ReplyPromptViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
