using GuiApp.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace GuiApp.ViewModel
{
    class SaveViewModel
    {
        // List of observer
        private List<IObserver> l_observers = new List<IObserver>();
        // list of save
        private List<Save> l_saves = new List<Save>();
        // instance of this class
        private static SaveViewModel instance;
        //Files's extension for cryptage
        private List<string> l_cryptExtension = new List<string>();
        // list of priorityExtensions
        private List<string> l_priorityExtension = new List<string>();
        private int nb_CopyPause;

        private SaveViewModel()
        { }

        //DP Singleton
        public static SaveViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new SaveViewModel();
            }
            return instance;
        }

        // create a new save and check informations
        // return if the save's information are good or not
        public int CreateSave(string str_name, string str_sourcePath, string str_destPath, int nb_type, bool b_crypt)
        {
            //Check if the save's name does not contain any prohibited characters
            var regex = new Regex(@"[^a-zA-Z0-9]");
            if (regex.IsMatch(str_name))
            {
                return 4;
            }
            if (str_name == "")
            {
                return 1;
            }
            if (str_destPath == "")
            {
                return 3;
            }
            // check if save's name already exist
            foreach (Save s in L_saves)
            {
                if (s.Str_name == str_name)
                {
                    return 1;
                }
            }
            // check if the source file or directory exist and if the max is ok
            if (!(Directory.Exists(str_sourcePath) || File.Exists(str_sourcePath)))
            {
                return 2;
            }
            else
            {
                Save s;
                // if all good we can create our save
                if (nb_type == 0)
                {
                    //Create the complete save
                    s = new SaveProcess(str_sourcePath, str_destPath, str_name, b_crypt, true);
                }
                else
                {
                    //Create the differential save
                    s = new SaveProcess(str_sourcePath, str_destPath, str_name, b_crypt, false);
                }
                // create an active session for state
                L_saves.Add(s);
                if (IsFolder(s.Str_filesSource))
                {
                    // notify state observer  
                    s.L_fileLeft = s.Nb_arborescenceFiles;
                    s.B_state = true;
                    this.NotifyObservers(s, "state");
                }
                else
                {
                    // notify state observer
                    s.Nb_arborescenceFiles = 1;
                    s.L_fileLeft = 1;
                    s.Nb_arborescenceSize = new FileInfo(s.Str_filesSource).Length;
                    s.L_sizeLeft = new FileInfo(s.Str_filesSource).Length;
                    s.B_state = true;
                    this.NotifyObservers(s, "state");
                }
            }
            return 0;
        }

        //Launch all the save in the same time
        public bool LaunchAllSaves()
        {
            Process[] proc = Process.GetProcessesByName("notepad");
            if (proc.Length == 0)
            {
                foreach (Save s in l_saves)
                {
                   s.LaunchSave();
                }
                return true;
            }
            return false;
        }

        //Play save from remote console
        public void PlaySave(string savename)
        {
            foreach (Save s in L_saves)
            {
                //search the concerned backup
                if (s.Str_name == savename)
                {
                    //When the client click on the "Play" boutton check If the backup has already been started
                    if (s.B_AlreadyLaunch)
                    {
                        //If it has already been launched, resume it
                        Save.dRes(s.T, s.ThreadLock);
                    }
                    else
                    {
                        //Else, launch the save
                        s.LaunchSave();
                    }
                    
                }
            }
        }

        //Pause save from remote console
        public void PauseSave(string savename)
        {
            foreach (Save s in L_saves)
            {
                //Search the the concerned backup
                if (s.Str_name == savename)
                {
                    Save.dPause(s.T, s.ThreadLock);
                }
            }
        }

        //Stop save from remote console
        public void StopSave(string savename)
        {
            foreach (Save s in L_saves)
            {
                //Search the the concerned backup
                if (s.Str_name == savename)
                {
                    Save.dStop(s.T, s.Stopper );
                }
            }
        }

        //delete a save
        public void DeleteSave(Save save)
        {
            L_saves.Remove(save);
            NotifyObservers(save, "deleteSave");
        }

        // check if folder or file exist
        public bool IsFolder(string str_source)
        {
            // get attributes of the path destination
            FileAttributes attr = File.GetAttributes(str_source);
            // if attributes get directory
            if (attr.HasFlag(FileAttributes.Directory))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // notify all observer that an action maked
        public void NotifyObservers(Save save, string str_type)
        {
            // foreach observer
            foreach (IObserver observer in l_observers)
            {
                // check the type of the notification
                switch (str_type)
                {
                    // if it's a log notification
                    case "log":
                        if (observer.GetType() == typeof(LogViewModel))
                        {
                            observer.Notify(save, str_type);
                        }
                        break;
                    // it's the end of the log
                    case "finalLog":
                        if (observer.GetType() == typeof(LogViewModel))
                        {
                            observer.Notify(save, str_type);
                        }
                        break;
                    // if it's a state notification
                    case "state":
                        if (observer.GetType() == typeof(StateViewModel))
                        {
                            observer.Notify(save, str_type);
                        }
                        //Notify the SocketServer to send data to connected clients
                        if (observer.GetType() == typeof(SocketServer))
                        {
                            observer.Notify(save, str_type);
                        }
                        break;
                    case "deleteSave":
                        if (observer.GetType() == typeof(StateViewModel))
                        {
                            observer.Notify(save, str_type);
                        }
                        //Notify the SocketServer to send data to connected clients
                        if (observer.GetType() == typeof(SocketServer))
                        {
                            observer.Notify(save, str_type);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        // add an observer
        public void RegisterObserver(IObserver observer)
        {
            l_observers.Add(observer);
        }

        // remove an observer
        public void RemoveObserver(IObserver observer)
        {
            l_observers.Remove(observer);
        }

        //All getter/setter
        public List<Save> L_saves
        {
            get { return this.l_saves; }
            set { this.l_saves = value; }
        }

        public List<string> L_cryptExtension
        {
            get { return l_cryptExtension; }
            set { l_cryptExtension = value; }
        }

        public List<string> L_priorityExtension
        {
            get { return l_priorityExtension; }
            set { l_priorityExtension = value; }
        }

        public int Nb_CopyPause
        {
            get { return nb_CopyPause; }
            set { nb_CopyPause = value; }
        }
    }
}
