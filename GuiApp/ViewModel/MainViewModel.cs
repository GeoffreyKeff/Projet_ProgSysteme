using GuiApp.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GuiApp.ViewModel
{
    class MainViewModel
    {
        // variable of the choose language
        private static string str_chooseLanguage;
        // the instance for the singleton patern
        private static MainViewModel instance;
        // the saveViewModel instance
        private static readonly SaveViewModel saveInstance = SaveViewModel.GetInstance();
        // the different view instance
        private static readonly StateViewModel stateInstance = StateViewModel.GetInstance();

        // private constructor for singleton
        private MainViewModel() 
        {
            // register observer
            saveInstance.RegisterObserver(LogViewModel.getInstance());
            saveInstance.RegisterObserver(StateViewModel.GetInstance());
            saveInstance.RegisterObserver(SocketServer.GetInstance()) ;
            // call the method for initialized app attributes
            InitializedValues();
        }

        // initialized the different value from settings
        public void InitializedValues()
        {
            // initialized language
            str_chooseLanguage = (string)ReadJsonFile(@"..\..\..\Settings\Settings.json")["language"];
            // initialized log path
            LogViewModel.getInstance().Str_path = (string)ReadJsonFile(@"..\..\..\Settings\Settings.json")["dirlog"];
            // initialized log type
            LogViewModel.getInstance().B_isJson = (string)ReadJsonFile(@"..\..\..\Settings\Settings.json")["type"] == ".json";
            // initialized state path
            StateViewModel.GetInstance().Str_path = (string)ReadJsonFile(@"..\..\..\Settings\Settings.json")["dirstate"];
            // initialized list of extension
            JObject test = (JObject)ReadJsonFile(@"..\..\..\Settings\Settings.json")["filetype"];
            for(int i = 0; i < test.Count; i++)
            {
                // add to the list
                SaveViewModel.GetInstance().L_cryptExtension.Add(test[i.ToString()].ToString());
            }
            // initialized list of extension
            JObject test2 = (JObject)ReadJsonFile(@"..\..\..\Settings\Settings.json")["filePrioritytype"];
            for (int i = 0; i < test2.Count; i++)
            {
                // add to the list
                SaveViewModel.GetInstance().L_priorityExtension.Add(test2[i.ToString()].ToString());
            }

            SaveViewModel.GetInstance().Nb_CopyPause = (int)ReadJsonFile(@"..\..\..\Settings\Settings.json")["nbCopyPause"];

            //Start the listenning
            SocketServer.GetInstance().StartListening();
        }

        //DP Singleton
        public static MainViewModel GetInstance()
        {
            // if the instance is not initialized
            if (instance == null)
            {
                // initialization
                instance = new MainViewModel();
            }
            // return the only instance
            return instance;
        }

        
        // insert the save in save list
        public void InsertInL_save()
        {
            if (File.Exists(stateInstance.Str_path + "process.json"))
            {
                // create a new save
                List<Save> l_save = new List<Save>();
                // open the file
                using (StreamReader file = File.OpenText(Path.GetFullPath(stateInstance.Str_path + "process.json")))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    // foreach save in state file
                    foreach (var o in (JObject)JToken.ReadFrom(reader))
                    {
                        if(Directory.Exists(o.Value["FileSource"].ToString()) || File.Exists(o.Value["FileSource"].ToString()))
                        {
                            // if the save is active
                            Save save;
                            bool active = false;
                            if (o.Value["State"].ToString() == "ACTIVE")
                            {
                                active = true;
                            }
                            bool IsCrypt = true;
                            // if the save is complete's type
                            if (o.Value["Type"].ToString() == "Complete")
                            {
                                save = new SaveProcess(o.Value["FileSource"].ToString(), o.Value["FileDestination"].ToString(), o.Value["Name"].ToString(), (bool)o.Value["CryptFormat"], true);
                                save.B_state = active;
                                // add save to the list
                                l_save.Add(save);
                            }
                            // if the save is dif's type
                            else
                            {
                                save = new SaveProcess(o.Value["FileSource"].ToString(), o.Value["FileDestination"].ToString(), o.Value["Name"].ToString(), (bool)o.Value["CryptFormat"], false);
                                save.B_state = active;
                                // add save to the list
                                l_save.Add(save);
                            }
                        }
                    }
                    // put in saveVM's the initialized list
                    saveInstance.L_saves = l_save;
                }
                // notify state that a save was create
                saveInstance.L_saves.ForEach(item => saveInstance.NotifyObservers(item, "state"));
            }
        }

        // read the json file
        public JObject ReadJsonFile(string path)
        {
            using StreamReader file = File.OpenText(path);
            using JsonTextReader reader = new JsonTextReader(file);
            // return the json as an object
            JObject o2 = (JObject)JToken.ReadFrom(reader);
            return o2;
        }

        // get the ask value in json file
        public string GetValueInJson(string str_s)
        {
            // return as string the value in the select language
            return (string)ReadJsonFile(Path.GetFullPath(@"..\..\..\Languages\languages.json"))[str_chooseLanguage][str_s];
        }

        // write in the json file for the settings
        public void WriteSettings(string path, string str_key, string str_value)
        {
            // get the object of the current settings file
            JObject o1 = JObject.Parse(File.ReadAllText(path));
            // if the value to insert is filetype
            if (str_key == "filetype")
            {
                // create new object for the current filetype
                JObject res = new JObject();
                for(int i = 0; i < SaveViewModel.GetInstance().L_cryptExtension.Count; i++)
                {
                    // add in the object each file type
                    res.Add(i.ToString(), SaveViewModel.GetInstance().L_cryptExtension[i]);
                }  
                // add to the current object the new object tab
                o1[str_key] = res;
            } else if(str_key == "filePrioritytype")
            {
                // create new object for the current filetype
                JObject res = new JObject();
                for (int i = 0; i < SaveViewModel.GetInstance().L_priorityExtension.Count; i++)
                {
                    // add in the object each file type
                    res.Add(i.ToString(), SaveViewModel.GetInstance().L_priorityExtension[i]);
                }
                // add to the current object the new object tab
                o1[str_key] = res;
            }
            else
            {
                // add to the current object the new object tab
                o1[str_key] = str_value;
            } 
            // write in the settings file new settings
            File.WriteAllText(path, o1.ToString());
        }
        
        // choose language getter an setter
        public static string Str_chooseLanguage
        {
            get { return str_chooseLanguage; }
            set { str_chooseLanguage = value; }
        }
    }
}
