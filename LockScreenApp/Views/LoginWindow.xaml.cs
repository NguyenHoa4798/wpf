using LockScreenApp.Services;
using LockScreenApp.ViewModels;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LockScreenApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel), "ViewModel cannot be null.");
            }
            DataContext = viewModel;

            PasswordBox.PasswordChanged += (s, e) => viewModel.Password = ((PasswordBox)s).Password;
        }
        public LoginWindow() : this(new LoginViewModel(
                    new AuthenticationService(
                        new HttpClient(),
                        Utilities.ConfigurationManager.BuildConfiguration()
                    ),
                    new HookService(),
                    new ShutdownService(Utilities.ConfigurationManager.BuildConfiguration()),
                    new IdleTimeoutService(Utilities.ConfigurationManager.BuildConfiguration())
                ))
        {
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4 && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Handled = true; // Block Alt+F4
            }
            else if (e.Key == Key.Enter)
            {
                // Trigger LoginCommand on Enter key
                if (DataContext is LoginViewModel vm && vm.LoginCommand?.CanExecute(null) == true)
                {
                    vm.LoginCommand.Execute(null);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Prevent closing via Alt+F4 or other means if desired
            if (!e.Cancel && Keyboard.IsKeyDown(Key.F4) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
    }
}