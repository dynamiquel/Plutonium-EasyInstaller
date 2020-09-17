using IWshRuntimeLibrary;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using static System.Environment;

namespace PlutoniumEasyInstaller
{
    public static class PlutoniumSetup
    {
        public static DownloadProgressChangedEventHandler DownloadProgressChangedEvent;
        public static AsyncCompletedEventHandler DownloadCompleteEvent;

        public static event Action InstallConfigComplete;
        public static event Action InstallComplete;

        public static readonly string PlutoniumUri = "http://cdn.plutonium.pw/updater/plutonium.exe";
        public static readonly string ReShadeUri = "https://github.com/dynamiquel/Plutonium-EasyInstaller/raw/master/dxgi.dll";

        private static readonly string PlutoniumAppData = System.IO.Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), "Plutonium");
        private static readonly string PlutoniumExecutableName = "plutonium.exe";

        private static Process currentProcess;

        static PlutoniumSetup()
        {
            App.Current.Exit += OnExit;
        }

        private static void OnExit(object sender, System.Windows.ExitEventArgs e)
        {
            if (currentProcess != null && !currentProcess.HasExited)
                currentProcess?.Kill();
        }

        public static bool Download(string directory)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += DownloadProgressChangedEvent;
                    wc.DownloadFileCompleted += DownloadCompleteEvent;
                    wc.DownloadFileAsync(new System.Uri(PlutoniumUri), System.IO.Path.Combine(directory, PlutoniumExecutableName));
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public static void CreateShortcuts(string directory, bool startMenu = true, bool desktop = true)
        {
            if (desktop)
                CreateShortcut(directory, GetFolderPath(SpecialFolder.Desktop));

            if (startMenu)
                CreateShortcut(directory, System.IO.Path.Combine(GetFolderPath(SpecialFolder.StartMenu), "Programs"));
        }

        private static void CreateShortcut(string targetDirectory, string shortcutDirectory)
        {
            var wsh = new WshShell();
            var shortcut = (IWshShortcut)wsh.CreateShortcut(System.IO.Path.Combine(shortcutDirectory, $"Plutonium.lnk"));
            shortcut.TargetPath = System.IO.Path.Combine(targetDirectory, PlutoniumExecutableName);
            shortcut.Description = "Plutonium";

            try
            {
                shortcut.Save();
            }
            catch { }
        }

        public static bool DownloadReShade()
        {
            try
            {
                string binDir = System.IO.Path.Combine(PlutoniumAppData, "bin");
                if (!System.IO.Directory.Exists(binDir))
                    System.IO.Directory.CreateDirectory(binDir);

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += DownloadProgressChangedEvent;
                    wc.DownloadFileCompleted += DownloadCompleteEvent;
                    wc.DownloadFileAsync(new System.Uri(ReShadeUri), System.IO.Path.Combine(binDir, "dxgi.dll"));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Install(string directory)
        {

            if (!System.IO.Directory.Exists(PlutoniumAppData))
                System.IO.Directory.CreateDirectory(PlutoniumAppData);

            var formattedDir = directory.Replace("\\", "\\\\");

            // Auto-sets the BO2 directory.
            System.IO.File.WriteAllText(System.IO.Path.Combine(PlutoniumAppData, "config.json"),
                $"{{\"iw5Path\":\"\",\"t6Path\":\"{formattedDir}\"}}");

            InstallConfigComplete?.Invoke();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = System.IO.Path.Combine(directory, PlutoniumExecutableName),
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                },
            };

            StartPlutoniumInstaller(process);
        }

        private static void StartPlutoniumInstaller(Process process)
        {
            currentProcess = process;

            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data;

            if (!string.IsNullOrEmpty(line) && line.Contains("100 %"))
            {
                var process = (Process)sender;
                process.CancelOutputRead();
                process.Kill();
                currentProcess = null;

                InstallComplete?.Invoke();
            }
        }

        public static void StartPlutoniumLauncher()
        {
            Process.Start(System.IO.Path.Combine(PlutoniumAppData, "bin", "plutonium-launcher-win32"));
        }
    }
}
