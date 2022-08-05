using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ProxyChecker
{
    public partial class Form1 : Form
    {
        int flows = 32;
        static List<string> Proxy = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            flows = (int)numericUpDown1.Value;
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\proxyList\\proxy.txt"))
            {
                Proxy = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\proxyList\\proxy.txt").ToList();
            }
            else
            {
                File.Create(Directory.GetCurrentDirectory() + "\\proxyList\\proxy.txt");
            }            
            try
            {
                richTextBox1.Text += Proxy[0];
                richTextBox1.Text = "файл успешно открыт";
            }
            catch (Exception)
            {
                richTextBox1.Text = "файл пустой";
                File.OpenText(Directory.GetCurrentDirectory() + "\\proxyList\\proxy.txt");
            }
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            if (Proxy.Count == 0)
            {
                richTextBox1.Text += "выберите файл \n";
            }
            else
            {
                richTextBox1.Text = "____Проверка началась____ \n";
                for (int i = 0; i < flows; i++)
                {
                    Thread Threads = (new Thread(() => CheckProxy(i)));
                    Threads.Start();
                }
            }            
        }
        private void buttonDownload_Click(object sender, EventArgs e)
        {
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\proxyList\\result.txt", richTextBox1.Text);
        }
        private void CheckProxy(int j)
        {
            for (int i = j; i < Proxy.Count; i += ++flows)
            {
                Proxy[i] = Proxy[i].Replace(" ", "");
                HttpRequest request = new HttpRequest();
                CheckHttp(request, i, j);
            }
        }

        private void CheckHttp(HttpRequest request, int i, int j)
        {
            try
            {
                request.Proxy = ProxyClient.Parse(ProxyType.HTTP, Proxy[i]);
                WriteInfo("HTTP", i, j, request);
            }
            catch (Exception) { CheckSocks4(request, i, j); }
        }

        private void CheckSocks4(HttpRequest request, int i, int j)
        {
            try
            {
                request.Proxy = ProxyClient.Parse(ProxyType.Socks4, Proxy[i]);
                WriteInfo("SOCKS4", i, j, request);
            }
            catch (Exception) { CheckSocks5(request, i, j); }
        }

        private void CheckSocks4a(HttpRequest request, int i, int j)
        {
            try
            {
                request.Proxy = ProxyClient.Parse(ProxyType.Socks4A, Proxy[i]);
                WriteInfo("SOCKS4A", i, j, request);
            }
            catch (Exception)
            {
                Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.Text += (Proxy[i] + "         \t died" + "      \t" + j + " поток" + '\n');
                }));                
            }
        }

        private void CheckSocks5(HttpRequest request, int i, int j)
        {
            try
            {
                request.Proxy = ProxyClient.Parse(ProxyType.Socks5, Proxy[i]);
                WriteInfo("SOCKS5", i, j, request);
            }
            catch (Exception) { CheckSocks4a(request, i, j); }
        }

        private void WriteInfo(string v, int i, int j, HttpRequest request)
        {
            request.Get("https://proxy-checker.net").ToString();
            Invoke((MethodInvoker)(() =>
            {
                richTextBox1.Text += (Proxy[i] + "         \t work       \t" + j + " поток      " + '\t' + v + '\n');
            }));
        }
    }
}
