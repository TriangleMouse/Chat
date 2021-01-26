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
            // Считываем IP-адреса сервера из  TextBox
            IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
            port= Convert.ToInt32(textBox1.Text);
            long IPv6 = ipAddr.ScopeId;
            //Создаем новый экземпляр объекта ChatServer
            ChatServer mainServer = new ChatServer(IPv6,port);
            // Подключаем обработчик событий StatusChanged к mainServer_StatusChanged
            ChatServer.StatusChanged += new StatusChangedEventHandler(mainServer_StatusChanged);
            //Начинаем  прослушивать соединения
            mainServer.StartListening();
          
            txtLog.AppendText("Мониторинг подключений...\r\n");
        }

        public void mainServer_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Вызываем метод, который обновляет форму
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // Обновляем журнал с сообщением
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