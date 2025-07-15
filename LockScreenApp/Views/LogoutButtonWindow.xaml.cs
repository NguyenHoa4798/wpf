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
                // Hiển thị ở góc trên bên phải
                Left = SystemParameters.WorkArea.Right - Width - 10;
                Top = SystemParameters.WorkArea.Top + 10;
            };
        }
    }
}
