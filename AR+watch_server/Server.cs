using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AR_watch_server
{

    public class Server : INotifyPropertyChanged
    {
        const int PORT = 8398;
        public TcpListener listener;
        public Thread threadListen;
        public bool listening = false;

        public List<TcpClient> clientList;

        MainWindow father;
        public List<string> ips;
        public string netInfo0;
        public string console0;
        public string data0;
        public string netInfo { get { return netInfo0; } set { netInfo0 = value; OnPropertyChanged("netInfo"); } }
        public string console { get { return console0; } set { console0 = value; OnPropertyChanged("console"); } }
        public string data { get { return data0; } set { data0 = value; OnPropertyChanged("data"); } }

        public delegate void InformHandle(object sender, string info);
        public event InformHandle Recv;

        public Server(MainWindow father)
        {
            this.father = father;
            string hostName = Dns.GetHostName();
            IPAddress[] addressList = Dns.GetHostAddresses(hostName);
            ips = new List<string>();
            foreach (IPAddress ip in addressList)
            {
                if (ip.ToString().Split('.').Length != 4) continue;
                ips.Add(ip.ToString());
            }
            clientList = new List<TcpClient>();
            netInfo = "Idle";
        }

        public void Listen()
        {
            if (listener == null)
            {
                try
                {
                    string ip = father.uComboIP.SelectedItem.ToString();
                    listener = new TcpListener(IPAddress.Parse(ip), PORT);
                    threadListen = new Thread(Listening);
                    threadListen.IsBackground = true;
                    threadListen.Start();
                }
                catch (Exception e)
                {
                    console += e.ToString() + "\n";
                }
            }
        }

        public void Stop()
        {
            listening = false;
            clientList.Clear();
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
            console += "Stop listening\n";
        }

        public void Listening()
        {
            listener.Start();
            listening = true;
            netInfo = "Listening...";
            console += "Listening...\n";
            try
            {
                while (listening)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    clientList.Add(client);
                    console += "Connection(" + clientList.Count + "): " + client.Client.RemoteEndPoint.ToString() + "\n";
                    Thread threadReceive = new Thread(Receiving);
                    threadReceive.IsBackground = true;
                    threadReceive.Start(client);
                }
            }
            catch (Exception e)
            {
                console += e.ToString() + "\n";
            }
            finally
            {
                netInfo = "Idle";
            }
        }

        public void Receiving(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;
            string remoteInfo = client.Client.RemoteEndPoint.ToString();
            try
            {
                StreamReader reader = new StreamReader(client.GetStream());
                while (listening)
                {
                    string info = reader.ReadLine();
                    if (info == null) break;
                    data = info;
                    Recv(this, info);
                }
                reader.Close();
            }
            catch (Exception e)
            {
                console += e.ToString() + "\n";
            }
            clientList.Remove(client);
            console += "Disconnection(" + clientList.Count + "): " + remoteInfo + "\n";
        }

        public void BroadCast(string s)
        {
            foreach (TcpClient client in clientList)
            {
                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.WriteLine(s);
                writer.Flush();
                console += "Send to " + client.Client.RemoteEndPoint.ToString() + " : " + s + "\n";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
