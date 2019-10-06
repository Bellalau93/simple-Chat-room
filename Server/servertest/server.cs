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
using System.Data.SqlClient;
using System.Threading;

namespace sockettest
{
    public partial class server : Form
    {
        private List<user> userlist = new List<user>();//save all the users being listened
        public int mport = 8888;//server port 
        private UdpClient sendclient;//anonymous send socket
        private UdpClient receiveclient;//receive socket 
        private IPEndPoint ipendpoint;//server address
        private TcpListener myTcplistener;//server listening socket
        private IPAddress ipaddress;//server ip
        private NetworkStream netstream;
        private BinaryWriter bw;
        String userListString;
        String username;
        public int rand1, rand2, tcport;//listening port
  
     
        public server()
        {
            InitializeComponent();
            String ServerIp = getLocalIP();
            IPAddress ip = IPAddress.Parse(ServerIp);
            textBox1.Text = ServerIp;
            textBox2.Text = mport.ToString();
            button2.Enabled = false;

        }
        //获取信息
        static public void refresh()
        {


        }
        public static string getLocalIP()
        {
            try
            {
                string HOSTname = Dns.GetHostName();
                IPHostEntry entry = Dns.GetHostEntry(HOSTname);
                for (int i = 0; i < entry.AddressList.Length; i++)
                {
                    if (entry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return entry.AddressList[i].ToString();
                    }
                }
                return "";

            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机ip地址错误");
                return "";
            }
        }
        private void SendtoClient(user auser, string message)
        {
            //sending 
            sendclient = new UdpClient(0);
            byte[] sendbytes = Encoding.Unicode.GetBytes(message);
            IPEndPoint remoteIPEndPoint = auser.GetIPEndPoint();
            sendclient.Send(sendbytes, sendbytes.Length, remoteIPEndPoint);
            sendclient.Close();
        }
        private void ReceiveMessage()
        {
            IPEndPoint remoteendpoit = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    //exception when close receiveclient
                    byte[] receiveBytes = receiveclient.Receive(ref remoteendpoit);
                    string message = Encoding.Unicode.GetString(receiveBytes, 0, receiveBytes.Length);

                    //display the content of message
                    AddItem(string.Format("{0},[{1}]", remoteendpoit, message));

                    //handle message
                    string[] splitmessage = message.Split(',');

                    //analyse server endpoint
                    string[] splitaddress = splitmessage[2].Split(':');// delete :
                    IPEndPoint clientIPendpoint = new IPEndPoint(IPAddress.Parse(splitaddress[0]), int.Parse(splitaddress[1]));
                   
                    switch (splitmessage[0])
                    {
                        //receive "login"
                        case "login":
                            user auser = new user(splitmessage[1], clientIPendpoint);
                            userlist.Add(auser);
                            username = auser.GetName();
                            AddItem(string.Format("用户{0}({1})加入", auser.GetName(), auser.GetIPEndPoint()));
                            string sendString = "Accept," + tcport.ToString();
                            SendtoClient(auser, sendString);   //向该用户发送同意关键字
                            AddItem(string.Format("向{0}({1})发出:[{2}]", auser.GetName(), auser.GetIPEndPoint(), sendString));

                            for (int i = 0; i < userlist.Count; i++)
                            {
                                if (userlist[i].GetName() != auser.GetName())
                                {
                                    //send to others
                                    SendtoClient(userlist[i], message);

                                }
                            }
                            string path = "D:\\test.txt";
                            FileStream fs = File.OpenWrite(path);
                            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                            fs.Seek(0, SeekOrigin.End);
                            sw.WriteLine(message);
                            sw.Close();
                            fs.Close();
                            AddItem(string.Format("广播:[{0}]", message));
                            break;

                        //logout
                        case "logout":
                            for (int i = 0; i < userlist.Count; i++)
                            {
                                if (userlist[i].GetName() == splitmessage[1])
                                {
                                    AddItem(string.Format("用户{0}({1})退出", userlist[i].GetName(), userlist[i].GetIPEndPoint()));
                                    userlist.RemoveAt(i);
                                }
                            }
                            //send to others
                            for (int i = 0; i < userlist.Count; i++)
                            {
                                SendtoClient(userlist[i], message);

                            }
                            string path1 = "D:\\test.txt";
                            FileStream fs1 = File.OpenWrite(path1);
                            StreamWriter sw1 = new StreamWriter(fs1, Encoding.Default);
                            fs1.Seek(0, SeekOrigin.End);
                            sw1.WriteLine(message);
                            sw1.Close();
                            fs1.Close();
                            AddItem(string.Format("广播:[{0}]", message));
                            break;

                    }
                }
                catch
                {
                    break;
                }
            }
                AddItem(string.Format("服务线程({0})终止", ipendpoint));
            }
    
        private delegate void AddItemToListBoxDelegate(string str);
        private void AddItem(string str)
        {
            if (status.InvokeRequired)
            {
                AddItemToListBoxDelegate d = AddItem;
                status.Invoke(d, str);
            }
            else
            {
                status.Items.Add(str);
                status.TopIndex = status.Items.Count - 1;
                status.ClearSelected();
            }
        }
        private void ListenClientConnect()
        {
            TcpClient newClient = null;
            while (true)
            {
                try
                {
                    //achieve tcp socket for transport data
                    newClient = myTcplistener.AcceptTcpClient();
                    AddItem(string.Format("接受客户端{0}的 TCP 请求", newClient.Client.RemoteEndPoint));
                }
                catch
                {
                    AddItem(string.Format("监听线程({0}:{1})终止", ipaddress, tcport));
                    break;
                }

                //启动发送用户列表线程
                Thread threadSend = new Thread(SendData);
                threadSend.Start(newClient);
            }
        }
        private void SendData(object userClient)
        {
            TcpClient newUserClient = (TcpClient)userClient;
            userListString= null;
            for (int i = 0; i < userlist.Count; i++)
            {
                userListString += userlist[i].GetName() + "," + userlist[i].GetIPEndPoint().ToString() + ";";
            }
            userListString += "end";
            netstream = newUserClient.GetStream();
            bw = new BinaryWriter(netstream);
            bw.Write(userListString);
            bw.Flush();      //don't save the current message
            AddItem(string.Format("向{0}传送:[{1}]", newUserClient.Client.RemoteEndPoint, userListString));
            bw.Close();
            newUserClient.Close();
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // creating receive socket
            ipaddress = IPAddress.Parse(textBox1.Text);
            ipendpoint = new IPEndPoint(ipaddress, mport);
            receiveclient = new UdpClient(ipendpoint);
            //start receive thread
            
                Thread threadreceive = new Thread(ReceiveMessage);
                threadreceive.Start();
               // MessageBox.Show("开始准备接受");
                button1.Enabled = false;

                button2.Enabled = true;
            
            //randomly assign listening port
            Random random = new Random();
            tcport = random.Next(mport + 1, 65536);

            //create listening socket
            myTcplistener = new TcpListener(ipaddress, tcport);
            myTcplistener.Start();

            //starting listening thread;
            Thread listenThread = new Thread(ListenClientConnect);
            listenThread.Start();
            AddItem(string.Format("服务线程({0})启动，监听端口{1}", ipendpoint, tcport));
          
            
         
         
        }

        private void button2_Click(object sender, EventArgs e)
        {

            myTcplistener.Stop();
            receiveclient.Close();
            button1.Enabled = true;
            button2.Enabled = false;
            MessageBox.Show("已断开连接");
        }
    }
}
