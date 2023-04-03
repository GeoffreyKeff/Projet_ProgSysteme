using System;
using System.Collections.Generic;
using System.Text;

namespace ClientGUI
{
    class Save
    {
        private string str_name;
        private string str_filesSource;
        private string str_filesDest;
        private long nb_arborescenceSize;
        private int progression;
        private long totalFilesToCopy;
        private long nbFilesLeftToDo;

        public Save(string str_name, string str_fileSource, string str_fileDest, long l_TotalFilesSize, int progression, long totalFilesToCopy, long nbFilesLeftToDo)
        {
            this.str_name = str_name;
            this.str_filesSource = str_fileSource;
            this.str_filesDest = str_fileDest;
            this.nb_arborescenceSize = l_TotalFilesSize;
            this.progression = progression;
            this.totalFilesToCopy = totalFilesToCopy;
            this.nbFilesLeftToDo = nbFilesLeftToDo;
        }

        public string Str_filesSource
        {
            get { return this.str_filesSource; }
            internal set { this.str_filesSource = value; }
        }

        public string Str_filesDest
        {
            get { return this.str_filesDest; }
            internal set { this.str_filesDest = value; }
        }

        public string Str_name
        {
            get { return this.str_name; }
            internal set { this.str_name = value; }
        }

        public long Nb_arborescenceSize
        {
            get { return nb_arborescenceSize; }
            set { nb_arborescenceSize = value; }
        }

        public int Progression
        {
            get { return progression; }
            set { progression = value; }
        }

        public long TotalFilesToCopy
        {
            get { return totalFilesToCopy; }
            set { totalFilesToCopy = value; }
        }

        public long NbFilesLeftToDo
        {
            get { return nbFilesLeftToDo; }
            set { nbFilesLeftToDo = value; }
        }
    }
}
