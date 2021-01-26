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
        // Будет содержать имя пользователя
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        //Необходимо обновить форму сообщениями из другого потока
        private delegate void UpdateLogCallback(string strMessage);
        // Необходимо установить форму в "отключенное" состояние от другого потока
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;

        public Form1()
        {
            // При выходе из приложения не забудьте сначала отключить его
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        // Обработчик событий для выхода из приложения
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Закрывает соединения, потоки и т. д.
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Если мы в данный момент не подключены но ждем соединения
            if (Connected == false)
            {
                // Инициализировать подключение
                InitializeConnection();
            }
            else //Мы соединены, таким образом разъединяемся
            {
                CloseConnection("Disconnected at user's request.");
            }
        }
        public int Port { get; set; }
        private void InitializeConnection()
        {
            // Проанализируйте IP-адрес из текстового поля в объект IPAddress
            ipAddr = IPAddress.Parse(txtIp.Text);

            long IPv6 = ipAddr.ScopeId;

            Port = Convert.ToInt32(textBox1.Text);
            // Запуск нового TCP соединения с сервером чата
            tcpServer = new TcpClient(AddressFamily.InterNetworkV6);
            tcpServer.Connect(ipAddr, Port);

            // Помогает нам отслеживать, связаны мы или нет
            Connected = true;
            // Подготовьте форму
            UserName = txtUser.Text;

            // Отключите и включите соответствующие поля
            txtIp.Enabled = false;
            txtUser.Enabled = false;
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            btnConnect.Text = "Disconnect";

            // Отправьте нужное имя пользователя на сервер
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUser.Text);
            swSender.Flush();

            // Запустите поток для получения сообщений и дальнейшего общения
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            //Получите ответ от сервера
            srReceiver = new StreamReader(tcpServer.GetStream());
            //Если первый символ ответа равен 1, то соединение прошло успешно
            string ConResponse = srReceiver.ReadLine();
            // Если первый символ равен 1, то соединение прошло успешно
            if (ConResponse[0] == '1')
            {
                // Обновите форму, чтобы сообщить ему, что теперь мы подключены
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            else // Если первый символ не является 1 (вероятно, 0), соединение было неудачным
            {
                string Reason = "Not Connected: ";
                // Извлеките причину из ответного сообщения. Причина начинается с 3-го символа
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                // Обновите форму с указанием причины, по которой мы не смогли подключиться
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                // Exit the method
                return;
            }
            // Пока мы успешно подключены, считывайте входящие строки с сервера
            while (Connected)
            {
                // Show the messages in the log TextBox
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
            }
        }

        // Этот метод вызывается из другого потока для обновления журнала TextBox
        private void UpdateLog(string strMessage)
        {
            // Добавление текста также прокручивает текстовое поле вниз каждый раз
            txtLog.AppendText(strMessage + "\r\n");
        }

        // Closes a current connection
        private void CloseConnection(string Reason)
        {
            // Покажите причину, по которой соединение заканчивается
            txtLog.AppendText(Reason + "\r\n");
            // Включение и отключение соответствующих элементов управления в форме
            txtIp.Enabled = true;
            txtUser.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Connect";

            // Закрываем обьекты
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        // Отправляет набранное сообщение на сервер
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

        // Мы хотим отправить сообщение при нажатии кнопки Отправить
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // Но мы также хотим отправить сообщение после нажатия клавиши Enter
        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Если ключ находится в поле Enter
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