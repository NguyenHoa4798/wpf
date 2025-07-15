using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using LockScreenApp.LoginModels;

namespace LockScreenApp.ViewModels
{
    public class LogoutViewModel : INotifyPropertyChanged
    {
        public ICommand LogoutCommand { get; }
        private Account _currentAccount;

        public Account CurrentAccount
        {
            get => _currentAccount;
            private set
            {
                _currentAccount = value;
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(RoleName));
            }
        }

        public string FullName => CurrentAccount?.FullName ?? string.Empty;
        public string RoleName => CurrentAccount?.RoleName ?? string.Empty;

        public LogoutViewModel()
        {
            LogoutCommand = new RelayCommand(async () => await LogoutAsync());
        }

        public void SetAccount(Account account)
        {
            CurrentAccount = account;
        }

        private Task LogoutAsync()
        {
            App.PerformLogout();
            return Task.CompletedTask;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
