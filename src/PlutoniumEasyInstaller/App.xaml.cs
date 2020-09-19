using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool AutoDetectBO2Installation { get; private set; } = true;
        public static string GivenBO2Directory { get; private set; } = string.Empty;
        public static bool AutoSteamInstall { get; private set; } = false;
        public static bool AutoNonSteamInstall { get; private set; } = false;
        public static bool NoStartShortcut { get; private set; } = false;
        public static bool NoDesktopShortcut { get; private set; } = false;
        public static bool NoReShade { get; private set; } = false;
        public static bool SingleInstance { get; private set; } = true;
        public static bool AutoConfig { get; private set; } = false;
        public static bool IgnoreSystemChecks { get; private set; } = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            for (var i = 0; i < e.Args.Length; i++)
            {
                string arg = e.Args[i];

                switch (arg)
                {
                    case "/DisableAutoDetect":
                        AutoDetectBO2Installation = false;
                        break;
                    case "/Directory":
                        try
                        {
                            string path = e.Args[i + 1];
                            GivenBO2Directory = path;
                        }
                        catch { }
                        break;
                    // First-come, first-served.
                    case "/Steam":
                        if (!AutoNonSteamInstall)
                            AutoSteamInstall = true;
                        break;
                    case "/NoSteam":
                        if (!AutoSteamInstall)
                            AutoNonSteamInstall = true;
                        break;
                    case "/NoStartShortcut":
                        NoStartShortcut = true;
                        break;
                    case "/NoDesktopShortcut":
                        NoDesktopShortcut = true;
                        break;
                    case "/NoReShade":
                        NoReShade = true;
                        break;
                    case "/DisableSingleInstance":
                        SingleInstance = false;
                        break;
                    case "/AutoConfig":
                        AutoConfig = true;
                        break;
                    case "/Language":
                        try
                        {
                            string language = e.Args[i + 1];
                            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
                        }
                        catch { }
                        break;
                    case "/IgnoreSystemChecks":
                        IgnoreSystemChecks = true;
                        break;
                }
            }

            base.OnStartup(e);
        }
    }
}
