using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace XamlRichMenuMaker
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static AppSettings Settings { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            Settings = configuration.GetSection("AppSettings").Get<AppSettings>();
            
            base.OnStartup(e);
        }
        public class AppSettings
        {
            public string ChannelAccessToken { get; set; }
            public string DebugUserId { get; set; }
        }
    }
    
}
