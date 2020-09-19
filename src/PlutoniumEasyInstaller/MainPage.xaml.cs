using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool loaded = false;

        public MainPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (loaded)
                return;

            if (!App.IgnoreSystemChecks)
            {
                DetectAntiMalware();
                DetectVCPP();
                DetectDirectX();
            }

            if (App.AutoSteamInstall)
                GoToPlutoniumSetup();
            else if (App.AutoNonSteamInstall)
                GoToPirySetup();

            loaded = true;
        }

        public override void OnNavigated()
        {
            Window.EnableNavigationBar = false;
        }

        private void DetectAntiMalware()
        {
            List<string> malwareProcessNames = new List<string>()
            {
                "Bitdefender Agent",
                "Bitdefender agent",
                "bdservicehost",
                "AVG Antivirus",
                "McAfee",
                "McAfee Host",
                "Norton Internet Security",
                "Norton Security",
                "Malwarebytes Service"
            };

            foreach (var malwareProcessName in malwareProcessNames)
            {
                // get the list of all processes by the "procName"       
                if (Process.GetProcessesByName(malwareProcessName).Length > 0)
                {
                    System.Windows.MessageBox.Show(Properties.Resources.AntiMalwareWarning, 
                        Properties.Resources.AntiMalwareWarningHeader, 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);

                    break;
                }
            }
        }

        private void DetectVCPP()
        {
            var installedVersionStr = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\10.0\VC\VCRedist\x86", "Version", "v0");
            installedVersionStr = installedVersionStr.Replace("v", "");

            var installedVersion = Version.Parse(installedVersionStr);
            var requiredVersion = Version.Parse("10.0.40219.325");
            

            if (installedVersion < requiredVersion)
            {
                var result = System.Windows.MessageBox.Show(Properties.Resources.Start_VCPPOutdated,
                    Properties.Resources.Start_OutdatedDependencies,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo("https://www.microsoft.com/en-gb/download/confirmation.aspx?id=5555"));
            }
        }

        private void DetectDirectX()
        {
            if (!System.IO.File.Exists(@"C:\Windows\System32\d3d11.dll"))
            {
                var result = System.Windows.MessageBox.Show(Properties.Resources.Start_DirectXOutdated,
                    Properties.Resources.Start_OutdatedDependencies,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo("https://www.microsoft.com/en-us/download/confirmation.aspx?id=35"));
            }
        }

        private static bool IsValidBO2Directory(string path) =>
            System.IO.File.Exists(System.IO.Path.Combine(path, "t6rzm.exe")) || 
            System.IO.File.Exists(System.IO.Path.Combine(path, "t6zm.exe"));

        private static bool IsMissingDLC(string path)
        {
            return !System.IO.File.Exists(System.IO.Path.Combine(path, "DLC1.md5")) ||
                !System.IO.File.Exists(System.IO.Path.Combine(path, "DLC2.md5")) ||
                !System.IO.File.Exists(System.IO.Path.Combine(path, "DLC3.md5")) ||
                !System.IO.File.Exists(System.IO.Path.Combine(path, "DLC4.md5")) ||
                !System.IO.File.Exists(System.IO.Path.Combine(path, "Nuketown.md5"));
        }

        private static string ShowBO2DirectoryDialogue()
        {
            var folderDialogue = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true,
                Description = Properties.Resources.Start_SelectBO2Folder,
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
                string locateMessage = Properties.Resources.Start_BO2NotFound;

                while (string.IsNullOrWhiteSpace(bo2Directory))
                {
                    var messageBoxResult = System.Windows.MessageBox.Show(
                        locateMessage,
                        Properties.Resources.GameNotFound,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation);

                    if (messageBoxResult != MessageBoxResult.Yes)
                        return string.Empty;

                    string selectedPath = ShowBO2DirectoryDialogue();

                    if (IsValidBO2Directory(selectedPath))
                        bo2Directory = selectedPath;
                    else
                        locateMessage = Properties.Resources.Start_InvalidBO2Folder;
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

            if (IsMissingDLC(bo2Directory))
            {
                var result = System.Windows.MessageBox.Show(Properties.Resources.Start_DLCMissing,
                    Properties.Resources.Start_DLCMissingHeader,
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var piryInstallPage = new PiryInstallPage
                    {
                        BO2Directory = bo2Directory
                    };

                    Window.Navigate(piryInstallPage);
                    return;
                }
                else if (result == MessageBoxResult.Cancel)
                    return;
            }

            var plutoConfigPage = new PlutoniumConfigPage
            {
                BO2Directory = bo2Directory
            };

            Window.EnableBackButton = true;
            Window.Navigate(plutoConfigPage);
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
