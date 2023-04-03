using GuiApp.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace GuiApp.ViewModel
{
    class LogViewModel : IObserver
    {
        // the path of the log file
        private static string str_path;
        // the only instance of this class
        private static LogViewModel instance;
        // if the log file is in json
        private static bool b_isJson;

        private static Mutex lockerMutex = new Mutex();

        // constructor
        private LogViewModel() { }

        //DP Singleton
        public static LogViewModel getInstance()
        {
            if (instance == null)
            {
                instance = new LogViewModel();
            }
            return instance;
        }

        // choose if the file is in json or in xml
        public void CreateOrInsterLog(Log l)
        {
            // check if the log's directory exist or not and create it
            CreateLogDirectory();
            // check if we need to insert log in xml or in json
            if (b_isJson)
            {
                // write in json the log
                WriteInJson(l);
            }
            else
            {
                // write in xml the log
                WriteInXml(l);
            }
        }

        // write the log in xml
        public void WriteInXml(Log l)
        {
            lockerMutex.WaitOne();
            XDocument doc;
            // if the log doesn't exist
            if (!IsLogExist())
            {
                // create new xml file
                doc = new XDocument();
                // add in xml file the new element saves
                doc.Add(new XElement("Saves"));
            }
            else
            {
                // if log exist we just load the today's log
                doc = XDocument.Load(str_path + "LOG_" + DateTime.Today.ToString("ddMMyyyy") + ".xml"); 
            }
            // add the log generation in xml file
            doc.Root.Add(l.CreateXml());
            // save the doc
            doc.Save(str_path + "LOG_" + DateTime.Today.ToString("ddMMyyyy") + ".xml");
            lockerMutex.ReleaseMutex();
        }

        // method for write in the json file
        public void WriteInJson(Log l)
        {
            lockerMutex.WaitOne();
            string str_todayFile = str_path + "LOG_" + DateTime.Today.ToString("ddMMyyyy") + ".json";
            // if the log already exist
            if (!IsLogExist())
            {
                // create the today's log file
                FileStream fs = File.Create(str_todayFile);
                // write in file the different log informations
                Byte[] text = new UTF8Encoding(true).GetBytes(l.CreateJson());
                fs.Write(text, 0, text.Length);
                // close today's file
                fs.Close();
            }
            // if the log doesn't exist
            else
            {      
                // open file
                using (FileStream aFile = new FileStream(str_todayFile, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(aFile))
                {
                    // write at the end of the log file
                    sw.WriteLine(l.CreateJson());
                } 
            }
            lockerMutex.ReleaseMutex();
        }

        // check if the today's log exist
        public bool IsLogExist()
        {
            // get all log files in directory path
            string[] files = Directory.GetFiles(str_path);
            // foreach file
            foreach (var item in files)
            {
                string str_ext = ".xml";
                if (b_isJson)
                {
                    str_ext = ".json";
                }
                // if the file contain today's log
                if (File.GetCreationTime(item).ToString("dd/MM/yyyy") == DateTime.Today.ToString("dd/MM/yyyy") && Path.GetExtension(item) == str_ext)
                {
                    // the log already exist
                    return true;
                }
            }
            // the log doesn't exist
            return false;
        }

        // create rep with log
        public void CreateLogDirectory()
        {
            // if not the directory path exist
            if (!Directory.Exists(str_path))
            {
                // create the directory
                Directory.CreateDirectory(str_path);
            }
        }

        // catch the notification from the saveViewModel
        public void Notify(Save save, string str_type)
        {
            // if the type of the notification is log
            if(str_type == "log")
            {
                // just insert in log file the current value of copy file
                CreateOrInsterLog(new Log(save.Str_name, save.Str_currentCopyFile, save.Str_currentCopyFileDestination, new FileInfo(save.Str_currentCopyFile).Length, save.L_currentFileTimeTransfer, save.L_cryptTimeCurrentFile));
            } else if(str_type == "finalLog")
            {
                // insert the final value of the log
                CreateOrInsterLog(new Log(save.Str_name, save.Str_filesSource, save.Str_filesDest, save.Nb_arborescenceSize, save.L_timeTransfer, save.L_totalCryptTime));
            }    
        }

        // get or set the log type
        public bool B_isJson
        {
            get { return b_isJson; }
            set { b_isJson = value; }
        }

        // get or set the path of the log file
        public string Str_path
        {
            get { return str_path; }
            set { str_path = value; }
        }
    }
}
