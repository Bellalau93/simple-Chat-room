using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace clienttest
{
    public partial class chat : Form
    {
        private string selfname;
        private string peername;
        private IPEndPoint peerendpoint;
        private UdpClient sendclient;
        public chat()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        public void SetUserInfo(string selfName, string peerName, IPEndPoint peeripEndPoint)
        {
            selfname = selfName;
            peername = peerName;
            peerendpoint = peeripEndPoint;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sendclient = new UdpClient(0);
            Thread sendthread = new Thread(sendmessage);
            sendthread.Start(string.Format("talk,{0},{1},{2}", DateTime.Now.ToLongDateString(), selfname, textBox2.Text));
            textBox1.AppendText(selfname + " " + DateTime.Now.ToLongDateString() + Environment.NewLine + textBox2.Text);
            textBox1.AppendText(Environment.NewLine);
            textBox2.Text = "";
            textBox2.Focus();
        }
        private void sendmessage(object obj)
        {
            string message = (string)obj;
            byte[] sendbytes = Encoding.Unicode.GetBytes(message);
            sendclient.Send(sendbytes, sendbytes.Length, peerendpoint);
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


        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
