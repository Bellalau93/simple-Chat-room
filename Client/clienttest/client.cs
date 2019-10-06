using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;


namespace clienttest
{
    public partial class client : Form
    {
    //server connection and statement
        int rand1, rand2, clientport,count;
        int serverport = 8888;
        IPAddress serverIP=IPAddress.Parse("10.138.136.132");
        IPAddress localIP;
        private UdpClient sendclient;//send socket
        private UdpClient receiveclient;//receive socket
        private IPEndPoint clientendpoint;//client address
        private TcpClient myTcpclient;
        private NetworkStream networkstream;
        private BinaryReader br;
        string userliststring;
        public string username;
        private delegate string GetItemListViewCallBack(string str);
        private GetItemListViewCallBack getItemListViewCallBack;
        private List<chat> chatlist = new List<chat>();
        private List<chats> chatslist = new List<chats>(); 



        public client()
        {
            InitializeComponent();
            localIP = IPAddress.Parse(getLocalIP());
            textBox2.Text = getLocalIP();
            
            button1.Enabled = true;
            button2.Enabled = false;
            getItemListViewCallBack = new GetItemListViewCallBack(GetItemListView);

        }
        private void client_Load(object sender, EventArgs e)
        {

        }

        //get local ip
        public static string getLocalIP()
        {
            try
            {
                string HOSTname = Dns.GetHostName();
                IPHostEntry entry = Dns.GetHostEntry(HOSTname);
                for (int i = 0; i < entry.AddressList.Length; i++){
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
        
      
        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        //********************************
        //连接服务器，接受信息，更新状态
        //********************************
        #region connect and state
        private void button1_Click(object sender, EventArgs e)
        {

            Random newrandon = new Random();
            rand1 = newrandon.Next(5, 200);
            rand2 = newrandon.Next(0, 200);
            clientport = (rand1 << 8) | rand2;
            textBox3.Text = clientport.ToString();
            clientendpoint = new IPEndPoint(localIP, clientport);
            receiveclient = new UdpClient(clientendpoint);

            //starting receive thread
            Thread receivethread = new Thread(ReceiveMessage);
            receivethread.Start();
            AddItemToListBox(string.Format("客户线程({0})启动", clientendpoint));
            //匿名发送
            sendclient = new UdpClient(0);
            //启动发送线程
            Thread threadSend = new Thread(SendMessage);
            username = textBox1.Text;
            threadSend.Start(string.Format("login,{0},{1}", username, clientendpoint));
            AddItemToListBox(string.Format("发出:[login,{0},{1}]", username, clientendpoint));
            button1.Enabled = false;
            button2.Enabled = true;
        }

       

        //接受信息封装，包括收到login logout talk（私聊） talks（群聊）
        private void ReceiveMessage()
        {
            IPEndPoint remoteIPendpoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    //exception if close the udpclient
                    byte[] receivebytes = receiveclient.Receive(ref remoteIPendpoint);
                    string message = Encoding.Unicode.GetString(receivebytes, 0, receivebytes.Length);

                    // show message
                    AddItemToListBox(string.Format("{0}:[{1}]", remoteIPendpoint, message));
                    string[] splitmessage = message.Split(',');
                    //根据不同指令执行
                    switch (splitmessage[0])
                    {
                        //收到服务端的accept指令表示接受连接
                        case "Accept":
                            try
                            {
                                AddItemToListBox(string.Format("连接{0}:{1}...", remoteIPendpoint.Address, splitmessage[1]));
                                IPHostEntry hostentry = Dns.GetHostEntry(remoteIPendpoint.Address);
                                myTcpclient = new TcpClient();
                                myTcpclient.Connect(remoteIPendpoint.Address, int.Parse(splitmessage[1]));
                                if (myTcpclient != null)
                                {
                                    AddItemToListBox("连接成功！");
                                    networkstream = myTcpclient.GetStream();
                                    //读取服务端的记录
                                    br = new BinaryReader(networkstream);
                                    string path = "D:\\test.txt";
                                    string[] str = new string[100];
                                    FileStream fs = File.OpenRead(path);
                                    StreamReader sr = new StreamReader(fs, Encoding.Default);
                                    int k = 0;
                                    str[k] = sr.ReadLine();
                                    while (str[k] != null)
                                    {
                                        k++;
                                        str[k] = sr.ReadLine();
                                    }
                                    for (int j = 0; j < k; j++)
                                    {
                                        string[] splitstring = str[j].Split(',');
                                        switch (splitstring[0])
                                        {
                                            case "login":
                                                if (splitstring[1] != username) AddItemToListBox(string.Format("系统消息：{0}({1})加入", splitstring[1], splitstring[2])); break;
                                            case "logout": AddItemToListBox(string.Format("系统消息：{0}({1})退出", splitstring[1], splitstring[2])); break;
                                        }
                                    }
                                    sr.Close();
                                    fs.Close();
                                }
                            }
                            catch
                            {
                                AddItemToListBox("连接失败..");
                            }
                            //得到用户列表
                            Thread threadGetList = new Thread(GetUserList);
                            threadGetList.Start();
                            break;
                        //得到其他用户登录消息
                        case "login":
                            AddItemToListBox(string.Format("新用户{0}({1})加入",splitmessage[1],splitmessage[2]));
                            string userItemInfo = splitmessage[1] + "," + splitmessage[2];
                            int count = 0;
                            //更新用户状态
                            for (int i = 0; i < listView1.Items.Count; i++)
                            {
                                string str = i.ToString();
                                string str2 = (string)listView1.Invoke(getItemListViewCallBack, str);
                                if (str2 == splitmessage[1])
                                {
                                    int n = 1;
                                    string s = splitmessage[1] + "," + n.ToString();
                                    SetListState(s);  
                                }
                                else
                                {
                                    count++;
                                }
                            }
                            if (count == listView1.Items.Count) { 
                                AddItemToListView(userItemInfo);
                            }
                    break;
                        //有用户退出，并更新用户列表
                        case "logout":
                            AddItemToListBox(string.Format("用户{0}({1})退出", splitmessage[1], splitmessage[2]));
                            int l = 0;
                            string s1 = splitmessage[1] + "," + l.ToString();
                            SetListState(s1);
                            break;
                        //有用户发出聊天请求
                        case "talk":
                            for (int i = 0; i < chatlist.Count; i++)
                            {
                                if (chatlist[i].Text== splitmessage[2])
                                    chatlist[i].showmessage(splitmessage[2], splitmessage[1], splitmessage[3]);
                            }
                            for (int i = 0; i < chatslist.Count; i++)
                                if (chatslist[i].Text != splitmessage[2])
                                {
                                    chatslist[i].showmessage(splitmessage[2], splitmessage[1], splitmessage[3]);
                                }
                            break;
                     }
                                
                }
                catch
                {
                    break;
                }
                AddItemToListBox(string.Format("客户线程({0})终止", clientendpoint));
            }
        }
        //发送信息
        private void SendMessage(object obj)
        {
            
                string message = (string)obj;
                byte[] sendbytes = Encoding.Unicode.GetBytes(message);
                IPEndPoint remoteIPEndPoint = new IPEndPoint(serverIP, serverport);
                sendclient.Send(sendbytes, sendbytes.Length, remoteIPEndPoint);  //匿名发送
                MessageBox.Show(message);
                sendclient.Close();
         
        }
        //得到用户名
        private string GetItemListView(string str)
        {
            int i = Convert.ToInt32(str);
            return listView1.Items[i].SubItems[0].Text;

        }
        //状态栏添加信息
        private delegate void AddItemToListBoxDelegate(string str);
        private void AddItemToListBox(string str)
        {
            if (listBox1.InvokeRequired)
            {
                AddItemToListBoxDelegate d = AddItemToListBox;
                listBox1.Invoke(d, str);
            }
            else
            {
                listBox1.Items.Add(str);
                listBox1.TopIndex = listBox1.Items.Count - 1;
                listBox1.ClearSelected();
            }
        }

        //添加用户列表信息
        private delegate void AddItemToListViewDelegate(string str);
        private void AddItemToListView(string str)
        {
            if (listView1.InvokeRequired)
            {
                AddItemToListViewDelegate d = AddItemToListView;
                listView1.Invoke(d, str);
            }
            else
            {
                string[] splitString = str.Split(',');
                listView1.Items.Add(new ListViewItem(new string[] { splitString[0], splitString[1], "在线" }));
               
            }
        }
        //获取用户列表
        private void GetUserList()
        {
            while (true)
            {
                userliststring = null;
                try
                {
                    userliststring = br.ReadString();
                    if (userliststring.EndsWith("end"))
                    {
                        AddItemToListBox(string.Format("收到:[{0}]", userliststring));

                        string[] splitString = userliststring.Split(';');
                        for (int i = 0; i < splitString.Length - 1; i++)
                        {
                            AddItemToListView(splitString[i]);
                        }
                        br.Close();
                        myTcpclient.Close();
                    }

                    break;
                }
                catch
                {
                    break;
                }
            }
        }
        //更新用户状态
        private delegate void SetListStateDelegate(string str);
        private void SetListState(string str)
        {
            if (listView1.InvokeRequired)
            {
                SetListStateDelegate d = SetListState;
                listView1.Invoke(d, str);
            }
            else
            {
                for(int i = 0; i < listView1.Items.Count; i++)
                {
                    string []splitmessage = str.Split(',');
                    int k = Convert.ToInt32(splitmessage[1]);
                    if (listView1.Items[i].SubItems[1].Text == splitmessage[0] && k == 1)
                    {
                        listView1.Items[i].SubItems[3].Text = "在线";
                    }
                    else if(listView1.Items[i].SubItems[1].Text==splitmessage[0]&&k==0)
                    {
                        listView1.Items[i].SubItems[3].Text = "离线";
                    }
                }
            }
        }

        #endregion
        private void button8_Click(object sender, EventArgs e)
        {
            string s = "";
            string []s1 = new string[10];
            string[] s2 = new string[10];
            IPEndPoint[] ep = new IPEndPoint[10];
            int count = 0;
            for(int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[0].Text != username)
                {
                    s1[count] = listView1.Items[i].SubItems[0].Text;
                    s2[count] = listView1.Items[i].SubItems[1].Text;
                    string[] splitstring = s2[count].Split(':');
                    IPAddress ip = IPAddress.Parse(splitstring[0]);
                    ep[count]= new IPEndPoint(ip, int.Parse(splitstring[1]));
                    count++;
                }
                s = s + listView1.Items[i].SubItems[1].Text + "、";
            }
            chats chatgroupform = new chats();
            chatgroupform.SetUserInfo(username, s1, ep, count);
            chatgroupform.Text = username;
            chatslist.Add(chatgroupform);
            chatgroupform.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sendclient = new UdpClient(0);
            Thread threadsend = new Thread(SendMessage);
            threadsend.Start(String.Format("logout,{0},{1}", username, clientendpoint));
            receiveclient.Close();
            listView1.Items.Clear();
            listBox1.Items.Clear();
            button2.Enabled = false;
            button1.Enabled = true;
            textBox1.Text = "";
            textBox3.Text = "";

            this.Text = "Client";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            int index = 0;
            if (listView1.SelectedItems.Count > 0)
            {
                index = this.listView1.SelectedItems[0].Index;
                string name = listView1.Items[index].SubItems[0].Text;
                string add = listView1.Items[index].SubItems[1].Text;
                string[] splitstring = add.Split(':');
                IPAddress peerip = IPAddress.Parse(splitstring[0]);
                IPEndPoint peerend = new IPEndPoint(peerip, int.Parse(splitstring[1]));
                chat chatform = new chat();
                chatform.SetUserInfo(username, name, peerend);
                chatform.Text = name;
                chatlist.Add(chatform);
                chatform.Show();

            }

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
          

        }
     
    }

}
