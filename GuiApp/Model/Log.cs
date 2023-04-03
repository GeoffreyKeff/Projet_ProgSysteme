using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GuiApp.Model
{
    // class log
    class Log
    {
        // the name
        private string str_name { get; set; }
        // the file source
        private string str_fileSource { get; set; }
        // the file destination
        private string str_fileDestination { get; set; }
        // the size of the files
        private long nb_size { get; set; }
        // the time of transfer
        private long fl_transfer { get; set; }
        //Copy's date
        private string dt_create;
        //Crypt time
        private long cryptTime;

        // constructor for create a Log
        public Log(string str_name, string str_fileSource, string str_fileDest, long nb_size, long l_timeTransfer, long cryptT)
        {
            //Save's name
            this.str_name = str_name;
            //File's source
            this.str_fileSource = str_fileSource;
            //File's destination
            this.str_fileDestination = str_fileDest;
            //File's size
            this.nb_size = nb_size;
            //File's time transfer
            this.fl_transfer = l_timeTransfer;
            //Copy's date
            this.dt_create = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            //Crypt Time
            this.cryptTime = cryptT;
        }

        //Export this log to json format
        public string CreateJson()
        {
            // create a jobject
            JObject res = new JObject();
            res.Add("Name", this.str_name);
            res.Add("FileSource", this.str_fileSource);
            res.Add("FileDestination", this.str_fileDestination);
            res.Add("FileSize", this.nb_size);
            res.Add("FileTransferTime", this.fl_transfer);
            res.Add("CryptTime", this.cryptTime);
            res.Add("Time", this.dt_create);
            // convert jobject in string
            string str_res2 = res.ToString() + ",\n\n";
            return str_res2;
        }

        // create an xml element for the log
        public XElement CreateXml()
        {
            // create new Xml element and return it
            return new XElement(new XElement(this.str_name,
                                    new XElement("Name", this.str_name),
                                    new XElement("FileSource", this.str_fileSource),
                                    new XElement("FileDestination", this.str_fileDestination),
                                    new XElement("FileSize", this.nb_size),
                                    new XElement("FileTransferTime", this.fl_transfer),
                                    new XElement("CryptTime", this.cryptTime),
                                    new XElement("Time", this.dt_create)
                                    ));
        }
    }
}
