using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.ObjectModel;
using System.Timers;

namespace Lib_TcpServer
{
    public class TcpServer
    {
        //private TcpListener tcpListener;
        private Socket s, worker;
        /// <summary>
        /// 客户端会话列表
        /// </summary>
        //private static List<TcpClient> _clients = new List<TcpClient>();
        //private List<TcpClient> _last_clients;
        private static List<Socket> list = new List<Socket>();
        byte[] buffer = new byte[1024];

        private void clearBuffer()
        {
            for (int i = 0; i < this.buffer.Length; i++)
            {
                this.buffer[i] = (byte)0;
            }
        }

        private byte[] trimBuffer(byte[] byteArray)
        {
            // http://stackoverflow.com/a/240745
            int index = byteArray.Length - 1;
            while (byteArray[index] == 0) --index;

            if (-1 == index) throw new Exception("trim error, maybe caused by client close");

            byte[] finalArray = new byte[index + 1];
            Array.Copy(byteArray, finalArray, index + 1);
            return finalArray;
        }

        private void printBuffer(byte[] bf)
        {
            string sAscii = string.Empty;
            foreach (var b in bf)
            {
                sAscii += " " + b.ToString();
            }
            string mm = "  ASCII码序列： [ " + sAscii.TrimStart() + "]";
        }

        #region 构造器
        /// <summary>
        /// 同步TCP服务器
        /// </summary>
        /// <param name="listenPort">监听的端口</param>
        public TcpServer(int listenPort)
            : this(IPAddress.Any, listenPort)
        {
        }

        /// <summary>
        /// 同步TCP服务器
        /// </summary>
        /// <param name="localEP">监听的终结点</param>
        public TcpServer(IPEndPoint localEP)
            : this(localEP.Address, localEP.Port)
        {
        }

        public TcpServer(string ip, int listenPort = 502)
            : this(IPAddress.Parse(ip), listenPort)
        {
        }

        /// <summary>
        /// ModubusTCP服务器
        /// </summary>
        /// <param name="localIPAddress">监听的IP地址</param>
        /// <param name="listenPort">监听的端口</param>
        public TcpServer(IPAddress localIPAddress, int listenPort)
        {
            this.Address = localIPAddress;
            this.Port = listenPort;

            list = new List<Socket>(); 
            //tcpListener = new TcpListener(new IPEndPoint(this.Address, this.Port));
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建一个socket对像，如果用udp协议，则要用SocketType.Dgram类型的套接字
            s.Bind(new IPEndPoint(this.Address, this.Port));
        }
        #endregion

        #region Properties
        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// 监听的IP地址
        /// </summary>
        public IPAddress Address { get; private set; }
        /// <summary>
        /// 监听的端口
        /// </summary>
        public int Port { get; private set; }
        #endregion

        #region Method
        /// <summary>
        /// 启动服务器
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                s.Listen(10);
                //tcpListener.Start();                

                IsRunning = true;

                Thread thread = new Thread(Accept);
                thread.IsBackground = true;
                thread.Start();

                System.Timers.Timer timer = new System.Timers.Timer(1000);
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Start();
            }
        }

        //每秒服务端向客户端推送
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (list.Count > 0)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {

                    string sendStr = "Server Information";
                    byte[] bs = Encoding.ASCII.GetBytes(sendStr);
                    if (list[i].Poll(1000, SelectMode.SelectRead)) //SelectMode.SelectRead表示，如果已调用 并且有挂起的连接，true。或 - 如果有数据可供读取，则为 true。- 或 - 如果连接已关闭、重置或终止，则返回 true（此种情况就表示若客户端断开连接了，则此方法就返回true）； 否则，返回 false。
                    {
                        //有客户端断开连接                        
                        TCPEventArgs m = new TCPEventArgs(list[i]);
                        RaiseClientDisconnected(m);
                        list[i].Close();//关闭socket
                        list.RemoveAt(i);//从列表中删除断开的socke
                        continue;

                    }

                    list[i].Send(bs, bs.Length, 0);

                }
            }
        }

        /// <summary>
        /// 开始进行监听客户端连接断开情况
        /// </summary>
        private void Accept()
        {
            try
            {
                worker = s.Accept();
                list.Add(worker);
                //有新的客户端连接
                TCPEventArgs m = new TCPEventArgs(worker);
                RaiseClientConnected(m);
            }
            catch (Exception)
            {

            }
            while (IsRunning)
            {
                clearBuffer();
                try
                {
                    worker.Receive(buffer);
                }
                catch (SocketException ex)
                {
                    //ex.Message
                    break;
                }
                byte[] trimedMsg1;
                try
                {
                    trimedMsg1 = trimBuffer(buffer);//todo
                    TCPEventArgs m = new TCPEventArgs(System.Text.Encoding.Default.GetString(trimedMsg1));
                    RaiseDataReceive(m);
                }
                catch (IndexOutOfRangeException)
                {
                    //say("可能是客户端关闭了连接。");
                    break;
                }
            }
            Accept();//run again
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void Stop()
        {
            if (IsRunning)
            {
                try
                {
                    IsRunning = false;

                    for (int i = 0; i < list.Count; i++)
                    {
                        TCPEventArgs m = new TCPEventArgs(list[i]);
                        RaiseClientDisconnected(m);
                    }
                    list.Clear();
                    
                    s.Close();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="mStr"></param>
        public void SendMessage(string mStr)
        {
            worker.Send(System.Text.Encoding.Default.GetBytes(mStr));
        }
        #endregion

        #region Event  
        /// <summary>
        /// 与客户端的连接已建立事件
        /// </summary>
        public event EventHandler<TCPEventArgs> ClientConnected;
        private void RaiseClientConnected(TCPEventArgs e)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, e);
            }
        }

        /// <summary>
        /// 与客户端的连接已断开事件
        /// </summary>
        public event EventHandler<TCPEventArgs> ClientDisconnected;
        private void RaiseClientDisconnected(TCPEventArgs e)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, e);
            }
        }

        /// <summary>
        /// 接受到数据
        /// </summary>
        public event EventHandler<TCPEventArgs> DataReceive;
        private void RaiseDataReceive(TCPEventArgs e)
        {
            if (DataReceive != null)
            {
                DataReceive(this, e);
            }
        }
        #endregion

    }
}
