using GuiApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;

namespace GuiApp.ViewModel
{
    class SocketServer : IObserver
    {
        //Byte array for data recieve
        private static byte[] temp;
        //Socket that will listen to any incoming connections
        private static Socket listenerSocket;
        //Socket for the client
        private static Socket acceptedSocket;

        //DP Singleton
        private static SocketServer instance;
        public static SocketServer GetInstance()
        {
            if (instance == null)
            {
                instance = new SocketServer();
            }
            return instance;
        }

        //Constructor
        private SocketServer()
        {

        }

        //Start listening for clent's connection
        public void StartListening()
        {
            //Socket infos
            IPHostEntry host = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            //Create the listening socket
            listenerSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //binds the socket to an IPEndPoint.
            //contains the host and local or remote port information needed by an application to connect to a service on a host.
            listenerSocket.Bind(localEndPoint);

            //number of incoming connections that can be queued for acceptance.
            listenerSocket.Listen(10);

            //The server will start listening for incoming connections and will go on with other logic.
            //When there is an connection the server switches back to this method and will run the AcceptCallBack methodt
            listenerSocket.BeginAccept(AcceptCallback, listenerSocket);
        }

        //Accept connection
        public static void AcceptCallback(IAsyncResult AR)
        {
            //Accept the client's connection
            acceptedSocket = listenerSocket.EndAccept(AR);
            //start listening again when the socket is done
            listenerSocket.BeginAccept(AcceptCallback, listenerSocket);

            //The first transmission is the saves list
            string dataSend = StateViewModel.GetInstance().ConvertToString();
            //Send to the client
            Send(dataSend);
            //Start listenning from client's recieve data
            StartReceiving();
        }

        //Send datas
        public static void Send(string data)
        {
            //before send, check if the socket is connected
            if (acceptedSocket != null)
            {
                try
                {
                    //List of bytes
                    var fullPacket = new List<byte>();

                    //Convert data to an array of bytes
                    fullPacket.AddRange(BitConverter.GetBytes(data.Length));
                    fullPacket.AddRange(Encoding.Default.GetBytes(data));

                    //Send to the client
                    acceptedSocket.Send(fullPacket.ToArray());
                }
                //If there are a exception, check if the socket is always connected
                catch (Exception)
                {
                    if (!acceptedSocket.Connected)
                    {
                        //Check if the connection is always active, if it is not, disconnect
                        Disconnect();
                    }
                    else
                    {
                        //Else, there has been an error
                        throw new Exception();
                    }
                }

            }
        }

        //listen for receiving datas from client
        public static void StartReceiving()
        {
            try
            {
                //The first 4 bytes we reserve for the lenght of the data 
                temp = new byte[4];
                //start receiving from connected server and when it will receive data it will run the ReceiveCallback function.
                acceptedSocket.BeginReceive(temp, 0, temp.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch { }
        }

        //receiving datas from client
        private static void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                // if bytes are less than 1 the client is disconnect from the server.
                if (acceptedSocket.EndReceive(AR) > 1)
                {
                    //Convert the first 4 bytes (int 32) that we received and convert it to an Int32.
                    temp = new byte[BitConverter.ToInt32(temp, 0)];

                    //Recieve the data
                    acceptedSocket.Receive(temp, temp.Length, SocketFlags.None);

                    // Convert the bytes to
                    string dataRecieve = Encoding.Default.GetString(temp);

                    //Differentiate the data according to the end tag
                    //If the client want to pause a save 
                    if (dataRecieve.IndexOf("<PAUSE>") > -1)
                    {
                        //Remove the end tag from the data recieve and launch pause methode
                        SaveViewModel.GetInstance().PauseSave(dataRecieve.Remove(dataRecieve.Length - 7, 7));
                    }
                    //If the client want to launch or resume a save
                    else if (dataRecieve.IndexOf("<PLAY>") > -1)
                    {
                        //Remove the end tag from the data recieve and launch play methode
                        SaveViewModel.GetInstance().PlaySave(dataRecieve.Remove(dataRecieve.Length - 6, 6));
                    }
                    //If the client want to stop a save
                    else if (dataRecieve.IndexOf("<STOP>") > -1)
                    {
                        //Remove the end tag from the data recieve and launch stop methode
                        SaveViewModel.GetInstance().StopSave(dataRecieve.Remove(dataRecieve.Length - 6, 6));
                    }
                    else
                    {
                        //If the Client send another data, just display the data
                        MessageBox.Show("Another Data recieve from client : " + dataRecieve);
                    }

                    //start all over again with waiting for a data to come from the socket.
                    StartReceiving();
                }
                else
                {
                    //If the data recieve is <1 bytes, disconnect
                    Disconnect();
                }
            }
            catch
            {
                //If there is an error in the reception
                if (acceptedSocket == null || !acceptedSocket.Connected)
                {
                    //check if the connection is always active, if it is not, disconnect
                    Disconnect();
                }
                else
                {
                    //Else, there has been a transmission error so we start listening again
                    StartReceiving();
                }
            }
        }

        //disconnect the socket
        private static void Disconnect()
        {
            //Disconnect only if a connection is active (to avoid error trying to disconnect a socket that is not connected)
            if (acceptedSocket != null && acceptedSocket.Connected)
            {
                // Close connection
                try
                {
                    //ShutDown socket (disable send and recieve on the socket)
                    acceptedSocket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    //Close the socket and release all the ressources
                    acceptedSocket.Close();
                    //Put the socket to null (to avoid a recieve infinite loop)
                    acceptedSocket = null;
                }
            }
        }

        // get the notification from the SaveViewModel
        public void Notify(Save save, string str_type)
        {
            string dataSend = StateViewModel.GetInstance().ConvertToString();
            Send(dataSend);

        }
    }
}

