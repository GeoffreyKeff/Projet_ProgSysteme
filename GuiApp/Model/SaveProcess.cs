using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using GuiApp.ViewModel;

namespace GuiApp.Model
{
    class SaveProcess : Save {

        //---Constructor
        public SaveProcess(string fileSource, string fileDestination, string name, bool crypt, bool saveType) : base(fileSource, fileDestination, name, crypt, saveType) { }

        //---Backup process
        public override void SaveDirProcess()
        {
            try
            {
                List = new List<string>();
                //Get the list with all the sources/destination path
                List<string> list = ListAllFiles(Str_filesSource, Str_filesDest);
                //Total size copied
                long nb_size = 0;
                //Browse all the list ( even = source path / odd = destination path )
                for (int i =0; i < list.Count; i+=2)
                {
                    //Check if the thread is stop
                    if(Stopper.IsCancellationRequested)
                    {
                        return;
                    }
                    //Check if a parent process is running and pause the save if it's running
                    if(IsProcessRunning())
                    {
                        Save.dPause(this.T, this.ThreadLock);
                    }
                    try
                    {
                        //Asign the current copy file destination and source (for the log)
                        this.Str_currentCopyFile = list[i];
                        this.Str_currentCopyFileDestination = list[i + 1];
                        //Check if the destination path exist
                        IfDirExist(list[i + 1]);

                        if (!IfSourceFileExist(list[i])) {
                            SaveView.GetInstance().CopyError();
                            Save.dPause(this.T, this.ThreadLock);
                        }

                        //Check if the save is complete or differential
                        if (B_type)
                        {
                            //Launch the save process
                            SaveProcess(list[i], list[i + 1]);
                        }
                        //If the save is differential
                        else
                        {
                            //Get the files infos from file to copy and already copied file
                            FileInfo fileDestInfo = new FileInfo(list[i + 1] + Path.GetFileName(list[i]));
                            FileInfo fileSourceInfo = new FileInfo(list[i]);
                            //Copy only if the file has been modified after the last save
                            if ((fileSourceInfo.LastWriteTime > fileDestInfo.LastWriteTime))
                            {
                                //Launch the save process
                                SaveProcess(list[i], list[i + 1]);
                            }
                        }

                        //Variable for the number of copied files
                        Nb_filecopy++;
                        //Add the file size to total copied files size
                        nb_size += new FileInfo(list[i]).Length;
                        //Calculate the number of files left
                        this.L_fileLeft = Nb_arborescenceFiles - Nb_filecopy;
                        //Calculate the size left
                        this.L_sizeLeft = Nb_arborescenceSize - nb_size;
                        //B_state = true because she save is still running
                        this.B_state = true;
                        //Calculate the progression for the progression's bar
                        this.Nb_progression = (int)(((this.Nb_arborescenceSize - this.L_sizeLeft) * 100) / this.Nb_arborescenceSize);
                        //Notify the observer
                        NotifyObserser("state");
                    }
                    catch
                    {
                        //if the file has not been copied, the copied files size is not incremented
                        nb_size += 0;
                    }
                }
                //When the save is finish:
                //Progression is 100%
                this.Nb_progression = 100;
                //0 file left
                this.L_fileLeft = 0;
                //Size left = 0
                this.L_sizeLeft = 0;
                //B_state = false because the save is ending
                this.B_state = false;

                //Notify the log
                SaveViewModel.GetInstance().NotifyObservers(this, "finalLog");
                //Notify the observer
                NotifyObserser("state");
                B_isRunning = false;
                B_AlreadyLaunch = false;
            }
            catch (IOException error)
            {
                //Return an error and a null size if the copy terminated with an error
                Console.WriteLine(error.Message);
            }
        }
    }
}
