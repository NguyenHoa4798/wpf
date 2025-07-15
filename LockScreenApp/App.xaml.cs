using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using LockScreenApp.Services;
using LockScreenApp.Utilities;
using LockScreenApp.ViewModels;
using LockScreenApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;

namespace LockScreenApp
{
    public partial class App : System.Windows.Application
    {
        private readonly IServiceProvider _serviceProvider;
        private LoginWindow _loginWindow;
        private LogoutButtonWindow _logoutButton;
        private NotifyIcon _notifyIcon;
        private LoginViewModel _loginViewModel;

        public static event Action OnLogout = delegate { };

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            TaskManagerBlocker.DisableTaskManager();
        }

        public static void PerformLogout()
        {
            OnLogout?.Invoke();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            var configuration = Utilities.ConfigurationManager.BuildConfiguration();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddSingleton<HookService>();
            services.AddSingleton<ShutdownService>();
            services.AddSingleton<IdleTimeoutService>();

            var endpoint = configuration["ApiSettings:BaseUrl"];
            services.AddSingleton<GraphQLHttpClient>(sp =>
            {
                var options = new GraphQLHttpClientOptions { EndPoint = new Uri(endpoint) };
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                return new GraphQLHttpClient(options, new SystemTextJsonSerializer(), httpClient);
            });

            services.AddHttpClient<AuthenticationService>(client =>
            {
                client.BaseAddress = new Uri(endpoint);
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("ApiSettings:TimeoutSeconds"));
            });

            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<LogoutViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            OnLogout += OnLogoutHandler;
            ShowLoginWindow();
        }

        private void OnLoginSuccess()
        {
            _loginWindow?.Close();
            _loginWindow = null;

            var logoutVM = _serviceProvider.GetService<LogoutViewModel>();

            _logoutButton = new LogoutButtonWindow
            {
                DataContext = logoutVM
            };

            _logoutButton.Left = SystemParameters.WorkArea.Right - _logoutButton.Width - 20;
            _logoutButton.Top = 20;
            _logoutButton.Closing += LogoutWindow_Closing;
            _logoutButton.Show();

            ShowTrayIcon();
        }

        private void OnLogoutHandler()
        {
            if (_logoutButton != null)
            {
                _logoutButton.Closing -= LogoutWindow_Closing;
                _logoutButton.Close();
                _logoutButton = null;
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            if (_loginWindow != null)
            {
                _loginWindow.Close();
                _loginWindow = null;
            }

            _loginViewModel?.ResetState();

            var shutdownService = _serviceProvider.GetService<ShutdownService>();
            shutdownService.Stop();
            shutdownService.ResetTimer();

            ShowLoginWindow();
        }


        private void ShowLoginWindow()
        {
            if (_loginWindow != null)
            {
                _loginWindow.Close();
                _loginWindow = null;
            }

            if (_loginViewModel == null)
            {
                _loginViewModel = _serviceProvider.GetService<LoginViewModel>();
                _loginViewModel.OnLoginSuccess += OnLoginSuccess;
            }

            _loginWindow = new LoginWindow(_loginViewModel);
            _loginWindow.DataContext = _loginViewModel;

            _loginViewModel.StartServices();
            var shutdownService = _serviceProvider.GetService<ShutdownService>();
            shutdownService.Start();

            _loginWindow.Show();
        }

        private void LogoutWindow_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            _logoutButton?.Hide(); 
        }

        private void ShowTrayIcon()
        {
            if (_notifyIcon != null)
                return;

            var iconStream = GetResourceStream(new Uri("pack://application:,,,/Resources/appIcon.ico"))?.Stream;
            if (iconStream == null) return;

            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(iconStream),
                Visible = true,
                Text = "LockScreenApp - Click để mở"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Hiện ứng dụng", null, (s, e) => ShowLogoutButton());
            contextMenu.Items.Add("Đăng xuất", null, (s, e) => PerformLogout());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowLogoutButton();
        }

        private void ShowLogoutButton()
        {
            if (_logoutButton != null)
            {
                _logoutButton.Show();
                _logoutButton.Activate();
            }
        }

        public static T GetService<T>() where T : class
        {
            return ((App)Current)._serviceProvider.GetService<T>();
        }
    }
}
