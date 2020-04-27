using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using sto.DataObjects;

namespace sto
{
    public partial class Captcha : Form
    {
        public static int CaptchaThreadNumber { get; set; }
        public static int CaptchaNumberToSave { get; set; }
        public bool Working { get; set; } = true;
        public static List<Captcha> Captchas { get; set; } = new List<Captcha>();
        public ChromiumWebBrowser chromeBrowser { get; private set; }
        public static readonly List<Host> Hosts = new List<Host>();
        private Host current;
        public static Main Main { get; set; }
        private int i = 0;
        public string ChromeBrowserHTML { get; set; }
        private string URLToFile(string str)
        {
            str = str.Replace(":", "");
            str = str.Replace(@"\", "");
            str = str.Replace("/", "");
            str = str.Replace("*", "");
            str = str.Replace("?", "");
            str = str.Replace("<", "");
            str = str.Replace(">", "");
            str = str.Replace("|", "");
            return str;
        }
        public Captcha()
        {
            InitializeComponent();
            InitializeChromium();
        }
        public void InitializeChromium()
        {
            chromeBrowser = new ChromiumWebBrowser("http://lordkian.ir/");
            groupBox1.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;
            chromeBrowser.AddressChanged += ChromeBrowser_AddressChanged;
            chromeBrowser.FrameLoadEnd += ChromeBrowser_FrameLoadEnd;
        }
        private void ChromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {

            //chromeBrowser.ViewSource();
            //chromeBrowser.GetSourceAsync().ContinueWith(taskHtml =>
            //{
            //    ChromeBrowserHTML = taskHtml.Result;
            //    int j = 0;
            //    var name = URLToFile(e.Url);
            //    while (File.Exists(name + "__" + j + ".html"))
            //        j++;
            //    var sw = new StreamWriter(name + "__" + j + ".html");
            //    sw.WriteLine(taskHtml.Result);
            //    sw.Close();

            //});

        }
        private void ChromeBrowser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            if (current == null)
                return;
            if (!e.Address.Contains("s.to"))
            {
                var b = false;
                foreach (var item in HostPriority.SiteAddrs)
                    b |= e.Address.Contains(item);


                if (b)
                {
                    current.HostURL = e.Address;
                    if (Captchas.Count > CaptchaThreadNumber)
                    {
                        current = null;
                        Captchas.Remove(this);
                        Close();
                    }
                    else
                        LoadURL();
                }
                else
                    chromeBrowser.Load(current.URL);
            }
        }
        public static void AddHost(Host host)
        {
            Hosts.Add(host);
            AddCapcha();
        }
        public static void AddHosts(IEnumerable<Host> hosts)
        {
            Hosts.AddRange(hosts);
            AddCapcha();
        }
        public static void AddCapcha()
        {
            foreach (var item in Captchas)
                item.Start();
            while (Captchas.Count < CaptchaThreadNumber)
            {
                var c = new Captcha();
                c.Show();
                Captchas.Add(c);
            }
        }
        private void CrossThreadClose()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(CrossThreadClose));
            }
            else
                Close();
        }
        public void LoadURL()
        {
            current = null;

            lock (Hosts)
            {
                if (Hosts.Count > 0)
                {
                    current = Hosts.First();
                    Hosts.Remove(current);
                }
            }

            if (current == null)
            {
                new Thread(() => { Main.SaveAll(); }).Start();
                chromeBrowser.Load("http://s.to/");
                chromeBrowser.AddressChanged -= ChromeBrowser_AddressChanged;
                Working = false;
            }
            else
            {
                chromeBrowser.Load(current.URL);
                i++;
                if (i >= CaptchaNumberToSave)
                {
                    i = 0;
                    new Thread(() => { Main.SaveAll(); }).Start();
                }
            }
        }
        public static void End()
        {
            var l = new List<Captcha>();
            l.AddRange(Captchas);
            foreach (var item in l)
                item.Close();
            CaptchaThreadNumber = 0;
            Cef.Shutdown();
        }
        public void Start()
        {
            if (!Working && Hosts.Count > 0)
            {
                Working = true;
                chromeBrowser.AddressChanged += ChromeBrowser_AddressChanged;
                LoadURL();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Captchas != null && Captchas.Count > 0)
            {
                if (Captchas.Contains(this))
                    Captchas.Remove(this);
                if (current != null)
                    Hosts.Add(current);
            }
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            LoadURL();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                chromeBrowser.Load("http://lordkian.ir/");
                Thread.Sleep(200);
                chromeBrowser.Load(current.URL);
            }).Start();
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            Hosts.Add(current);
            LoadURL();
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            Hosts.Add(current);
            current = null;
            chromeBrowser.Load("http://lordkian.ir/");
            button3.Click -= Button3_Click;
            button3.Click += Button3_Click2;
            button3.Text = "Resume";
        }
        private void Button3_Click2(object sender, EventArgs e)
        {
            LoadURL();
            button3.Click -= Button3_Click2;
            button3.Click += Button3_Click;
            button3.Text = "Pause";
        }
        private void Button4_Click(object sender, EventArgs e)
        {
            chromeBrowser.Refresh();
        }
    }
}
