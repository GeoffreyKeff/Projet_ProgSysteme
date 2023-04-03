using ClientGUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //list of all curent saves recieve from the server
        List<Save> L_saves;

        //Byte array for data recieve
        private static byte[] temp;

        //Socket
        private static Socket connectingSocket;

        public MainWindow()
        {
            InitializeComponent();
            L_saves = new List<Save>();
        }

        //Method for the bouttons
        private void TryToConnect_Click(object sender, RoutedEventArgs e)
        {
            StateUpdate("Try to connect...");
            //try to connect to the server
            TryToConnect();
            //start receiving data from the servers
            StartReceiving();
            //Send(dataSend);
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            //Button to disconnect the socket
            Disconnect();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (saveList.SelectedItems.Count > 0)
            {
                foreach (Save s in saveList.SelectedItems)
                {
                    Send(s.Str_name + "<PLAY>");
                }
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (saveList.SelectedItems.Count > 0)
            {
                foreach (Save s in saveList.SelectedItems)
                {
                    Send(s.Str_name + "<STOP>");
                }
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (saveList.SelectedItems.Count > 0)
            {
                foreach (Save s in saveList.SelectedItems)
                {
                    Send(s.Str_name + "<PAUSE>");
                }
            }
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            //Disconnect before quit (to avoid errors on the server side)
            Disconnect();
            Environment.Exit(0);
        }

        //Methods to update the state display
        public void StateUpdate(string text)
        {
            StateText.Dispatcher.BeginInvoke(new Action(() =>
            {
                StateText.Content = text;
            }));

        }

        public void RecieveTimeUpdate()
        {
            StateText_Time.Dispatcher.BeginInvoke(new Action(() =>
            {
                StateText_Time.Content = "Last reception: " + DateTime.Now;
            }));
        }

        //Methode for the socket connection
        public void TryToConnect()
        {
            //Server's IP
            string str_ip = "127.0.0.1";

            //Socket infos
            IPHostEntry host = Dns.GetHostEntry(str_ip);
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            //Creating the socket
            connectingSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                StateUpdate("Try to connect...");
                //Try to connect
                connectingSocket.Connect(remoteEP);
                //Confirm the connection
                StateUpdate("Socket connected to: " + remoteEP);
            }
            catch {
                StateUpdate("Error to connect");
            }
        }

        public void Send(string data)
        {
            //before send, check if the socket is connected
            if (connectingSocket.Connected)
            {
                try
                {
                    //List of bytes
                    var fullPacket = new List<byte>();

                    //Convert data to an array of bytes
                    fullPacket.AddRange(BitConverter.GetBytes(data.Length));
                    fullPacket.AddRange(Encoding.Default.GetBytes(data));

                    //Send to the client
                    connectingSocket.Send(fullPacket.ToArray());
                }
                //If there are a exception, check if the socket is always connected
                catch (Exception)
                {
                    if (!connectingSocket.Connected)
                    {
                        Disconnect();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }

            }
        }

        public void StartReceiving()
        {
            try
            {
                // the first 4 bytes we reserve for the lenght of the data 
                temp = new byte[4];

                //start receiving from connected server and when it will receive data it will run the ReceiveCallback function.
                connectingSocket.BeginReceive(temp, 0, temp.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch { }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                //Check is the socket is created and connected
                if (connectingSocket != null && connectingSocket.Connected)
                {
                    //Convert the first 4 bytes (int 32) that we received and convert it to an Int32.
                    temp = new byte[BitConverter.ToInt32(temp, 0)];

                    //Recieve the data
                    connectingSocket.Receive(temp, temp.Length, SocketFlags.None);

                    // Convert the bytes to string
                    string dataRecieve = Encoding.Default.GetString(temp);
                    RecieveTimeUpdate();
                    //Converte the Recieve Data into a JObject
                    ConvertToJson(dataRecieve);
                    //start all over again with waiting for a data to come from the socket.
                    StartReceiving();
                }
            } catch
            {
                if (connectingSocket == null || !connectingSocket.Connected)
                {
                    //If the connection is terminated without action from the client, the server has terminated the connection.
                    StateUpdate("The server has terminated the connection");
                    L_saves.Clear();
                    PrintAllSaves();
                }
                else
                {
                    //Else, there has been a transmission error so we start listening again
                    StartReceiving();
                }
            }
        }

        private void Disconnect()
        {
            //Disconnect only if a connection is active (to avoid error trying to disconnect a socket that is not connected)
            if (connectingSocket != null && connectingSocket.Connected)
            {
                // Close connection
                connectingSocket.Disconnect(true);
                //Put the socket to null (to avoid a recieve infinite loop)
                connectingSocket = null;

                //Clear the save list and update the saves list
                L_saves.Clear();
                PrintAllSaves();

                StateUpdate("Disconnected from the server");
            }

        }

        //Methods for manipulate the recieve data
        private void PrintAllSaves()
        {
            //Print all saves list
            saveList.Dispatcher.BeginInvoke(new Action(() =>
            {
                saveList.ItemsSource = new ObservableCollection<Save>(L_saves);
            }));

        }

        private void ConvertToJson(string recieve)
        {
            //Convert the list
            JObject json = JObject.Parse(recieve);

            //For each json in the Jobject
            for (int i = 1; i < json.Count + 1; i++)
            {
                
                //If the save is already in the list
                if (L_saves.Find(save => save.Str_name == (string)json["Save" + i]["Name"]) != null)
                {
                    //Just replace the values
                    Save s1 = L_saves.Find(save => save.Str_name == (string)json["Save" + i]["Name"]);
                    s1.Str_name = (string)json["Save" + i]["Name"];
                    s1.Str_filesSource = (string)json["Save" + i]["FileSource"];
                    s1.Str_filesDest = (string)json["Save" + i]["FileDestination"];
                    s1.Progression = (int)json["Save" + i]["Progression"];
                    s1.TotalFilesToCopy = (long)json["Save" + i]["TotalFilesToCopy"];
                    s1.NbFilesLeftToDo = (long)json["Save" + i]["NbFilesLeftToDo"];

                } else
                {
                    //If the save is not in the list
                    //Create the save 
                    Save s = new Save((string)json["Save" + i]["Name"], (string)json["Save" + i]["FileSource"], (string)json["Save" + i]["FileDestination"], (long)json["Save" + i]["TotalFilesSize"], (int)json["Save" + i]["Progression"], (long)json["Save" + i]["TotalFilesToCopy"], (long)json["Save" + i]["NbFilesLeftToDo"]);
                    //Add the save to the list
                    L_saves.Add(s);
                }

            }

            //Check if a save is deleted comparated to the recieve json before actualise
            CheckList(json);
            
        }

        public void CheckList(JObject json)
        {
            //Check if a save is delete

            //Bool for the save exist
            bool isHere;
            //For each save in the list, check if she is in the json
            foreach (Save s in L_saves)
            {
                //By default, we considere that the save doesn't exist
                isHere = false;
                for (int j = 1; j < json.Count + 1; j++)
                {
                    if (s.Str_name == (string)json["Save" + j]["Name"])
                    {
                        //If the save exist, the boolean is set to true
                        isHere = true;
                    }
                }

                //After browse the list, if the boolean is false, delete the save from the list
                if (!isHere)
                {
                    L_saves.Remove(s);
                }

                //Print all saves
                PrintAllSaves();
            }
        }
    }
}












