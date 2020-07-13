using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lib_TcpServer;

namespace WinForm_TcpServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TcpServer mTcpServer;
        private void button1_Click(object sender, EventArgs e)
        {
            mTcpServer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mTcpServer = new TcpServer(60000);
            mTcpServer.DataReceive += new EventHandler<TCPEventArgs>(mTcpServer_DataReceive);
            mTcpServer.ClientConnected += new EventHandler<TCPEventArgs>(mTcpServer_ClientConnected);
            mTcpServer.ClientDisconnected += new EventHandler<TCPEventArgs>(mTcpServer_ClientDisconnected);
        }

        void mTcpServer_ClientDisconnected(object sender, TCPEventArgs e)
        {
            InvokeChangeLabelText(label1, e._handle.RemoteEndPoint.ToString()+"DisConnected");
            
        }

        void mTcpServer_ClientConnected(object sender, TCPEventArgs e)
        {
            InvokeChangeLabelText(label1, e._handle.RemoteEndPoint.ToString()+"Connected");
        }

        void mTcpServer_DataReceive(object sender, TCPEventArgs e)
        {
            InvokeAddItem(listBox1, e._msg);
        }

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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mTcpServer.Stop();
            mTcpServer.DataReceive -= new EventHandler<TCPEventArgs>(mTcpServer_DataReceive);
            mTcpServer.ClientConnected -= new EventHandler<TCPEventArgs>(mTcpServer_ClientConnected);
            mTcpServer.ClientDisconnected -= new EventHandler<TCPEventArgs>(mTcpServer_ClientDisconnected);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mTcpServer.SendMessage(textBox1.Text);
        }
    }
}
