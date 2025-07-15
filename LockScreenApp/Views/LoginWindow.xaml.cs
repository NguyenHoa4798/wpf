using LockScreenApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace LockScreenApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginViewModel ViewModel => DataContext as LoginViewModel;
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/appIcon.ico"));
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel), "ViewModel cannot be null.");
            }
            DataContext = viewModel;

            PasswordBox.PasswordChanged += (s, e) => viewModel.Password = ((PasswordBox)s).Password;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is LoginViewModel vm && vm.LoginCommand?.CanExecute(null) == true)
                {
                    vm.LoginCommand.Execute(null);
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!e.Cancel && Keyboard.IsKeyDown(Key.F4) && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }
    }
}