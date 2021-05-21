using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace VitorBattleClientTest
{
    class VitorBattleClient
    {
        public class GameLog
        {
            public static void Log(string content, ConsoleColor color = ConsoleColor.Gray)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(content);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public static char packageChar = Encoding.UTF8.GetChars(new byte[] { 255 })[0];
        public static MD5 md5 = new MD5CryptoServiceProvider();
        public static TcpClient Client = new TcpClient("49.234.233.59", 17483);
        public static NetworkStream nwStream = Client.GetStream();
        public static int packageserverid = 0;
        public static int packageclientid = 0;

        public static string MD5Encrypt(string strText)
        {
            byte[] result = md5.ComputeHash(Encoding.Default.GetBytes(strText));
            string res = "";
            foreach (byte b in result) res += string.Format("{0:X}", b);
            return res;
        }
        static void SendWithCheckCode(string content)
        {
            string checkcode = MD5Encrypt("packagecheck" + (packageclientid - packageserverid) * 40.4);
            GameLog.Log($"计算检查码：{checkcode}");
            Send(content + packageChar + checkcode);
            if (packageclientid >= int.MaxValue - 12) packageclientid = int.MinValue;
            packageclientid+=12;
        }
        static void Send(string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content + packageChar);
            nwStream.Write(buffer, 0, buffer.Length);
        }
        static void KeepAlive()
        {
        head:
            try
            {
                while (Client.ReceiveBufferSize <= 0) ;

                byte[] buffer = new byte[Client.ReceiveBufferSize];
                int bytesRead = nwStream.Read(buffer, 0, Client.ReceiveBufferSize);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                string[] package = data.Split(packageChar);
                if(packageclientid == 0)
                {
                    string[] temp = package[0].Split(',');
                    packageclientid = int.Parse(temp[0]);
                    packageserverid = int.Parse(temp[1]);
                    GameLog.Log($"取得ID：{packageclientid},{packageserverid}");
                    goto SkipCheck;
                }
                for (int i = 0; i < package.Length - 1; i += 2)
                {
                    if (package.Length == i) throw new Exception("非法的数据包！");
                    string checkcode = MD5Encrypt("packagecheck" + (packageserverid - packageclientid) * 40.4);
                    if (checkcode == package[i + 1])
                    {
                        GameLog.Log($"服务器：{package[i]}\n包检查码：{checkcode}（√）");
                        GameLog.Log($"延迟：{(DateTime.Now.Ticks - long.Parse(package[i])) * 0.1} 纳秒");
                    }
                    else
                    {
                        throw new Exception($"数据包检查码不匹配:{checkcode}（×）");
                    }
                    if (packageserverid >= int.MaxValue - 12) packageserverid = int.MinValue;
                    packageserverid +=12;
                }
            SkipCheck:
                data = "";
            }
            catch (Exception err)
            {
                GameLog.Log($"服务器由于异常断开了连接：{err.Message}", ConsoleColor.Red);
                Client.Close();
                return;
            }
            goto head;
        }

        static void Main(string[] args)
        {
            new Thread(new ThreadStart(KeepAlive)).Start();
            while (true)
            {
                string msg = Console.ReadLine();
                if(msg == "--")
                {
                    GameLog.Log($"故意直接乱发", ConsoleColor.Yellow);
                    packageclientid /= 2;
                }
                else if (msg == "++")
                {
                    SendWithCheckCode(DateTime.Now.Ticks.ToString());
                }
                else
                {
                    SendWithCheckCode(msg);
                }
            }
            
        }
    }
}
