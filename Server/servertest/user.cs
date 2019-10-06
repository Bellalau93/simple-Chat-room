using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace sockettest
{
    class user
    {
        private string userName;           //用户名
        private IPEndPoint userIPEndPoint;  //用户地址 
        public user(string name, IPEndPoint ipEndPoint)
        {
            userName = name;
            userIPEndPoint = ipEndPoint;
        }
        public string GetName()
        {
            return userName;
        }
        public IPEndPoint GetIPEndPoint()
        {
            return userIPEndPoint;
        }
    }
}
