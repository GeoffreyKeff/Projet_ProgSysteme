using GuiApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CryptoSoft;
using System.Security.Cryptography;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows;
using System.Security.Permissions;

namespace GuiApp.Model
{
    //delegate for pause/resume
    public delegate void Del(Thread t, ManualResetEvent threadLock);
    public delegate void DelS(Thread t, CancellationTokenSource stopper);
    public abstract class Save : INotifyPropertyChanged
    {
        //Crypt or not
        private bool b_crypt;
        //State of the save (active or end)
        private bool b_state;
        //Save state(not launch or lauch)
        private bool b_AlreadyLaunch = false;
        //Type of the Save (Complete or differencial)
        private bool b_type;

        //Total Crypt time
        private long l_totalCryptTime;
        //Crypt time of the curent file
        private long l_cryptTimeCurrentFile;
        // the total time transfer
        private long l_timeTransfer;
        //Transfer time of the curent file
        private long l_currentFileTimeTransfer;
        //Size of all the arborescence
        private long nb_arborescenceSize = 0;
        //Size left
        private long l_sizeLeft;
        //Number of files left
        private long l_fileLeft;

        //Save's progression
        private int nb_progression;
        //Number of total files
        private int nb_arborescenceFiles = 0;
        //Number of copied files
        private int nb_filecopy;

        //Save's name
        private string str_name;
        //Source path of the file(s)
        private string str_filesSource;
        //Destination path of the file(s)
        private string str_filesDest;
        //Curent copy file
        private string str_currentCopyFile;
        //Curent destination path
        private string str_currentCopyFileDestination;

        private static ManualResetEvent _threadsLocker = new ManualResetEvent(true);
        private static volatile int currentCopyValue = 0;

        //List with all the files path
        private List<string> list = new List<string>();

        //Constructor
        public Save(string fileSource, string fileDestination, string name, bool crypt, bool saveType)
        {
            //Type of save
            this.b_type = saveType;
            //File source
            this.str_filesSource = fileSource;
            //File Destination
            this.str_filesDest = fileDestination;
            //Save's name
            this.str_name = name;
            //Crypt or not
            this.b_crypt = crypt;
            //Save's progression
            this.Nb_progression = 0;

            //Get the file attributes
            FileAttributes attr = File.GetAttributes(str_filesSource);
            //If it's a directory
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //Check if the Source path contain "\" at the end
                if (!str_filesSource.EndsWith("\\")){
                    //Add "\" at the end
                    str_filesSource = str_filesSource + "\\";
                }
                //Check if the destination path contain "\" at the end
                if (!str_filesDest.EndsWith("\\"))
                {
                    //Add "\" at the end
                    str_filesDest = str_filesDest + "\\";
                }

                //Launch the methode to get all the arborescence
                long[] res = GetTotalSizeArborescence(str_filesSource, str_filesDest);
                nb_arborescenceSize = res[0];
                nb_arborescenceFiles = (int)res[1];
            }
            else
            //If there is only one file
            {
                //Check if the Source path contain "\" at the end
                if (!str_filesDest.EndsWith("\\"))
                {
                    //Add "\" at the end
                    str_filesDest = str_filesDest + "\\";
                }

                nb_arborescenceSize = new FileInfo(str_filesSource).Length;
                nb_arborescenceFiles = 1;
            }

            this.L_fileLeft = nb_arborescenceFiles;
            this.L_sizeLeft = nb_arborescenceSize;
        }

        // thread gestion
        private Thread t;
        // manual locker for the current thread
        private ManualResetEvent threadLock = new ManualResetEvent(true);
        // manual stopper for the current thread
        private CancellationTokenSource stopper = new CancellationTokenSource();
        private bool b_isRunning;

        // Launch the save
        public void LaunchSave()
        {
            if (!B_isRunning)
            {
                t = new Thread(new ThreadStart(SaveDirProcess));
                Nb_filecopy = 0;
                L_fileLeft = Nb_arborescenceFiles;
                L_sizeLeft = Nb_arborescenceSize;
                b_isRunning = true;
                stopper.Dispose();
                stopper = new CancellationTokenSource();
                //Boolean = true because the save is launch
                b_AlreadyLaunch = true;
                // launch save on dir
                t.Start();
            }
        }

        public abstract void SaveDirProcess();

        //Copy process
        public void SaveProcess(string item, string current_copy_dir)
        {
            // if this thread is on
            // (not global)
            threadLock.WaitOne();
            // if all thread are in pause (global thread)
            _threadsLocker.WaitOne();
            // if the file to copy is > of the size transfer available
            if ((int)new FileInfo(item).Length > (SaveViewModel.GetInstance().Nb_CopyPause) * 1000)
            {
                // block all thread and allow only this transfer
                _threadsLocker.Reset();
            }
            // else if the current copy max plus the lenght of the file to transfer is up the size transfer available
            else if ((currentCopyValue + (int)new FileInfo(item).Length) > (SaveViewModel.GetInstance().Nb_CopyPause) * 1000)
            {
                while ((currentCopyValue + (int)new FileInfo(item).Length) > (SaveViewModel.GetInstance().Nb_CopyPause) * 1000)
                {
                    Thread.Sleep(10);
                }
            }
            // add to the volatile value 
            currentCopyValue += (int)new FileInfo(item).Length;

            //Stopwatch for the copy time
            Stopwatch copyTime = new Stopwatch();
            //Start the stopwatch
            copyTime.Start();
            //Copy the file
            File.Copy(item, current_copy_dir + Path.GetFileName(item), true);
            //Stop the stopwatch
            copyTime.Stop();
            //Save the copy time
            this.l_currentFileTimeTransfer = copyTime.ElapsedMilliseconds;
            //Incremente the total time transfert
            this.l_timeTransfer += copyTime.ElapsedMilliseconds;

            //Get the file's extension
            string fileExtension = Path.GetExtension(item);
            //Launch the encryption process if the files's extension matche with the extension's list
            if (b_crypt)
            {
                if (SaveViewModel.GetInstance().L_cryptExtension.Find(x => x == Path.GetExtension(item)) != null)
                {
                    this.l_cryptTimeCurrentFile = CryptFile(item, current_copy_dir + Path.GetFileName(item));
                }
                else
                {
                    this.l_cryptTimeCurrentFile = 0;
                }
            }
            else
            {
                this.l_cryptTimeCurrentFile = 0;
            }


            // if the locker was reset
            if ((int)new FileInfo(item).Length > (SaveViewModel.GetInstance().Nb_CopyPause) * 1000)
            {
                // unreset the locker and continu all threads
                _threadsLocker.Set();
            }

            currentCopyValue -= (int)new FileInfo(item).Length;

            //Notify the observer
            NotifyObserser("log");
        }

        //Delegate for Play/Pause/Stop
        public static Del dPause = (Thread t, ManualResetEvent threadLock) =>
        {
            //Only if the save exist
            if (t != null)
            {
                //And only if it is in progress
                if (t.IsAlive)
                {
                    threadLock.Reset();
                }
            }
        };

        public static Del dRes = (Thread t, ManualResetEvent threadLock) =>
        {
            //Only if the process is not running
            if (!Save.IsProcessRunning())
            {
                //And if the save exist and is in progress
                if (t != null)
                {
                    if (t.IsAlive)
                    {
                        threadLock.Set();
                    }
                }
            }
        };

        public static DelS dStop = (Thread t, CancellationTokenSource stopper) =>
        {
            //Stop the save only if it is in progress
            if (t != null)
            {
                if (t.IsAlive)
                {
                    stopper.Cancel();
                }
            }
        };

        //Check if the specified application is running
        public static bool IsProcessRunning()
        {
            //Check if a parent process is running
            Process[] proc = Process.GetProcessesByName("notepad");
            if (proc.Length > 0)
            {
                //Pause all the save if the process is detected 
                SaveView.GetInstance().DispPause();
                return true;
            }
            return false;
        }

        //Encryption process
        public long CryptFile(string source, string dest)
        {
            //key for encryption
            string str_key = (string)MainViewModel.GetInstance().ReadJsonFile(@"..\..\..\Settings\Settings.json")["cryptKey"];
            //Execute CryptoSoft
            long encrypt = Crypt.EncryptFile(source, dest, str_key);
            //Increase Total Crypt Time with CryptoSoft's response
            l_totalCryptTime += encrypt;
            return encrypt;
        }

        //List all the file to copy and return a list with [Source1,Dest1,Source2,Dest2,...,...]
        public List<string> ListAllFiles(string current_dir, string current_copy_dir)
        {
            //Get the file's attributes
            FileAttributes attr = File.GetAttributes(str_filesSource);
            //If it's a directory
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory) 
            {
                //For each file in the directory
                foreach (var file in Directory.GetFiles(current_dir))
                {
                    //File's extension
                    string fileExtension = Path.GetExtension(file);
                    //Check if the file has priority
                    if (SaveViewModel.GetInstance().L_priorityExtension.Find(x => x == Path.GetExtension(file)) != null)
                    {
                        //Add the full path in the top of the list
                        list.Insert(0, file);
                        //Add the destination path in the second position of the list
                        //Check if the destination path is at the root 
                        //If the destination path is not a root: Path.GetDirectoryName(Str_filesDest) is not null)
                        if (Path.GetDirectoryName(Str_filesDest) != null)
                        {
                            //Insert the destination path with Path.GetDirectoryName(Str_filesDest) at the begining
                            list.Insert(1, Path.GetDirectoryName(Str_filesDest) + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
                        } else
                        //If the destination path is a root: Path.GetDirectoryName(Str_filesDest) is null)
                        {
                            //Insert the destination path with just Str_filesDest at the begining
                            list.Insert(1, Str_filesDest + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
                        }
                    }
                    else
                    {
                        //Add the full path in the list
                        list.Add(file);
                        //Add the destination path in the second position of the list
                        //Check if the destination path is at the root 
                        //If the destination path is not a root: Path.GetDirectoryName(Str_filesDest) is not null)
                        if (Path.GetDirectoryName(Str_filesDest) != null)
                        {
                            //Insert the destination path with Path.GetDirectoryName(Str_filesDest) at the begining
                            list.Add(Path.GetDirectoryName(Str_filesDest) + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
                        } else
                        //If the destination path is a root: Path.GetDirectoryName(Str_filesDest) is null)
                        {
                            //Insert the destination path with just Str_filesDest at the begining
                            list.Add(Str_filesDest + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
                        }
                    }
                }

                //For each subdirectory:
                foreach (var subdir in Directory.GetDirectories(current_dir))
                {
                    //Relaunch the method
                    if(Path.GetDirectoryName(Str_filesDest)!= null)
                    {
                        ListAllFiles(subdir, Path.GetDirectoryName(Str_filesDest) + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
                    }
                    else
                    {
                        ListAllFiles(subdir, Str_filesDest + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
                    }                  
                }
            }
            else //If it's a file
            {
                //Add the full path in the list
                list.Add(str_filesSource);
                //Add the destination path in the list
                list.Add(str_filesDest);
            }
            return list;
        }

        //Check if a directory exist
        public void IfDirExist(string path)
        {
            //check if directory exist
            if (!Directory.Exists(path))
            {
                //Create the directory if it doesn't exist
                Directory.CreateDirectory(path);
            }
        }

        public bool IfSourceFileExist(string path)
        {
            //check if directory exist
            if (File.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Get total size to transfer off the arborescence (directory and subdirectories)
        public long[] GetTotalSizeArborescence(string current_dir, string current_copy_dir)
        {
            //Get all the files in the current directory
            string[] files = Directory.GetFiles(current_dir);
            //For each files in the directory
            foreach (var str_nb in files)
            {
                //Increase the number off file (to have the total files number in the arborescence)
                nb_arborescenceFiles += 1;
                //Increase the total size
                nb_arborescenceSize += new FileInfo(str_nb).Length;
            }
            //Check if there are subdirectories
            string[] dir = Directory.GetDirectories(current_dir);
            //For each subdirectories
            foreach (var item in dir)
            {
                //Recall the method
                GetTotalSizeArborescence(item, Path.GetDirectoryName(Str_filesDest) + "\\" + current_dir.Substring(Str_filesSource.LastIndexOf(@"\") + 1) + @"\");
            }

            long[] res = { nb_arborescenceSize, nb_arborescenceFiles };
            return res;
        }

        //All the setter/getter
        public string Str_filesSource
        {
            get { return this.str_filesSource; }
            internal set { this.str_filesSource = value; NotifyPropertyChanged(); }
        }
        public string Str_filesDest
        {
            get { return this.str_filesDest; }
            internal set { this.str_filesDest = value; NotifyPropertyChanged(); }
        }
        public string Str_name
        {
            get { return this.str_name; }
            internal set { this.str_name = value; NotifyPropertyChanged(); }
        }
        public string Str_currentCopyFile
        {
            get { return this.str_currentCopyFile; }
            internal set { this.str_currentCopyFile = value; NotifyPropertyChanged(); }
        }
        public string Str_currentCopyFileDestination
        {
            get { return this.str_currentCopyFileDestination; }
            internal set { this.str_currentCopyFileDestination = value; NotifyPropertyChanged(); }
        }

        public long Nb_arborescenceSize
        {
            get { return nb_arborescenceSize; }
            set { nb_arborescenceSize = value; NotifyPropertyChanged(); }
        }
        public long L_fileLeft
        {
            get { return this.l_fileLeft; }
            internal set { this.l_fileLeft = value; NotifyPropertyChanged(); }
        }
        public long L_sizeLeft
        {
            get { return this.l_sizeLeft; }
            internal set { this.l_sizeLeft = value; NotifyPropertyChanged(); }
        }
        public long L_totalCryptTime
        {
            get { return this.l_totalCryptTime; }
            internal set { this.l_totalCryptTime = value;}
        }
        public long L_cryptTimeCurrentFile
        {
            get { return this.l_cryptTimeCurrentFile; }
            internal set { this.l_cryptTimeCurrentFile = value;}
        }
        public long L_timeTransfer
        {
            get { return this.l_timeTransfer; }
            internal set { this.l_timeTransfer = value;}
        }
        public long L_currentFileTimeTransfer
        {
            get { return this.l_currentFileTimeTransfer; }
            internal set { this.l_currentFileTimeTransfer = value;}
        }

        public int Nb_filecopy
        {
            get { return this.nb_filecopy; }
            internal set { this.nb_filecopy = value; }
        }
        public int Nb_arborescenceFiles
        {
            get { return this.nb_arborescenceFiles; }
            internal set { this.nb_arborescenceFiles = value; }
        }
        public int Nb_progression
        {
            get { return this.nb_progression; }
            internal set { this.nb_progression = value; NotifyPropertyChanged(); }
        }

        public bool B_crypt
        {
            get { return this.b_crypt; }
            internal set { this.b_crypt = value;}
        }
        public bool B_state
        {
            get { return this.b_state; }
            internal set { this.b_state = value; }
        }
        public bool B_AlreadyLaunch
        {
            get { return this.b_AlreadyLaunch; }
            internal set { this.b_AlreadyLaunch = value; }
        }
        public bool B_type
        {
            get { return this.b_type; }
            internal set { this.b_type = value; }
        }

        public CancellationTokenSource Stopper
        {
            get { return this.stopper; }
        }

        public Thread T
        {
            get { return this.t; }
        }

        public ManualResetEvent ThreadLock
        {
            get { return this.threadLock; }
        }

        public bool B_isRunning
        {
            get { return this.b_isRunning; }
            set { this.b_isRunning = value; }
        }

        public List<string> List
        {
            get { return this.list; }
            set { this.list = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void NotifyObserser(string str_type)
        {
            // notify the action of the log
            SaveViewModel.GetInstance().NotifyObservers(this, str_type);
        }
    }
}
