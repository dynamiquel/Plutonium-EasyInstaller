using System;
using System.IO;
using System.Windows.Forms;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for PlutoniumConfigPage.xaml
    /// </summary>
    public partial class PiryConfigPage : NavigationPage
    {
        private bool loaded;
        private readonly string directoryName = "Call of Duty Black Ops II";

        public PiryConfigPage()
        {
            InitializeComponent();

            DirectoryTextBox.Text = !string.IsNullOrWhiteSpace(App.GivenBO2Directory) 
                ? App.GivenBO2Directory
                : Path.Combine(@"C:\Games", directoryName);
        }

        private void PageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loaded)
                return;

            if (App.AutoConfig)
                OnNextButtonPressed();

            loaded = true;
        }

        protected override void OnWindowSet()
        {
            
        }

        protected override void OnNextButtonPressed()
        {
            var piryInstallPage = new PiryInstallPage
            {
                BO2Directory = DirectoryTextBox.Text
            };

            Window.Navigate(piryInstallPage);
        }

        public override void OnNavigated()
        {
            Window.EnableBackButton = true;
            Window.EnableNextButton = true;
            Window.EnableNavigationBar = true;
        }

        private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var folderDialogue = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                Description = Properties.Resources.PirySetup_SelectNewBO2Folder,
                RootFolder = Environment.SpecialFolder.MyComputer
            };

            folderDialogue.ShowDialog();

            if (string.IsNullOrWhiteSpace(folderDialogue.SelectedPath))
                return;

            if (GetTotalFreeSpace(folderDialogue.SelectedPath.Substring(0, 3)) < 2.2000E+10)
            {
                MessageBox.Show(Properties.Resources.PirySetup_BO2NotEnoughSpace,
                    Properties.Resources.NotEnoughSpaceHeader, 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DirectoryTextBox.Text = Path.Combine(folderDialogue.SelectedPath, directoryName);
        }

        private long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.TotalFreeSpace;
                }
            }
            return -1;
        }
    }
}
