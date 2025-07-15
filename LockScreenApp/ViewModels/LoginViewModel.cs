// LoginViewModel.cs
using LockScreenApp.LoginModels;
using LockScreenApp.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LockScreenApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthenticationService _authService;
        private readonly HookService _hookService;
        private readonly ShutdownService _shutdownService;
        private readonly IdleTimeoutService _idleService;
        private string _username;
        private string _password;
        private string _errorMessage;
        private string _countdownMessage;
        private DispatcherTimer _countdownTimer;

        public event Action OnLoginSuccess; // New event for successful login

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public string CountdownMessage
        {
            get => _countdownMessage;
            set { _countdownMessage = value; OnPropertyChanged(nameof(CountdownMessage)); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(AuthenticationService authService, HookService hookService,
            ShutdownService shutdownService, IdleTimeoutService idleService)
        {
            _authService = authService;
            _hookService = hookService;
            _shutdownService = shutdownService;
            _idleService = idleService;
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            _idleService.OnIdleTimeout += OnIdleTimeout;
            StartServices();
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter username and password.";
                return;
            }

            var request = new LoginRequest { Username = Username, Password = Password };
            var response = await _authService.LoginAsync(request);
            if (response.Success)
            {
                //_hookService.Dispose();
                _shutdownService.Stop();
                _idleService.Stop();
                OnLoginSuccess?.Invoke(); // Trigger success event
            }
            else
            {
                ErrorMessage = response.Message;
            }
        }

        public void StartServices()
        {
            _hookService.StartHooks();
            _shutdownService.Start();
            _idleService.Start();

            // Khởi tạo lại countdown timer
            _countdownTimer?.Stop();
            _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            var startTime = DateTime.Now;
            _countdownTimer.Tick += (s, e) =>
            {
                var remaining = 600 - (DateTime.Now - startTime).TotalSeconds;
                if (remaining <= 0)
                {
                    CountdownMessage = "System is shutting down...";
                    _countdownTimer.Stop();
                }
                else
                {
                    CountdownMessage = $"System will shutdown in {(int)remaining} seconds.";
                }
            };
            _countdownTimer.Start();
        }

        private void OnIdleTimeout()
        {
            ErrorMessage = "Session timed out. Please log in again.";
            Username = string.Empty;
            Password = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        public RelayCommand(Func<Task> execute) => _execute = execute;
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public async void Execute(object parameter) => await _execute();
    }
}