using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace VitorBattleServer
{
    class WebCommunication
    {
        public static char packageChar = Encoding.UTF8.GetChars(new byte[] { 255 })[0];
        public static MD5 md5 = new MD5CryptoServiceProvider();
        public static string MD5Encrypt(string strText)
        {
            byte[] result = md5.ComputeHash(Encoding.Default.GetBytes(strText));
            string res = "";
            foreach (byte b in result) res += string.Format("{0:X}", b);
            return res;
        }
        static void KeepAlive(object client)
        {
            TcpClient Client = (TcpClient)client;
            NetworkStream nwStream = Client.GetStream();
            int packageserverid = Guid.NewGuid().GetHashCode();
            int packageclientid = Guid.NewGuid().GetHashCode();
            void SendWithCheckCode(string content)
            {
                string checkcode = MD5Encrypt("packagecheck" + (packageserverid - packageclientid) * 40.4);
                Send(content + packageChar + checkcode);
                if (packageserverid >= int.MaxValue - 12) packageserverid = int.MinValue;
                packageserverid +=12;
            }
            void Send(string content)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(content + packageChar);
                nwStream.Write(buffer, 0, buffer.Length);
            }
            Send(packageclientid + "," + packageserverid);
        head:
            try
            {
                while (Client.ReceiveBufferSize <= 0) ;

                byte[] buffer = new byte[Client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(buffer, 0, Client.ReceiveBufferSize);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                string[] package = data.Split(packageChar);
                for(int i = 0;i < package.Length - 1; i+=2)
                {
                    if (package.Length == i) throw new Exception("非法的数据包！");
                    string checkcode = MD5Encrypt("packagecheck" + (packageclientid - packageserverid) * 40.4);
                    if (packageclientid >= int.MaxValue - 12) packageclientid = int.MinValue;
                    packageclientid += 12;
                    if (checkcode == package[i + 1])
                    {
                        GameLog.Log($"玩家（{Client.GetHashCode()}）：{package[i]}\n包检查码：{checkcode}（√）");
                        SendWithCheckCode(package[i]);
                    }
                    else
                    {
                        throw new Exception($"数据包检查码不匹配:{package[i+1]}（×）\n期望：{checkcode}");
                    }
                }
            }
            catch(Exception err)
            {
                GameLog.Log($"玩家（{Client.GetHashCode()}）由于异常被断开了连接：{err.Message}",ConsoleColor.Red);
                Client.Close();
                return;
            }
            goto head;
        }
        public static void Listening()
        {
        listen:
            TcpListener listener = new TcpListener(IPAddress.Any, 17483);
            listener.Start();
            TcpClient client = listener.AcceptTcpClient();
            new Thread(new ParameterizedThreadStart(KeepAlive)).Start(client);
            GameLog.Log($"玩家（{client.GetHashCode()}）连接上了服务器。", ConsoleColor.Green);
            listener.Stop();
            goto listen;
        }
    }
}
