using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace PlutoniumEasyInstaller
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : NavigationPage
    {
        private static readonly List<string> possibleSteamLocations = new List<string>()
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\Call of Duty Black Ops II",
            @"C:\Games\Call of Duty Black Ops II",
            @"D:\Program Files (x86)\SteamLibrary\steamapps\common\Call of Duty Black Ops II",
            @"D:\Games\Call of Duty Black Ops II",
            @"S:\Program Files (x86)\SteamLibrary\steamapps\common\Call of Duty Black Ops II",
            @"S:\Games\Call of Duty Black Ops II",
        };

        private static readonly string locateBO2DirectoryMessage = "The folder selected does not contain Call of Duty: Black Ops 2.\nTry again?";
        private static readonly string locateBO2DirectoryMessageFirstTime = "Call of Duty: Black Ops 2 could not be found on your computer.\nCould you show me where it is?";

        private bool loaded = false;

        public MainPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (loaded)
                return;

            if (App.AutoSteamInstall)
                GoToPlutoniumSetup();
            else if (App.AutoNonSteamInstall)
                GoToPirySetup();

            loaded = true;
        }

        protected override void OnNextButtonPressed()
        {

        }

        protected override void OnWindowSet()
        {

        }

        public override void OnNavigated()
        {
            Window.EnableNavigationBar = false;
        }

        private static bool IsValidBO2Directory(string path) =>
            System.IO.File.Exists(System.IO.Path.Combine(path, "t6rzm.exe"));

        private static string ShowBO2DirectoryDialogue()
        {
            var folderDialogue = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                Description = "Select the COD: Black Ops 2 folder.",
                RootFolder = Environment.SpecialFolder.MyComputer
            };

            folderDialogue.ShowDialog();

            return folderDialogue.SelectedPath;
        }

        private static string GetBO2InstalledDirectory()
        {
            // Would be string.Empty, unless a command argument was given.
            string bo2Directory = App.GivenBO2Directory;

            // Proceed with the given command argument.
            if (IsValidBO2Directory(bo2Directory))
                return bo2Directory;

            if (App.AutoDetectBO2Installation)
            {
                for (var i = 0; i < possibleSteamLocations.Count; i++)
                {
                    if (IsValidBO2Directory(possibleSteamLocations[i]))
                    {
                        bo2Directory = possibleSteamLocations[i];
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(bo2Directory))
            {
                string locateMessage = locateBO2DirectoryMessageFirstTime;

                while (string.IsNullOrWhiteSpace(bo2Directory))
                {
                    var messageBoxResult = System.Windows.MessageBox.Show(
                        locateMessage,
                        "Game not found!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);

                    if (messageBoxResult != MessageBoxResult.Yes)
                        return string.Empty;

                    string selectedPath = ShowBO2DirectoryDialogue();

                    if (IsValidBO2Directory(selectedPath))
                        bo2Directory = selectedPath;
                    else
                        locateMessage = locateBO2DirectoryMessage;
                }

            }

            return bo2Directory;

            // Check possible locations
            // If none found, display message box saying choose black ops folder.
            // Open folder picker
            // Check if it is blak ops 2 folder
            // Remember path
        }

        private void GoToPlutoniumSetup()
        {
            var bo2Directory = GetBO2InstalledDirectory();

            if (string.IsNullOrWhiteSpace(bo2Directory))
                return;

            var plutoniumConfigPage = new PlutoniumConfigPage
            {
                BO2Directory = bo2Directory
            };

            Window.EnableBackButton = true;
            Window.Navigate(plutoniumConfigPage);
        }

        private void GoToPirySetup()
        {
            var piryConfigPage = new PiryConfigPage();
            Window.EnableBackButton = true;
            Window.Navigate(piryConfigPage);
        }

        private void SteamButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPlutoniumSetup();
        }

        private void NonSteamButton_Click(object sender, RoutedEventArgs e)
        {
            GoToPirySetup();
        }
    }
}
