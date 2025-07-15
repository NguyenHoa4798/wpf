using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace LockScreenApp.Services
{
    public class ShutdownService
    {
        private readonly DispatcherTimer _timer;
        private readonly int _timeoutSeconds;
        private DateTime _lastActivity;

        public ShutdownService(IConfiguration configuration)
        {
            _timeoutSeconds = configuration.GetValue<int>("AppSettings:ShutdownTimeoutSeconds");
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            _lastActivity = DateTime.Now;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void ResetTimer()
        {
            _lastActivity = DateTime.Now;
            if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastActivity).TotalSeconds >= _timeoutSeconds)
            {
                try
                {
                    Process.Start("shutdown", "/s /t 0");
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
