using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MSATClient
{
    class Program
    {
        static Boolean clientStatus = false;
        static long startTimeStamp = GetTimeStamp();
        static String serverIPAdress = "192.168.1.1";
        static short serverPort = 4444;
        static long stayTime = -1;
        static long endTimeStamp = 0;

        static void Main(string[] args)
        {
            IPEndPoint serverIP = null;
            if (args != null)
            {
                if (args.Length == 2)
                {
                    serverIPAdress = args[0];
                    serverPort = Convert.ToInt16(args[1]);
                }
                else if (args.Length == 3)
                {
                    serverIPAdress = args[0];
                    serverPort = Convert.ToInt16(args[1]);
                    endTimeStamp = Convert.ToInt64(args[2]) * 60 + startTimeStamp;
                }
                else
                {
                    System.Environment.Exit(System.Environment.ExitCode);
                }
            }
            else
            {
                System.Environment.Exit(System.Environment.ExitCode);
            }
            try
            {
                serverIP = new IPEndPoint(IPAddress.Parse(serverIPAdress), serverPort);
            }
            catch (Exception ex)
            {
                System.Environment.Exit(System.Environment.ExitCode);
            }
            Socket tcpClient = null;
            MSATSocket msatSocket = null;
            //Boolean timesFlag = true;

            while (true)
            {
                if (endTimeStamp != 0)
                {
                    if (GetTimeStamp() > endTimeStamp)
                    {
                        System.Environment.Exit(System.Environment.ExitCode);
                    }
                }
                msatSocket = new MSATSocket();
                try
                {
                    //Console.WriteLine("clientStatus : "+ clientStatus);
                    if (!clientStatus)
                    {
                        //Console.WriteLine("监听中...");
                        tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        tcpClient.Connect(serverIP);
                        //Console.WriteLine("连接成功！");
                        msatSocket.TcpSocket(tcpClient);
                        clientStatus = true;
                    }

                    /** 判断Socket是否断开！若断开则重新连接 **/
                    try
                    {
                        byte[] tmp = new byte[1];
                        tcpClient.Send(tmp, 0, 0);
                    }
                    catch
                    {
                        msatSocket.setTcpStatus(false);
                        clientStatus = false;
                    }
                    Thread.Sleep(100);
                    GC.Collect();
                }
                catch(Exception ex)
                {
                    tcpClient = null;
                    //Console.WriteLine("Socket连接失败或异常中断！EX："+ex.Message);
                    //msatSocket.setTcpStatus(false);
                    msatSocket = null;
                    GC.Collect();
                }
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

    }
}
