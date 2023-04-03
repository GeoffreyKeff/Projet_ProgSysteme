using GuiApp.Model;
using GuiApp.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GuiApp
{
    public partial class SaveView : Page
    {
        // get the instance of main
        private readonly MainViewModel main = MainViewModel.GetInstance();
        // the only instance of this class
        private static SaveView instance;

        // constructor of this class
        private SaveView()
        {
            InitializeComponent();
            // print all saves that exist
            PrintAllSaves();
            // initialized labels and buttons of the interface
            Initialized();
        }

        public static SaveView GetInstance()
        {
            if (instance == null)
            {
                instance = new SaveView();
            }
            return instance;
        }

        // initialized labels and buttons of the interface
        public new void Initialized()
        {
            // label nameOFSave
            GVName.Header = GetText("nameOfSave");
            GVSource.Header = GetText("sourceOfSave");
            GVDest.Header = GetText("destOfSave");
            GVSize.Header = GetText("sizeOfSave");
            PlaySave.Content = GetText("start");
            PauseSave.Content = GetText("pause");
            DeleteSave.Content = GetText("delete");
            GVFileLeft.Header = GetText("gvFileLeft");
            GVSizeLeft.Header = GetText("gvSizeLeft");
            StopSave.Content = GetText("stop");
            GVProgress.Header = GetText("progress");
            GVType.Header = GetText("type");
            // button for launch all saves
            LaunchAllSaves.Content = GetText("launchAllSaveButton");
        }

        // print in interface all the save that exist
        public void PrintAllSaves()
        {
            saveList.ItemsSource = new ObservableCollection<Save>(SaveViewModel.GetInstance().L_saves);
        }

        private void PlaySave_Click(object sender, RoutedEventArgs e)
        {
            
            Process[] proc = Process.GetProcessesByName("notepad");
            if (proc.Length == 0)
            {


                if (saveList.SelectedItems.Count > 0)
                {
                    foreach (Save s in saveList.SelectedItems)
                    {
                        //check is the save is already launch or not
                        if (s.B_AlreadyLaunch)
                        {
                            //If it has already been started, just resume the backup
                            Save.dRes(s.T, s.ThreadLock);
                        }
                        else
                        {
                            //If it has not yet been launched, then we launch the save
                            s.LaunchSave();
                        }

                    }
                }
            } else
            {
                MessageBox.Show(GetText("processRun"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PauseSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveList.SelectedItems.Count > 0)
            {
                foreach (Save s in saveList.SelectedItems)
                {
                    Save.dPause(s.T, s.ThreadLock);
                }
            }
        }

        private void StopSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveList.SelectedItems.Count > 0)
            {
                foreach (Save s in saveList.SelectedItems)
                {
                    Save.dStop(s.T, s.Stopper);
                }
            }
        }

        private void DeleteSave_Click(object sender, RoutedEventArgs e)
        {
            if (saveList.SelectedItems.Count > 0)
            {
                foreach (Save s in saveList.SelectedItems)
                {
                    SaveViewModel.GetInstance().DeleteSave(s);
                }
                PrintAllSaves();
            }
        }

        // action for the launchallsaves button
        private void LaunchAllSaves_Click(object sender, RoutedEventArgs e)
        {
            saveList.IsEnabled = false;
            LaunchAllSaves.IsEnabled = false;
            // launch all save
            if (!SaveViewModel.GetInstance().LaunchAllSaves())
            {
                MessageBox.Show(GetText("processRun"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            saveList.IsEnabled = true;
            LaunchAllSaves.IsEnabled = true;
        }

        // get the different text in language json
        private string GetText(string str_i)
        {
            return main.GetValueInJson(str_i);
        }

        public void DispPause()
        {
            System.Windows.MessageBox.Show(GetText("processRun2"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void CopyError()
        {
            System.Windows.MessageBox.Show(GetText("SourceError"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
