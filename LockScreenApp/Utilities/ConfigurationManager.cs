using Microsoft.Extensions.Configuration;
using System;

namespace LockScreenApp.Utilities
{
    public static class ConfigurationManager
    {
        public static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}