using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GuiApp.Model
{
    class State
    {
        //The files destination
        private string str_fileDestination { get; set; }
        //The files source
        private string str_fileSource { get; set; }
        //The name of the save
        private string str_name { get; set; }
        //The size of files left
        private long nb_progression { get; set; }
        //The number of files left
        private int nb_filesLeft { get; set; }
        //The size Left
        private long nb_sizeFiles { get; set; }
        //Number of files
        private int nb_files { get; set; }
        //State of the save
        private bool state { get; set; }
        //Type of the save (complete/diferential)
        private int nb_type;
        //Crypt the save or not
        private bool b_crypt;

        //Constructor
        public State(string fileDest, string fileSource, string name, long progress, int filesLeft, long sizeFiles, int nb_files, bool state, int type, bool crypt)
        {
            //File's destination
            this.str_fileDestination = fileDest;
            //File's source
            this.str_fileSource = fileSource;
            //Save's name
            this.str_name = name;
            //Save's progression
            this.nb_progression = progress;
            //Files left
            this.nb_filesLeft = filesLeft;
            //Size left
            this.nb_sizeFiles = sizeFiles;
            //Number of total files
            this.nb_files = nb_files;
            //Save's state
            this.state = state;
            //Save's type
            this.nb_type = type;
            //Crypt or not
            this.b_crypt = crypt;
        }

        //Create the json
        public JObject createJson()
        {
            JObject res = new JObject();
            res.Add("Name", this.str_name);
            if(this.nb_type == 0)
            {
                res.Add("Type", "Complete");
            } else
            {
                res.Add("Type", "Dif");
            }
            res.Add("FileSource", this.str_fileSource);
            res.Add("FileDestination", this.str_fileDestination);
            if(this.state)
            {
                res.Add("State", "ACTIVE");
            } else
            {
                res.Add("State", "END");
            }
            res.Add("CryptFormat", this.b_crypt);
            res.Add("TotalFilesToCopy", this.nb_files);
            res.Add("TotalFilesSize", this.nb_sizeFiles);
            res.Add("NbFilesLeftToDo", this.nb_filesLeft);
            res.Add("Progression", this.nb_progression);
            return res;
        }

        //All the setter/getter
        public string Str_name
        {
            get { return this.str_name; }
            set { this.str_name = value; }
        }

        public long Nb_progression
        {
            get { return this.nb_progression; }
            set { this.nb_progression = value; }
        }

        public int Nb_filesLeft
        {
            get { return this.nb_filesLeft; }
            set { this.nb_filesLeft = value; }
        }

        public bool GetState()
        { return this.state; }

        public void SetState(bool value)
        { this.state = value; }
    }
}
