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
            _loginWindow?.Hide();

            var logoutVM = _serviceProvider.GetService<LogoutViewModel>();

            _logoutButton = new LogoutButtonWindow
            {
                DataContext = logoutVM
            };

            _logoutButton.Left = SystemParameters.WorkArea.Right - _logoutButton.Width - 20;
            _logoutButton.Top = 20;
            _logoutButton.Show();
        }

        private void OnLogoutHandler()
        {
            _logoutButton?.Close();
            _logoutButton = null;

            var shutdownService = _serviceProvider.GetService<ShutdownService>();
            shutdownService.Stop();
            shutdownService.ResetTimer();

            ShowLoginWindow();
        }


        private void ShowLoginWindow()
        {
            var loginVM = _serviceProvider.GetService<LoginViewModel>();
            _loginWindow = new LoginWindow(loginVM);
            _loginWindow.DataContext = loginVM;
            loginVM.OnLoginSuccess += OnLoginSuccess;

            loginVM.StartServices(); 
            var shutdownService = _serviceProvider.GetService<ShutdownService>();
            shutdownService.Start();

            _loginWindow.Show();
        }
        public static T GetService<T>() where T : class
        {
            return ((App)Current)._serviceProvider.GetService<T>();
        }
    }
}
