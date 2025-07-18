﻿using Microsoft.Extensions.Configuration;
using System;
using System.Windows.Threading;

namespace LockScreenApp.Services;

public class IdleTimeoutService
{
    private readonly DispatcherTimer _timer;
    private readonly int _timeoutSeconds;
    private DateTime _lastActivity;
    public event Action OnIdleTimeout;

    public IdleTimeoutService(IConfiguration configuration)
    {
        _timeoutSeconds = configuration.GetValue<int>("AppSettings:IdleTimeoutSeconds");
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
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if ((DateTime.Now - _lastActivity).TotalSeconds >= _timeoutSeconds)
        {
            OnIdleTimeout?.Invoke();
        }
    }

    public void Stop()
    {
        _timer.Stop();
    }
}
