using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using LockScreenApp.Services;
using LockScreenApp.Utilities;
using LockScreenApp.ViewModels;
using LockScreenApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Windows;

namespace LockScreenApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private LoginWindow _loginWindow;
        private LogoutOverlayWindow _logoutOverlay;
        private LogoutButtonWindow _logoutButton;

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
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("ApiSettings:TimeoutSeconds"));
            });

            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<MainViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            OnLogout += OnLogoutHandler;
            ShowLoginWindow();
        }

        private void OnLoginSuccess()
        {
            // 1. Ẩn LoginWindow
            _loginWindow?.Hide();

            // 2. Tắt overlay che toàn màn hình
            _logoutOverlay?.Close();
            _logoutOverlay = null;

            // 3. Hiện nút "Đăng xuất" nhỏ
            _logoutButton = new LogoutButtonWindow
            {
                DataContext = _serviceProvider.GetService<MainViewModel>()
            };
            _logoutButton.Show();
        }

        private void OnLogoutHandler()
        {
            // 1. Tắt nút đăng xuất nhỏ
            _logoutButton?.Close();
            _logoutButton = null;

            // 2. Show lại LoginWindow + overlay
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            var loginVM = _serviceProvider.GetService<LoginViewModel>();
            _loginWindow = new LoginWindow(loginVM);
            _loginWindow.DataContext = loginVM;
            loginVM.OnLoginSuccess += OnLoginSuccess;
            _loginWindow.Show();

            // Mở lớp che toàn màn hình khi chưa đăng nhập
            _logoutOverlay = new LogoutOverlayWindow
            {
                DataContext = _serviceProvider.GetService<MainViewModel>()
            };
            _logoutOverlay.Show();
        }
    }
}
