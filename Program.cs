using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MSATClient
{
    class Program
    {
        static Boolean clientStatus = false;
        static void Main(string[] args)
        {
            IPEndPoint serverIP = new IPEndPoint(IPAddress.Parse("192.168.247.1"), 4444);
            Socket tcpClient = null;
            MSATSocket msatSocket = null;
            Boolean timesFlag = true;
            
            while (true)
            {
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
                }
                catch(Exception ex)
                {
                    //Console.WriteLine("Socket连接失败或异常中断！EX："+ex.Message);
                    msatSocket.setTcpStatus(false);
                }
            }
        }
    }
}
