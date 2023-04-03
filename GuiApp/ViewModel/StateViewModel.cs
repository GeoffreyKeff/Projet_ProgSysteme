using GuiApp.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace GuiApp.ViewModel
{
    class StateViewModel : IObserver
    {
        // the path of state file
        private static string str_path;
        // the list of save's state
        private static List<State> l_states = new List<State>();
        // the instance of this file
        private static StateViewModel instance;

        private static Object obj = new Object();

        // the private constructor
        private StateViewModel() { }

        //DP Singleton
        public static StateViewModel GetInstance()
        {
            // if the instance doesn't exist
            if (instance == null)
            {
                // create the instance
                instance = new StateViewModel();
            }
            // return the instance of this class
            return instance;
        }

        //Convert the State's file into a string to send it to the client
        public string ConvertToString()
        {
            //Convert the state's JSON file into a string (to send it to the client)
            JObject o1 = JObject.Parse(File.ReadAllText(str_path + "process.json"));
            return o1.ToString();
        }

        // method for write in file
        public void UpdateFile()
        {
            lock(obj)
            {
                // check if the folder exist
                IsFolderExist();
                // check if the file exist
                IsFileExist();

                using (var fss = new FileStream(str_path + "process.json", FileMode.Truncate))
                { }
                // create new object
                JObject res = new JObject();

                // open the file
                FileStream fs = File.OpenWrite(str_path + "process.json");
                int nb_i = 1;
                // foreach state
                foreach (State s in l_states)
                {
                    // add the save
                    res.Add("Save" + nb_i, s.createJson());
                    nb_i++;
                }
                // create a byte table
                Byte[] text = new UTF8Encoding(true).GetBytes(res.ToString());
                fs.Write(text, 0, text.Length);
                fs.Close();
            }
        }

        // check if the source folder exist
        public void IsFolderExist()
        {
            // if the directory doesn't exist
            if (!Directory.Exists(str_path))
            {
                // create the directory
                Directory.CreateDirectory(str_path);
            }
        }

        // check if the source file exist
        public void IsFileExist()
        {
            // if the file doesn't exist
            if (!File.Exists(str_path + "process.json"))
            {

                // create a new file with empty data
                JObject j = new JObject();
                using StreamWriter file = File.CreateText(str_path + "process.json");
                using JsonTextWriter writer = new JsonTextWriter(file);
                // write empy data in the file
                j.WriteTo(writer);

            }
        }

        // get the notification from the SaveViewModel
        public void Notify(Save save, string str_type)
        {
            // if the notify call a save was delete
            if (str_type == "deleteSave")
            {
                // delete the state for the save
                l_states.RemoveAll(state => state.Str_name == save.Str_name);
            }
            else
            {
                // init a bool
                bool exist = false;
                // check if the state already exist in the list
                foreach (State st in l_states)
                {
                    // if we find the state
                    if (st.Str_name == save.Str_name)
                    {
                        // we juste update the different values
                        st.Nb_filesLeft = (int)save.L_fileLeft;
                        st.Nb_progression = save.Nb_progression;
                        st.SetState(save.B_state);
                        exist = true;
                    }
                }
                // if the state doesn't exist
                if (!exist)
                {
                    // check if the save is type of savecomplet or savedif
                    if (save.B_type == true)
                    {
                        // add the new state to the list
                        l_states.Add(new State(save.Str_filesDest, save.Str_filesSource, save.Str_name, save.Nb_progression, (int)save.L_fileLeft, save.Nb_arborescenceSize, save.Nb_arborescenceFiles, save.B_state, 0, save.B_crypt));
                    }
                    else
                    {
                        // add the new state to the list
                        l_states.Add(new State(save.Str_filesDest, save.Str_filesSource, save.Str_name, save.Nb_progression, (int)save.L_fileLeft, save.Nb_arborescenceSize, save.Nb_arborescenceFiles, save.B_state, 1, save.B_crypt));
                    }
                }
            }
            // update state file
            UpdateFile();
        }

        //Getter/Setter
        public string Str_path
        {
            get { return str_path; }
            set { str_path = value; }
        }
    }
}
