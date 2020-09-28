using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Festo_Welcome
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        C_TcpServer mTcpServer;
        

        private void label1_Click(object sender, EventArgs e)
        {
            //if (mTcpServer.IsRunning)
            //{
            //    try
            //    {
            //        mTcpServer.SendMessage("SOS");
            //        InvokeChangeLabelText(lbl_Status, "sent");
            //    }
            //    catch (Exception)
            //    {

            //    }                
            //}
            
        }       

        private void Form1_Load(object sender, EventArgs e)
        {
            mTcpServer = new C_TcpServer(this, 60000);
            mTcpServer.Say = delegate(string msg) { this.updateGUI(msg); };
            mTcpServer.Run();
        }

        private void enableControl()
        {
            var task = Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(200);
                }
            });
            if (task.Wait(60000) == false)
            {
                linkLabel1.Enabled = true;
            }
        }

        void updateGUI(string content)
        {
            lbl_Status.Text = content;
            if (content=="已响应")
            {
                if (MessageBox.Show("已处理", "Message", MessageBoxButtons.OK,MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == DialogResult.OK)
                {
                    linkLabel1.Enabled = true;
                    lbl_Status.Text = "已确认";
                }
                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
       
        #region 委托
        protected delegate void ChangeLabelHandler(Label LabelCtrl, string Txt);
        void InvokeChangeLabelText(Label LabelCtrl, string Txt)
        {
            LabelCtrl.Invoke((ChangeLabelHandler)ChangeLabelText, LabelCtrl, Txt);
        }
        void ChangeLabelText(Label LabelCtrl, string Txt)
        {
            LabelCtrl.Text = Txt;
        }

        protected delegate void ChangeTextboxHandler(TextBox TextBoxCtrl, string Txt);
        void InvokeChangeTextBoxText(TextBox TextBoxCtrl, string Txt)
        {
            TextBoxCtrl.Invoke((ChangeTextboxHandler)ChangeTextBoxText, TextBoxCtrl, Txt);
        }
        void ChangeTextBoxText(TextBox TextBoxCtrl, string Txt)
        {
            TextBoxCtrl.Text = Txt;
        }

        protected delegate void AppendTextHandler(TextBox textBoxCtrl, string Txt);
        void InvokeAppendText(TextBox textBoxCtrl, string Txt)
        {
            textBoxCtrl.Invoke((AppendTextHandler)AppendText, textBoxCtrl, Txt);
        }
        void AppendText(TextBox textBoxCtrl, string Txt)
        {
            textBoxCtrl.AppendText(Txt);
        }

        protected delegate void DeleteItemHandler(ListBox listBoxCtrl, string Txt);
        void InvokeDeleteItem(ListBox listBoxCtrl, string Txt)
        {
            listBoxCtrl.Invoke((DeleteItemHandler)DeleteItem, listBoxCtrl, Txt);
        }
        void DeleteItem(ListBox listBoxCtrl, string Txt)
        {
            for (int i = 0; i < listBoxCtrl.Items.Count; i++)
            {
                if (listBoxCtrl.Items[i].ToString() == Txt)
                {
                    listBoxCtrl.Items.RemoveAt(i);
                }
            }
        }

        protected delegate void AddItemHandler(ListBox listBoxCtrl, string Txt);
        void InvokeAddItem(ListBox listBoxCtrl, string Txt)
        {
            listBoxCtrl.Invoke((AddItemHandler)AddItem, listBoxCtrl, Txt);
        }
        void AddItem(ListBox listBoxCtrl, string Txt)
        {
            listBoxCtrl.Items.Add(Txt);
        } 
        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (mTcpServer.sendSOS())
            {
                linkLabel1.Enabled = false;
                //Thread t = new Thread(() =>
                //    {
                //        Thread.Sleep(10000000);
                //    });
                //t.Start();
                //bool isOver = t.Join(60000);
                //if (!isOver)
                //{                   
                //    linkLabel1.Enabled = true;
                //    t.Abort();
                //}
                ThreadStart start = delegate
                {
                    enableControl();
                };
                Thread tStart = new Thread(start);
                tStart.IsBackground = true;
                tStart.Start();
            }
        }
        class Timeout
        {
            //属性
            // 设定超时间隔为1000ms
            private readonly int TimeoutInterval = 1000;
            // lastTicks 用于存储新建操作开始时的时间
            public long lastTicks;
            // 用于存储操作消耗的时间
            public long elapsedTicks;

            //构造函数
            public Timeout(int Interval)
            {
                TimeoutInterval = Interval;
                lastTicks = DateTime.Now.Ticks;
            }
            public bool IsTimeout()
            {
                elapsedTicks = DateTime.Now.Ticks - lastTicks;
                TimeSpan span = new TimeSpan(elapsedTicks);
                double diff = span.TotalSeconds;
                if (diff > TimeoutInterval)
                    return true;
                else
                    return false;

            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            double n_Width = (double)this.Width / (double)617;
            double n_Height = (double)this.Height / (double)909;
            linkLabel1.Location = new Point((int)(373 * n_Width), (int)(722 * n_Height));
            FontFamily mFontFamily = linkLabel1.Font.FontFamily;
            float mSize = 15;
            float mSize_new = mSize * (float)(n_Width > n_Height ? n_Height : n_Width);
            linkLabel1.Font = new System.Drawing.Font(mFontFamily, mSize_new);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {

        }
    }
}
