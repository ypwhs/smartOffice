using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Configuration;
using System.Collections;
using System.Web;
using System.IO;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form 
    {
        public int zhuanxuhao(char char1, char char2)
        {
            try
            {
                return Convert.ToInt32(char1.ToString() + char2.ToString());
            }
            catch (Exception exc)
            {
                return -1;
            }
        }

        public void Send(string a, TcpClient tcpclient)
        {
            byte[] bs = Encoding.UTF8.GetBytes(a);
            tcpclient.Client.Send(bs, bs.Length, 0);
        }

        public Form1()
        {
            InitializeComponent();
        }


        //
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            tSpy8080 = new Thread(new ThreadStart(spy8080));
            tSpy8080.IsBackground = true;
            tSpy8080.Start();
        }
        TcpListener tcplistener8080;

        int[] hongwai = new int[1000];
        int[] chair = new int[1000];
        int[] time = new int[1000];


        private void timecount()
        {
            int j;
                try
                {
                    while (true)
                    {
                        Thread.Sleep(10);
                        for (j = 1; j < 21; j++)
                            if ((hongwai[j] == 0 || chair[j] == 0) && (time[j] > 0))
                                time[j]--;
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                }
        }

        private void spy80()
        {
            int j;
            

            TcpListener listener80;
            

            try
            {
                listener80 = new TcpListener(IPAddress.Any, 80);
                listener80.Start();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Spy80 Failed.");
                return;
            }

            byte[] buffer = new byte[1024];
            String result = null;

            while (true)
            {
                TcpClient client = listener80.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                
                
                int i;
                i = stream.Read(buffer, 0, buffer.Length);
                result = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                Console.WriteLine(result);
                try
                {
                    if (result.IndexOf("1.jpg") > 0) { client.Client.SendFile("1.jpg"); client.Close(); continue; }
                    else if (result.IndexOf("2.jpg") > 0) { client.Client.SendFile("2.jpg"); client.Close(); continue; }
                }
                catch (Exception ex)
                { 
                }

                String html = "";

                html = html + "<html><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">";
                html+="<meta http-equiv=\"refresh\" content=\"10\">";
                html = html +"<table border=\"1\">";
                for (j = 1; j < 21;j++ )
                {
                    if (j == 1) html += "<tr>";
                    html += "<td>";

                    if(hongwai[j]==1&&chair[j]==1)html+="<img src=http://192.168.1.123/1.jpg>";
                    else html += "<img src=http://192.168.1.123/2.jpg>";

                    html = html + "<br>" + j.ToString() + "号员工<br>";
                    
                    if (hongwai[j]==1)html += "红外在线";
                    else html += "红外离线";
                    
                    html += "<br>";
                    
                    if (chair[j] == 1)html += "坐垫在线";
                    else html += "坐垫离线";

                    html += "<br>倒计时"+ time[j];

                    html += "</td>";

                    if (j %10 == 0)
                        html += "</tr><tr>";
                }
                html += "</table>";
                html = html + "</html>";
                String kb = new String('\n', 500);

                byte[] byteArray = Encoding.UTF8.GetBytes(html);

                html = "HTTP/1.1 200 OK\nContent-Length: " + byteArray.Length.ToString()+"\nContent-Type: text/html\nCache-control: private\nContent-Type: text/html\n\n" + html;
                Send(html, client);
                client.Close();
            }
            
        }

        private void spy8080()
        {
            int a;
            try
            {
                tcplistener8080 = new TcpListener(IPAddress.Any, 8080);
                tcplistener8080.Start();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Spy8080 Failed.");
                return;
            }
            
            byte[] buffer = new byte[1024];
            String clientstr = null;

            try
            {
                while (true)
                {
                    TcpClient tcpclient80 = tcplistener8080.AcceptTcpClient();
                    NetworkStream stream = tcpclient80.GetStream();
                    int i;
                    while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        clientstr = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                        //返回文本
                        label3.Text = clientstr;
                        if (clientstr.Length != 4)
                        {
                            continue;
                            //如果不是4个字节就丢弃
                        }
                        else
                        {
                            a = zhuanxuhao(clientstr[0], clientstr[1]);
                            //取前两位转换数字获取序号
                            if (a == -1)
                            {
                                continue;
                                //非法数字就丢弃
                            }
                            else
                            {
                                int zhuangtai;
                                if (clientstr[3] == '1') zhuangtai = 1;
                                else if (clientstr[3] == '0') zhuangtai = 0;
                                else continue;
                                //获取状态量

                                if (clientstr[2] == 'H')hongwai[a] = zhuangtai;
                                else if (clientstr[2] == 'D')chair[a] = zhuangtai;
                                else continue;
                                //置状态量
                            }
                            try
                            {
                                if (hongwai[a] == 1 && chair[a] == 1)
                                {
                                    time[a] = 3600;
                                    if (clientstr[2] == 'H' && time[a] > 0)
                                    {
                                        if (chair[a] == 1)
                                            Send("11", tcpclient80);
                                        else
                                            Send("10", tcpclient80);
                                    }
                                }
                                else
                                {
                                    if (clientstr[2] == 'H' && time[a] == 0)
                                    {
                                        if (chair[a] == 1)
                                            Send("01", tcpclient80);
                                        else
                                            Send("00", tcpclient80);
                                    }
                                }
                                label4.Text = "剩余时间"+time[a]+"秒";
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        try
                        {
                            tcpclient80.Close();
                            stream.Close();
                            //无论是否还有数据都关闭
                        }
                        catch (Exception ex)
                        {
                        }
                        break;
                        //无论是否还有数据都关闭
                    }
                }
            }
            catch (Exception exc)
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        Thread tSpy8080;
        Thread tTime;
        Thread tSpy80;
        private void Form1_Load(object sender, EventArgs e)
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            tTime = new Thread(new ThreadStart(timecount));
            tTime.IsBackground = true;
            tTime.Start();
            //启动计时线程
            tSpy80 = new Thread(new ThreadStart(spy80));
            tSpy80.IsBackground = true;
            tSpy80.Start();
            //启用网页线程
            button1.Enabled = false;
            tSpy8080 = new Thread(new ThreadStart(spy8080));
            tSpy8080.IsBackground = true;
            tSpy8080.Start();
            //按钮1被点击
        }
        private void kill(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

    }
}
