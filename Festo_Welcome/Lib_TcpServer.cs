using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Festo_Welcome
{
    class C_TcpServer
    {
        private Form1 mainForm = null;
        public int Port;
        Socket server, worker;

        public C_TcpServer(Form1 form, int port)
        {
            this.mainForm = form;
            this.Port = port;
        }

        public void Run()
        {
            IPEndPoint ipEp = new IPEndPoint(IPAddress.Any, Port);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                server.Bind(ipEp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bind error, {0}", ex.Message);
                return;
            }
            server.Listen(1);
            say("监听在 " + Port.ToString() + " 端口……");

            ThreadStart start = delegate
            {
                _run();
            };
            Thread tStart = new Thread(start);
            tStart.IsBackground = true;
            tStart.Start();
        }

        byte[] buffer = new byte[256];
        private void _run()
        {
            worker = server.Accept();
            say(string.Empty);
            say("接受了一个外部连接 " + worker.RemoteEndPoint.ToString());
            #region recv-send loop
            while (true)
            {
                // msg#1, i.e. 101\n
                clearBuffer();
                try
                {
                    worker.Receive(buffer);
                }
                catch (SocketException ex)
                {
                    say(ex.Message);
                    break;
                }
                byte[] trimedMsg1;
                try
                {
                    trimedMsg1 = trimBuffer(buffer);//todo
                }
                catch (IndexOutOfRangeException)
                {
                    //say(ex.Message + "ops...restart\n");
                    say("可能是客户端关闭了连接。");
                    break;
                }
                say("PLC 发送的 msg：");
                printBuffer(trimedMsg1);
                if (trimedMsg1[trimedMsg1.Length - 1] != 10) // \n = 10
                {
                    say("已响应");
                    //worker.Send(buildErrorResponse());
                }
                            
            }
            worker.Shutdown(SocketShutdown.Both);
            #endregion
            _run(); // run again
        }

        public bool sendSOS()
        {
            try
            {
                sendMsg("SOS");
                say("请求已发送");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool sendMsg(string mStr)
        {
            try
            {
                if (worker!=null)
                {
                    worker.Send(System.Text.Encoding.Default.GetBytes(mStr));
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

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
            say("  ASCII码序列： [ " + sAscii.TrimStart() + "]");
        }

        public Action<string> Say = Console.WriteLine;

        private void say(string msg)
        {
            Say(msg);
        }
    }
}
