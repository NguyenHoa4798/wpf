using System.Windows.Input;

namespace LockScreenApp.ViewModels;

public class MainViewModel
{
    public ICommand LogoutCommand { get; }

    public MainViewModel()
    {
        LogoutCommand = new RelayCommand(async () => await LogoutAsync());
    }

    private Task LogoutAsync()
    {
        App.PerformLogout();
        return Task.CompletedTask;
    }
}
