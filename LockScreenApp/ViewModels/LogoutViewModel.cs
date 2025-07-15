using System.Windows.Input;

namespace LockScreenApp.ViewModels;

public class LogoutViewModel
{
    public ICommand LogoutCommand { get; }

    public LogoutViewModel()
    {
        LogoutCommand = new RelayCommand(async () => await LogoutAsync());
    }

    private Task LogoutAsync()
    {
        App.PerformLogout();
        return Task.CompletedTask;
    }
}
