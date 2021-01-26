using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

namespace ChatServer
{
    
    // �������� ��������� ��� ������� StatusChanged
    public class StatusChangedEventArgs : EventArgs
    {
        // ��������, ������� ��� ����������, - ��� ���������, ����������� �������
        private string EventMsg;

        // �������� ��� ��������� � ��������� ��������� � �������
        public string EventMessage
        {
            get
            {
                return EventMsg;
            }
            set
            {
                EventMsg = value;
            }
        }

        // ����������� ��� ��������� ��������� � �������
        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }

    // ���� ������� ��������� ��� �������� ����������, ������� �� �������� ������ � ����� ��������
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);

    class ChatServer
    {
        public int Port { get; set; }
        // � ���� ���-������� �������� ������������ � ���������� (��������������� �������������)
        public static Hashtable htUsers = new Hashtable(30); // 30 ������������� �� ���� ����� �������
        // � ���� ���-������� �������� ���������� � ������������ (��������������� �� ����������)
        public static Hashtable htConnections = new Hashtable(30); // 30 ������������� �� ���� ����� �������
        // Will store the IP address passed to it
        private IPAddress ipAddress;
        private long IPv6;
        private TcpClient tcpClient;
        // ������� � ��� �������� ����� ���������� �����, ����� ������������ �����������, ����������, �������� ��������� � �. �.
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        //����������� ������������� IP-����� �� ���, ������� ��� ������� ��������� ��������� ��������
        public ChatServer(long address,int port)
        {

            IPv6 = address;
            Port = port;
        }

        // �����, ������� ����� ��������� �������������� ����������
        private Thread thrListener;

        // ������ TCP, ������� ������������ ����������
        private TcpListener tlsClient;

        // ������ ����� while, ����� �� ��������� ������� �� �������������
        bool ServRunning = false;

        // ��������� ������������ � ���-�������
        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            // ������� ��������� ��� ������������ � ��������� � ��� ���������� � ��� ��� �������
            ChatServer.htUsers.Add(strUsername, tcpUser);
            ChatServer.htConnections.Add(tcpUser, strUsername);

            // �����  � ����� ���������� �� ����� ������� �������������� � � ������ �������
            SendAdminMessage(htConnections[tcpUser] + " ������������� � ���");
        }

        // �������� ������������ �� ���-������
        public static void RemoveUser(TcpClient tcpUser)
        {
            // ���� ������������ ��������� � ���-�������
            if (htConnections[tcpUser] != null)
            {
                // ������� ���������� ���������� � ������������ ������ ������������� �� ����������
                SendAdminMessage(htConnections[tcpUser] + " ������� ���");

                // ������� ������������ �� ���-�������
                ChatServer.htUsers.Remove(ChatServer.htConnections[tcpUser]);
                ChatServer.htConnections.Remove(tcpUser);
            }
        }

        // ��� ����������, ����� �� ����� ������� ������� StatusChanged
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;
            if (statusHandler != null)
            {
                // �������� ���  �������
                statusHandler(null, e);
            }
        }

        // �������� ���������������� ���������
        public static void SendAdminMessage(string Message)
        {
            StreamWriter swSenderSender;

            // ������ �����, �������� � ����� ����������, ��� ��� �������
            e = new StatusChangedEventArgs("Administrator: " + Message);
            OnStatusChanged(e);

            // ������� ������ TCP-��������, ������ �������� ������������� ���������� ��������� � ��� �������������
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            // ��������� ������� TcpClient � ������
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            // ����������� �������� ������ TCP-��������
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // ��������� ��������� ��������� ������� �� ���
                try
                {
                    // ���� ��������� ������ ��� ���������� �������, ����������
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // ���������� ��������� �������� ������������ � �����
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // ���� �������� ��������, �� ������������ ��� ������ ���, ������� ���
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        // ���������� ��������� �� ������ ������������ ���� ���������
        public static void SendMessage(string From, string Message)
        {
            StreamWriter swSenderSender;

            // ������ �����, ������� � ����� ����������, ��� ��� �������
            e = new StatusChangedEventArgs(From + " �������: " + Message);
            OnStatusChanged(e);

            // ������� ������ TCP-��������, ������ �������� ������������� ���������� ��������� � ��� �������������
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            //��������� ������� TcpClient � ������
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            // ����������� �������� ������ TCP-��������
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // ���������� ��������� ��������� ������� �� ���
                try
                {
                    // ���� ��������� ������ ��� ���������� �������, ������ ����������

                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // ���� ��������� ������ ��� ���������� �������, �����������
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(From + " �������: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // ���� �������� ��������, �� ������������ ��� ������ ���, ������� ���
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public void StartListening()
        {

            // �������� IP-����� ������� �������� ����������, ������ ��� ����� ��������� ���������� � ��������� �������������
           // IPAddress ipaLocal = (IPAddress)IPv6;
            
            // ������� ������ �������������� TCP, ��������� IP-����� ������� � ��������� ����
            tlsClient = new TcpListener(IPAddress.IPv6Any,Port);

            // �������� �������������� TCP � ����������� ����������
            tlsClient.Start();

            // ���� while ����� ��������� �������� true � ���� ������ ����� ��������� ����������
            ServRunning = true;

            // ��������� ����� ���������, �� ������� ����������� ���������
            thrListener = new Thread(KeepListening);
            thrListener.Start();
        }

        private void KeepListening()
        {
            // ���� ������ ��������
            while (ServRunning == true)
            {
                // ��������� ���������� ����������
                tcpClient = tlsClient.AcceptTcpClient();
                // ������� ����� ��������� ����������
                Connection newConnection = new Connection(tcpClient);
            }
        }
    }

    // ���� ����� handels connections; ��� ����������� ����� ������� ��, ������� ������������ �������������
    class Connection
    {
        TcpClient tcpClient;
        // �����, ������� ����� ���������� ���������� �������
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;
        private string currUser;
        private string strResponse;

        // ����������� ������ ��������� � ���� TCP ����������
        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            // �����, ������� ��������� ������� � ������� ���������
            thrSender = new Thread(AcceptClient);
            // ����� �������� ����� AcceptClient() 
            thrSender.Start();
        }

        private void CloseConnection()
        {
            // ��������� ������� �������� �������
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        // ����������, ����� ����������� ����� ������
        private void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            // ������ ���������� � ����� �� �������
            currUser = srReceiver.ReadLine();

            // �� �������� ����� �� �������
            if (currUser != "")
            {
                // ��������� ��� ������������ � ���-�������
                if (ChatServer.htUsers.Contains(currUser) == true)
                {
                    // 0 �������� �� ���������
                    swSender.WriteLine("0|��� ��� ������������ ��� ����������.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else if (currUser == "Administrator")
                {
                    // 0 �������� �� ���������
                    swSender.WriteLine("0|��� ��� ������������ ���������������.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else
                {
                    // 1 �������� �������� �����������
                    swSender.WriteLine("1");
                    swSender.Flush();

                    // �������� ������������ � ���-������� � ������� ������������ ��������� �� ����
                    ChatServer.AddUser(tcpClient, currUser);
                }
            }
            else
            {
                CloseConnection();
                return;
            }

            try
            {
                // ���������� ����� ��������� �� ������������
                while ((strResponse = srReceiver.ReadLine()) != "")
                {
                    // ���� �� ��������������, ������� ������������
                    if (strResponse == null)
                    {
                        ChatServer.RemoveUser(tcpClient);
                    }
                    else
                    {
                        // � ��������� ������ ���������� ��������� ���� ��������� �������������
                        ChatServer.SendMessage(currUser, strResponse);
                    }
                }
            }
            catch
            {
                // ���� ���-�� ����� �� ��� � ���� �������������, ��������� ���
                ChatServer.RemoveUser(tcpClient);
            }
        }
    }
}
