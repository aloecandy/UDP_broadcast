using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Threading;

namespace ChatClient
{
    /// <summary>
    /// Client.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Client : UserControl
    {

        private Socket socket = null;
        private Thread waitMsg = null;
        private Thread waitMsgUDP = null;
        private Grid root;
        TcpClient myTCPclient = new TcpClient();
        string localIP = "";
        IPEndPoint iep;
        bool flag = true;
        public Client(Grid root)
        {
            
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            InitializeComponent();



            this.root = root;
            txtCont.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //Connect();
            btSend.Click += new RoutedEventHandler(btSend_Click);
            StartUDPserv();
        }
        void StartUDPserv()
        {
            //udpServ.Bind(udpEp);
            waitMsgUDP = new Thread(new ThreadStart(waitUDP));
            waitMsgUDP.Start();
        }
        void btSend_Click(object sender, RoutedEventArgs e)
        {
            //send 버튼 클릭
            SendMsg();
        }
        private void SendMsg()
        {
            try
            {
                if (chkATmode.IsChecked == true)
                {
                    socket.Send(Encoding.Default.GetBytes("at*ict*" + txtMsg.Text + "\r\n"));
                }
                else
                {
                    socket.Send(Encoding.Default.GetBytes(txtMsg.Text + "\r\n"));
                }
            }
            catch (Exception) { MessageBox.Show("메세지 센드에서 익셉션."); }
        }
        private void Connect(string ip, string port)
        {
            try
            {
                iep = new IPEndPoint(IPAddress.Parse(ip), Int32.Parse(port));
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(iep);
                //socket.Send(Encoding.Default.GetBytes("접속."));

                waitMsg = new Thread(new ThreadStart(wait));
                waitMsg.Start();
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
        }
        public void ScanStart()
        {
            string[] ipip= localIP.Split('.');
            ipip[3] = "255";


            Socket udpCli = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipip[0] +"."+ ipip[1] + "." + ipip[2] + "." + ipip[3]), 60000);
            byte[] temp = Encoding.UTF8.GetBytes("labco?");
            udpCli.SendTo(temp, endPoint);

        }
        delegate void MyDelegate();
        private void wait()
        {
            while (flag)
            {
                try
                {
                    byte[] data = new byte[1024];
                    string msg;
                    socket.Receive(data, data.Length, SocketFlags.None);
                    msg = Encoding.Default.GetString(data);
                    msg = msg.TrimEnd('\0');
                    //MessageBox.Show("받았다" + msg);
                    MyDelegate del = delegate ()
                    {
                        //MessageBox.Show("!");
                        txtCont.AppendText("\n" + msg);
                    };
                    root.Dispatcher.Invoke(DispatcherPriority.Normal, del);


                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }
        private void waitUDP()
        {
            while (true)
            {
                UdpClient listenner = new UdpClient(60001);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 60001);
                try
                {

                    byte[] data = listenner.Receive(ref groupEP);

                    string msg;
                    //udpServ.Receive(data, data.Length, SocketFlags.None);
                    msg = Encoding.UTF8.GetString(data);

                    //MessageBox.Show("받았다" + msg);
                    MyDelegate del = delegate ()
                    {
                        //MessageBox.Show("!");
                        txtScanned.AppendText(Encoding.ASCII.GetString(data, 0, data.Length) + " " + groupEP.ToString().Split(':')[0] + "\n");
                    };
                    root.Dispatcher.Invoke(DispatcherPriority.Normal, del);

                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                finally
                {
                    listenner.Close();
                }
            }
        }
        ~Client()
        {
            flag = false;
        }

        private void btConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect(ip.Text, port.Text);
            btConnect.IsEnabled = false;
            btDisconnect.IsEnabled = true;

        }

        private void txtCont_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btScan_Click(object sender, RoutedEventArgs e)
        {
            txtScanned.Text = "";
            ScanStart();
        }

        private void btDisconnect_Click(object sender, RoutedEventArgs e)
        {
            btConnect.IsEnabled = true;
            btDisconnect.IsEnabled = false;
            socket.Close();
        }

        private void txtMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.btSend_Click(sender, e);
                txtMsg.Text = "";
            }
        }
    }
}
