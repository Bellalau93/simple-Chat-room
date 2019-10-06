using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Text;

namespace sockettest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
           /* int port = 8888;
            String ip = 
            IPAddress Ip = IPAddress.Parse(ip);
            EndPoint ipe = new IPEndPoint(Ip, port);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(0);
            Console.WriteLine("waiting for connection");
            Socket Temp = s.Accept();
            Console.WriteLine("connecting");
            String recvStr = "";
            byte[] recvBty = new byte[1024];
            int bytes;
            bytes = Temp.Receive(recvBty, recvBty.Length, 0);
            recvStr += Encoding.UTF8.GetString(recvBty, 0, bytes);

            Console.WriteLine("servergetmessage:{0},", recvStr);
            String sendstr = "Ok!client send message successfully";
            byte[] bs = Encoding.UTF8.GetBytes(sendstr);
            Temp.Send(bs, bs.Length, 0);
            Temp.Close();
            s.Close();
            Console.ReadLine();*/
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new server());
        }
    }
}
