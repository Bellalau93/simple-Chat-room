using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace clienttest
{
    public partial class chats : Form
    {
        private string selfname;
        private string[] peernames;
        private UdpClient sendclient;
        private IPEndPoint[] peerendpoint;
        private int count;
        public chats()
        {
            InitializeComponent();
        }
        public void SetUserInfo(string selfName, string[] peerName, IPEndPoint[] peerIPEndPoint, int icount)
        {
            selfname= selfName;
            peernames = peerName;
            peerendpoint = peerIPEndPoint;
            count = icount;

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //启动发送线程
            sendclient = new UdpClient(0);
            Thread threadSend = new Thread(new ParameterizedThreadStart(SendMessageToGroup));
            threadSend.Start(string.Format("talk,{0},{1},{2}", DateTime.Now.ToLongDateString(), selfname, textBox2.Text));
            textBox1.AppendText(selfname + " " + DateTime.Now.ToLongDateString() + Environment.NewLine + textBox2.Text);
            textBox1.AppendText(Environment.NewLine);
            textBox2.Text = "";
            textBox2.Focus();
        }
        private void SendMessageToGroup(object obj)
        {
            string message = (string)obj;
            byte[] sendbytes = Encoding.Unicode.GetBytes(message);
            foreach(IPEndPoint ep in peerendpoint)
            {
                if (ep != null)
                {
                    sendclient.Send(sendbytes, sendbytes.Length, ep);
                }
            }
            sendclient.Close();
        }
        private delegate void showmessagedelegate(string peerName, string time, string content);



        public void showmessage(string peerName, string time, string content)
        {
            if (textBox1.InvokeRequired)
            {
                showmessagedelegate d = showmessage;
                textBox1.Invoke(d, peerName, time, content);
            }
            else
            {
                textBox1.AppendText(peerName + " " + time + Environment.NewLine + content);
                textBox1.AppendText(Environment.NewLine);
                textBox1.ScrollToCaret();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }
    }
}
