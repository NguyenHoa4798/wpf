using LockScreenApp.Services;
using LockScreenApp.Utilities;
using LockScreenApp.ViewModels;
using LockScreenApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace LockScreenApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Utilities.ConfigurationManager.BuildConfiguration());
            services.AddSingleton<HookService>();
            services.AddSingleton<ShutdownService>();
            services.AddSingleton<IdleTimeoutService>();
            services.AddHttpClient<AuthenticationService>(client =>
            {
                client.BaseAddress = new Uri(Utilities.ConfigurationManager.BuildConfiguration()["ApiSettings:BaseUrl"]);
                client.Timeout = TimeSpan.FromSeconds(Utilities.ConfigurationManager.BuildConfiguration().GetValue<int>("ApiSettings:TimeoutSeconds"));
            });

            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<LoginViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new LoginWindow(_serviceProvider.GetService<LoginViewModel>());
            loginWindow.Show();

            var taskbar = WinApiHelper.FindWindow("Shell_TrayWnd", "");
            WinApiHelper.ShowWindow(taskbar, WinApiHelper.SW_HIDE);
        }
    }
}
