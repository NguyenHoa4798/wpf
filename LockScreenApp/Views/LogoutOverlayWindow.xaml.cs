using System.Windows;

namespace LockScreenApp.Views
{
    public partial class LogoutOverlayWindow : Window
    {
        public LogoutOverlayWindow()
        {
            InitializeComponent();
            Left = SystemParameters.WorkArea.Right - Width - 20;
            Top = SystemParameters.WorkArea.Bottom - Height - 20;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.Topmost = true;
        }
    }
}
