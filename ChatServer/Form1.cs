using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private delegate void UpdateStatusCallback(string strMessage);
        public int port { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            // ��������� IP-������ ������� ��  TextBox
            IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
            port= Convert.ToInt32(textBox1.Text);
            long IPv6 = ipAddr.ScopeId;
            //������� ����� ��������� ������� ChatServer
            ChatServer mainServer = new ChatServer(IPv6,port);
            // ���������� ���������� ������� StatusChanged � mainServer_StatusChanged
            ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
            //��������  ������������ ����������
            mainServer.StartListening();
          
            txtLog.AppendText("���������� �����������...\r\n");
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // �������� �����, ������� ��������� �����
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // ��������� ������ � ����������
            txtLog.AppendText(strMessage + "\r\n");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtIp_TextChanged(object sender, EventArgs e)
        {

        }
    }
}