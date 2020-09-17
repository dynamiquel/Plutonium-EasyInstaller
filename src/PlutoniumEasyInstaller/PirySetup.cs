using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;

namespace PlutoniumEasyInstaller
{
    public static class PirySetup
    {
        public static DownloadProgressChangedEventHandler DownloadProgressChangedEvent;
        public static AsyncCompletedEventHandler DownloadCompleteEvent;

        public static event Action InstallComplete;
        public static event Action<string> OnPiryOutput;

        public static readonly string PiryUrl = "https://piry.plutonium.pw/piry.exe";
        public static readonly string PiryExecutableName = "piry.exe";

        private static Process currentProcess;

        static PirySetup()
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
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += DownloadProgressChangedEvent;
                    wc.DownloadFileCompleted += DownloadCompleteEvent;
                    wc.DownloadFileAsync(new System.Uri(PiryUrl), System.IO.Path.Combine(directory, PiryExecutableName));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Install(string directory)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.Combine(directory, PiryExecutableName),
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = directory
                    },
                };

                StartPiry(process);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void StartPiry(Process process)
        {
            currentProcess = process;

            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string line = e.Data;
            OnPiryOutput?.Invoke(line);

            if (!string.IsNullOrEmpty(line) && line.Contains("100 %"))
            {
                var process = (Process)sender;
                process.CancelOutputRead();
                process.Kill();
                currentProcess = null;
                InstallComplete?.Invoke();
            }
        }
    }
}
