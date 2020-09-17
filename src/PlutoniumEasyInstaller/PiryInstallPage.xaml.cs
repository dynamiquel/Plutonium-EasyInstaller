using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for PlutoniumInstallPage.xaml
    /// This page is pretty messy, huh.
    /// </summary>
    public partial class PiryInstallPage : NavigationPage
    {
        public string BO2Directory;

        private bool fileSizeAnnounced = false;
        private HashSet<int> recordedPercentages;

        public PiryInstallPage()
        {
            InitializeComponent();
        }

        protected override void OnWindowSet()
        {

        }

        protected override void OnNextButtonPressed()
        {
            Window.EnableBackButton = false;
        }

        public override void OnNavigated()
        {
            Window.EnableBackButton = false;
            Window.EnableNextButton = false;
            Window.EnableNavigationBar = true;
        }

        private void Write(string content)
        {
            System.Diagnostics.Debug.Write(content);
            DownloadStatusText.AppendText(content);
            DownloadStatusText.ScrollToEnd();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            DownloadPiry();
        }

        private void Error()
        {
            MessageBox.Show("There was an error during installation. Please open this app and try again.",
                "Failed to install",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Application.Current?.Shutdown();
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!fileSizeAnnounced)
            {
                fileSizeAnnounced = true;
                Write($"\rFile is {Math.Round(e.TotalBytesToReceive / 1000000f, 1)} MB.");
            }

            if (e.ProgressPercentage % 10 == 0)
            {
                if (recordedPercentages.Contains(e.ProgressPercentage))
                    return;

                recordedPercentages.Add(e.ProgressPercentage);
                Write($"\r{e.ProgressPercentage}% complete.");
            }
        }

        private void DownloadPiry()
        {
            recordedPercentages = new HashSet<int>();

            Write($"Downloading file from '{PirySetup.PiryUrl}' to '{BO2Directory}'...");
            PirySetup.DownloadProgressChangedEvent += OnDownloadProgressChanged;
            PirySetup.DownloadCompleteEvent += OnPiryDownloadComplete;

            bool downloadSuceeded = PirySetup.Download(BO2Directory);

            if (!downloadSuceeded)
            {
                PirySetup.DownloadCompleteEvent -= OnPiryDownloadComplete;
                PirySetup.DownloadProgressChangedEvent -= OnDownloadProgressChanged;
                Error();
            }
        }

        private void OnPiryDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            PirySetup.DownloadCompleteEvent -= OnPiryDownloadComplete;
            PirySetup.DownloadProgressChangedEvent -= OnDownloadProgressChanged;

            if (e.Cancelled)
            {
                Write("\rDownload was cancelled!");
                Error();
            }
            else if (e.Error != null)
            {
                Write($"\rDownload failed! Reason: {e.Error}.");
                Error();
            }
            else
            {
                Write("\rDownload complete!");

                InstallPiry();
            }
        }  

        private void InstallPiry()
        {
            PirySetup.InstallComplete += OnPiryInstallComplete;
            PirySetup.OnPiryOutput += HandlePiryOutput;

            Write("\rDownloading Call of Duty: Black Ops 2...");
            var successStart = PirySetup.Install(BO2Directory);

            if (!successStart)
            {
                PirySetup.InstallComplete -= OnPiryInstallComplete;
                Error();
            }
        }

        private async void HandlePiryOutput(string line)
        {
            await Dispatcher.SwitchToUi();

            Write($"\r{line}");
        }

        private async void OnPiryInstallComplete()
        {
            await Dispatcher.SwitchToUi();

            PirySetup.InstallComplete -= OnPiryInstallComplete;
            PirySetup.OnPiryOutput -= HandlePiryOutput;

            Write("\rCall of Duty: Black Ops 2 installed.");

            Window.EnableBackButton = false;

            var plutoniumConfigPage = new PlutoniumConfigPage
            {
                BO2Directory = BO2Directory
            };

            Window.Navigate(plutoniumConfigPage);
        }
    }
}
