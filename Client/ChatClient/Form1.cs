using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        // ����� ��������� ��� ������������
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        //���������� �������� ����� ����������� �� ������� ������
        private delegate void UpdateLogCallback(string strMessage);
        // ���������� ���������� ����� � "�����������" ��������� �� ������� ������
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;

        public Form1()
        {
            // ��� ������ �� ���������� �� �������� ������� ��������� ���
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        // ���������� ������� ��� ������ �� ����������
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // ��������� ����������, ������ � �. �.
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // ���� �� � ������ ������ �� ���������� �� ���� ����������
            if (Connected == false)
            {
                // ���������������� �����������
                InitializeConnection();
            }
            else //�� ���������, ����� ������� �������������
            {
                CloseConnection("Disconnected at user's request.");
            }
        }
        public int Port { get; set; }
        private void InitializeConnection()
        {
            // ��������������� IP-����� �� ���������� ���� � ������ IPAddress
            ipAddr = IPAddress.Parse(txtIp.Text);

            long IPv6 = ipAddr.ScopeId;

            Port = Convert.ToInt32(textBox1.Text);
            // ������ ������ TCP ���������� � �������� ����
            tcpServer = new TcpClient(AddressFamily.InterNetworkV6);
            tcpServer.Connect(ipAddr, Port);

            // �������� ��� �����������, ������� �� ��� ���
            Connected = true;
            // ����������� �����
            UserName = txtUser.Text;

            // ��������� � �������� ��������������� ����
            txtIp.Enabled = false;
            txtUser.Enabled = false;
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            btnConnect.Text = "Disconnect";

            // ��������� ������ ��� ������������ �� ������
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUser.Text);
            swSender.Flush();

            // ��������� ����� ��� ��������� ��������� � ����������� �������
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            //�������� ����� �� �������
            srReceiver = new StreamReader(tcpServer.GetStream());
            //���� ������ ������ ������ ����� 1, �� ���������� ������ �������
            string ConResponse = srReceiver.ReadLine();
            // ���� ������ ������ ����� 1, �� ���������� ������ �������
            if (ConResponse[0] == '1')
            {
                // �������� �����, ����� �������� ���, ��� ������ �� ����������
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            else // ���� ������ ������ �� �������� 1 (��������, 0), ���������� ���� ���������
            {
                string Reason = "Not Connected: ";
                // ��������� ������� �� ��������� ���������. ������� ���������� � 3-�� �������
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                // �������� ����� � ��������� �������, �� ������� �� �� ������ ������������
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                // Exit the method
                return;
            }
            // ���� �� ������� ����������, ���������� �������� ������ � �������
            while (Connected)
            {
                // Show the messages in the log TextBox
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
            }
        }

        // ���� ����� ���������� �� ������� ������ ��� ���������� ������� TextBox
        private void UpdateLog(string strMessage)
        {
            // ���������� ������ ����� ������������ ��������� ���� ���� ������ ���
            txtLog.AppendText(strMessage + "\r\n");
        }

        // Closes a current connection
        private void CloseConnection(string Reason)
        {
            // �������� �������, �� ������� ���������� �������������
            txtLog.AppendText(Reason + "\r\n");
            // ��������� � ���������� ��������������� ��������� ���������� � �����
            txtIp.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Connect";

            // ��������� �������
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        // ���������� ��������� ��������� �� ������
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                swSender.WriteLine(txtMessage.Text);
                swSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        // �� ����� ��������� ��������� ��� ������� ������ ���������
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // �� �� ����� ����� ��������� ��������� ����� ������� ������� Enter
        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // ���� ���� ��������� � ���� Enter
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtIp_TextChanged(object sender, EventArgs e)
        {

        }
    }
}