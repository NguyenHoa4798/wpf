using System;

namespace LockScreenApp.LoginModels
{
    public class Session
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsActive { get; set; }
    }
}