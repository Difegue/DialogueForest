using System;

using DialogueForest.Services;

using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using Microsoft.UI.Xaml;
using DialogueForest.ViewModels;
using DialogueForest.Core.Services;
using WinUIEx;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.UI.Windowing;
using CommunityToolkit.WinUI.Helpers;
using Sentry;
using System.Reflection;

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

            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                SentrySdk.Init(o =>
                {
                    // Tells which project in Sentry to send events to:
                    o.Dsn = "https://b00ea101ce53a91f7df754a02fe56328@o4508492455149568.ingest.de.sentry.io/4508492464783440";
                });
            }

            _window = new WindowEx()
            {
                MinHeight = 500,
                MinWidth = 500,
                Title = "DialogueForest",
                PersistenceId = "MainWindow",
                ExtendsContentIntoTitleBar = !AppWindowTitleBar.IsCustomizationSupported(),
                SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop(),
            };

            _window.AppWindow.SetIcon("Assets\\icon.ico");

            var theme = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<string>(nameof(SettingsViewModel.ElementTheme));
            Enum.TryParse(theme, out Theme elementTheme);

            _ = Task.Run(async () =>
            {
                Thread.Sleep(60000);
                await Ioc.Default.GetRequiredService<IDialogService>().ShowRateAppDialogIfAppropriateAsync();
            });

            var shell = new Views.ShellPage();

            shell.Loaded += async (s, e) =>
            {
                XamlRoot = shell.XamlRoot;

                // Analytics
                if (enableAnalytics)
                {
                    var assembly = Assembly.GetExecutingAssembly().GetName();
                    var isUnpackaged = false;

                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.Release = assembly.Version.ToString();

                        try
                        {
                            scope.Release = SystemInformation.Instance.ApplicationVersion.ToFormattedString();
                            scope.Contexts.Device.Brand = SystemInformation.Instance.DeviceManufacturer;
                            scope.Contexts.Device.Name = SystemInformation.Instance.DeviceModel;
                            scope.Contexts.Device.Family = SystemInformation.Instance.DeviceFamily;
                            scope.Contexts.Device.Architecture = SystemInformation.Instance.OperatingSystemArchitecture.ToString();

                            scope.SetTag("total_launch_count", SystemInformation.Instance.TotalLaunchCount.ToString());
                            scope.SetTag("unpackaged", "false");
                        }
                        catch
                        {
                            // SystemInformation will fail in unpackaged scenarios
                            scope.SetTag("unpackaged", "true");
                            isUnpackaged = true;
                        }
                    });

                    try
                    {
                        SystemInformation.Instance.TrackAppUse(args.UWPLaunchActivatedEventArgs, XamlRoot);
                    }
                    catch 
                    {
                        // SystemInformation will fail in unpackaged scenarios, disregard the exception
                        isUnpackaged = true;
                    }
                    
                    SentrySdk.CaptureMessage($"{assembly.Name} - {assembly.Version} " + (isUnpackaged ? "(Unpackaged)" : ""));
                }

                await Ioc.Default.GetRequiredService<IDialogService>().ShowFirstRunDialogIfAppropriateAsync();
            };

            _window.WindowContent = shell;
            _window.Activate();

            // We can only set the theme after the window has activated
            await Ioc.Default.GetRequiredService<IInteropService>().SetThemeAsync(elementTheme);
        }

        private void OnAppUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
#if DEBUG
#else
            var enableAnalytics = Ioc.Default.GetRequiredService<IApplicationStorageService>().GetValue<bool>(nameof(SettingsViewModel.EnableAnalytics), true);
            if (enableAnalytics)
            {
                SentrySdk.CaptureException(e.Exception);
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
            services.AddSingleton<WordCountingService>();

            // Viewmodel Factories
            //services.AddSingleton<AlbumViewModelFactory>();
            //services.AddSingleton<TrackViewModelFactory>();
            //services.AddSingleton<FilePathViewModelFactory>();

            // Viewmodels
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<OpenedNodesViewModel>();
            services.AddSingleton<PinnedNodesViewModel>();
            services.AddSingleton<WelcomeViewModel>();
            services.AddSingleton<ExportViewModel>();

            services.AddTransient<DialogueTreeViewModel>();
            services.AddTransient<DialogueNodeViewModel>();
            services.AddTransient<DialoguePartViewModel>();
            services.AddTransient<ReplyPromptViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
