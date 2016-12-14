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
        private Grid root;
        bool flag = true;
        public Client(Grid root)
        {
            InitializeComponent();
            this.root = root;
            Connect();
            btSend.Click += new RoutedEventHandler(btSend_Click);
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
                socket.Send(Encoding.Default.GetBytes(txtMsg.Text));
            }
            catch (Exception) { MessageBox.Show("메세지 센드에서 익셉션."); }
        }
        private void Connect()
        {
            try
            {
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse("210.107.236.52"), 3078);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(iep);
                socket.Send(Encoding.Default.GetBytes("접속."));

                waitMsg = new Thread(new ThreadStart(wait));
                waitMsg.Start();
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
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
                    MyDelegate del = delegate()
                    {
                        //MessageBox.Show("!");
                        txtCont.Text += ("\n" + msg);
                    };
                    root.Dispatcher.Invoke(DispatcherPriority.Normal, del);

                    
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }
        ~Client()
        {
            flag = false;
        }
    }
}
