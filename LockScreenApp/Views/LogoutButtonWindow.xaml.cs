using System.Windows;

namespace LockScreenApp.Views
{
    public partial class LogoutButtonWindow : Window
    {
        public LogoutButtonWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Left = SystemParameters.WorkArea.Right - Width - 10;
                Top = SystemParameters.WorkArea.Bottom - Height - 10;
            };
        }
    }
}
