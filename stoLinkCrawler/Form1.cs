using HtmlAgilityPack;
using Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stoLinkCrawler
{
    public partial class Form1 : Form
    {
        private delegate void deleg();
        List<Serie> series = new List<Serie>();
        bool isModified = false;
        private object lockObj1 = new object();
        public Form1()
        {
            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = 16;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var d = new FolderBrowserDialog();
            d.SelectedPath = label1.Text;
            var res = d.ShowDialog();
            if (res == DialogResult.OK)
                label1.Text = d.SelectedPath;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (label1.Text == "")
                button1_Click(sender, e);
            var strs = textBox2.Text.Replace("\r", "").Split('\n');
            textBox2.Text = "";
            textBox2.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;

            Task.Factory.StartNew(() =>
            {
                foreach (var item in strs)
                {
                    var s = new Serie() { URL = item };
                    series.Add(s);
                    Task.Factory.StartNew(() => { FillSerie(s); }, TaskCreationOptions.AttachedToParent);
                }
            }).ContinueWith((t) =>
            {
                Save();
                Unblock();
            });

        }
        private void Save()
        {
            foreach (var item in series)
            {
                Directory.CreateDirectory(label1.Text + "\\" + item.Name);
                StreamWriter streamWriter = new StreamWriter(label1.Text + "\\" + item.Name + "\\" + item.Name + ".json");
                streamWriter.Write(JsonConvert.SerializeObject(itemk, Formatting.Indented));
                streamWriter.Close();
            }
        }
        private void Unblock()
        {
            if (InvokeRequired)
                Invoke(new deleg(Unblock));
            else
            {
                textBox2.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }
        private void FillSerie(Serie serie)
        {
            var html = LoadData(serie.URL);
            var res1 = ParsDataFromHTML(html, "//h1[@itemprop='name']/span", "//span[@itemprop='startDate']/a", "//span[@itemprop='endDate']/a", "//p[@class='seri_des']");
            serie.Name = res1[0][0];
            serie.Info = res1[0][3];
            serie.jahr = res1[0][1] + " - " + res1[0][2];
            var res2 = ParsDataFromHTML(html, "//div[@id='stream']//li/a/@href", "//div[@id='stream']//li/a");
            foreach (var item in res2)
                if (!item[0].Contains("episode-"))
                {
                    var st = new Staffel() { URL = "https://s.to" + item[0], Number = item[1] };
                    serie.Staffeln.Add(st);
                    Task.Factory.StartNew(() => { FillStaffel(st); }, TaskCreationOptions.AttachedToParent);
                }
            lock (lockObj1)
            {
                isModified = true;
            }
        }
        private void FillStaffel(Staffel staffel)
        {
            var html = LoadData(staffel.URL);
            var res = ParsDataFromHTML(html, "//div[@id='stream']//li/a/@href", "//div[@id='stream']//li/a");
            foreach (var item in res)
                if (item[0].Contains("episode-"))
                {
                    var f = new Folge() { URL = "https://s.to" + item[0], Number = item[1] };
                    staffel.Folgen.Add(f);
                    Task.Factory.StartNew(() => { FillFolge(f); }, TaskCreationOptions.AttachedToParent);
                }
            lock (lockObj1)
            {
                isModified = true;
            }
        }
        private void FillFolge(Folge folge)
        {
            var html = LoadData(folge.URL);
            var res = ParsDataFromHTML(html, "//li[@data-lang-key='1']//a[@class='watchEpisode' and @itemprop='url' and @target='_blank']/@href",
                "//li[@data-lang-key='1']//a[@class='watchEpisode' and @itemprop='url' and @target='_blank']/h4");
            foreach (var item in res)
                folge.Hosts.Add(new Host() { URL = "https://s.to" + item[0], HostName = item[1] });
            lock (lockObj1)
            {
                isModified = true;
            }
        }
        public static string LoadData(string URL)
        {
            if (URL == null || URL.Length == 0)
                throw new Exception("URL Cannot be null or empty");
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            return client.DownloadString(URL);
        }
        private static List<List<string>> ParsDataFromHTML(string HTML, params string[] xPathes)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(HTML);
            var firstDraft = new List<List<string>>();
            int rowsCount = xPathes.Length;
            HtmlNodeCollection nodes = null;
            var regex = new Regex(@"/@\w+$");

            for (int i = 0; i < rowsCount; i++)
            {
                var list = new List<string>();
                var m = regex.Match(xPathes[i]);
                nodes = doc.DocumentNode.SelectNodes(xPathes[i]);
                if (nodes == null)
                    list.Add("");
                else
                {
                    if (!m.Success)
                        foreach (var node in nodes)
                            list.Add(node.InnerText.Trim());

                    else
                    {
                        string attribute = m.Value.Substring(2);
                        foreach (var node in nodes)
                            list.Add(node.GetAttributeValue(attribute, "Attribute not found !"));
                    }
                }
                firstDraft.Add(list);
            }

            //Transpose
            var grouped = new List<List<string>>();
            var len = firstDraft.First().Count;
            for (int i = 0; i < len; i++)
            {
                var list = new List<string>();
                foreach (var item in firstDraft)
                    if (item.Count > 0)
                    {
                        list.Add(item.First());
                        item.RemoveAt(0);
                    }
                    else
                        list.Add("");
                grouped.Add(list);
            }
            return grouped;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (lockObj1)
            {
                if (!isModified)
                    return;
                else
                    isModified = false;
            }
            listBox1.Items.Clear();
            foreach (var item in series)
            {
                listBox1.Items.Add(item.Status());
            }
        }
    }
}
