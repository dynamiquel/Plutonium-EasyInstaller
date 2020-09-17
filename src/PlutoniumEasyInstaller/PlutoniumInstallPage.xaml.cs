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
    public partial class PlutoniumInstallPage : NavigationPage
    {
        public string BO2Directory;

        private bool fileSizeAnnounced = false;
        private HashSet<int> recordedPercentages;
        private bool enableDesktopShortcut;
        private bool enableStartShortcut;
        private bool enableReShade;
        private bool errorEncountered;

        public PlutoniumInstallPage()
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

        public void Start(bool desktopShortcut, bool startShortcut, bool reShade)
        {
            enableDesktopShortcut = desktopShortcut;
            enableStartShortcut = startShortcut;
            enableReShade = reShade;

            ProcessInstallStage(InstallStage.DownloadPlutonium);           
        }

        private void ProcessInstallStage(InstallStage stage)
        {
            // 1. Download Plutonium.
            // 2. Download ReShade.
            // 3. Install ReShade.
            // 4. Create Shortcuts.
            // 5. Install Plutonium.

            switch (stage)
            {
                case InstallStage.DownloadPlutonium:
                    DownloadPlutonium();
                    break;
                case InstallStage.CreateShortcuts:
                    if (enableDesktopShortcut || enableStartShortcut)
                        SetupShortcuts();
                    else
                        ProcessInstallStage(InstallStage.InstallPlutonium);
                    break;
                case InstallStage.DownloadReShade:
                    if (enableReShade)
                        SetupReShade();
                    else
                        ProcessInstallStage(InstallStage.CreateShortcuts);
                    break;
                case InstallStage.InstallPlutonium:
                    InstallPlutonium();
                    break;
            }
        }

        private void Write(string content)
        {
            System.Diagnostics.Debug.Write(content);
            DownloadStatusText.AppendText(content);
            DownloadStatusText.ScrollToEnd();
        }

        private void Error()
        {
            errorEncountered = true;

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

        private void DownloadPlutonium()
        {
            recordedPercentages = new HashSet<int>();

            Write($"Downloading file from '{PlutoniumSetup.PlutoniumUri}' to '{BO2Directory}'...");
            PlutoniumSetup.DownloadProgressChangedEvent += OnDownloadProgressChanged;
            PlutoniumSetup.DownloadCompleteEvent += OnPlutoniumDownloadComplete;

            bool downloadSuceeded = PlutoniumSetup.Download(BO2Directory);

            //if (!downloadSuceeded)
              //  Error();

            PlutoniumSetup.DownloadProgressChangedEvent -= OnDownloadProgressChanged;
        }

        private void OnPlutoniumDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            PlutoniumSetup.DownloadCompleteEvent -= OnPlutoniumDownloadComplete;

            if (errorEncountered)
                return;

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

                ProcessInstallStage(InstallStage.DownloadReShade);
            }
        }

        private void SetupShortcuts()
        {
            Write("\rCreating shortcuts...");
            PlutoniumSetup.CreateShortcuts(BO2Directory, enableStartShortcut, enableDesktopShortcut);
            Write("\rShortcuts created.");

            ProcessInstallStage(InstallStage.InstallPlutonium);
        }

        private void SetupReShade()
        {
            recordedPercentages = new HashSet<int>();

            PlutoniumSetup.DownloadProgressChangedEvent += OnDownloadProgressChanged;
            PlutoniumSetup.DownloadCompleteEvent += OnReShadeDownloadComplete;

            Write("\rDownloading ReShade...");

            bool downloadSuccess = PlutoniumSetup.DownloadReShade();
            if (!downloadSuccess)
                Write("\rReShade installation failed!");
        }

        private void OnReShadeDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                Write("\rReShade download was cancelled!");
            else if (e.Error != null)
                Write("\rReShade download failed!");
            else
            {
                Write("\rReShade download complete!");
            }

            ProcessInstallStage(InstallStage.CreateShortcuts);
        }

        private void InstallPlutonium()
        {
            PlutoniumSetup.InstallConfigComplete += OnPlutoniumInstallConfigComplete;
            PlutoniumSetup.InstallComplete += OnPlutoniumInstallComplete;

            Write("\rCreating config file...");
            PlutoniumSetup.Install(BO2Directory);
        }

        private async void OnPlutoniumInstallComplete()
        {
            await Dispatcher.SwitchToUi();

            PlutoniumSetup.InstallComplete -= OnPlutoniumInstallComplete;
            Write("\rPlutonium installed.");

            Window.EnableNavigationBar = false;
            Window.Navigate(new CompletePage());
        }

        private void OnPlutoniumInstallConfigComplete()
        {
            PlutoniumSetup.InstallConfigComplete -= OnPlutoniumInstallConfigComplete;
            Write("\rConfig file created.");
            Write("\rInstalling Plutonium...");
        }
    }

    enum InstallStage
    {
        DownloadPlutonium,
        DownloadReShade,
        CreateShortcuts,
        InstallPlutonium,
    }
}
