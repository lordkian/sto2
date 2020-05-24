using HtmlAgilityPack;
using Library;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        public Form1()
        {
            InitializeComponent();
            ServicePointManager.DefaultConnectionLimit = 16;
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            var strs = textBox2.Text.Replace("\r", "").Split('\n');
            textBox2.Text = "";
            textBox1.Enabled = false;
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
            }).ContinueWith((t) => { Unblock(); });

        }
        private void Unblock()
        {
            if (InvokeRequired)
                Invoke(new deleg(Unblock));
            else
            {
                textBox1.Enabled = true;
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
        }
        private void FillFolge(Folge folge)
        {
            var html = LoadData(folge.URL);
            var res = ParsDataFromHTML(html, "//li[@data-lang-key='1']//a[@class='watchEpisode' and @itemprop='url' and @target='_blank']/@href",
                "//li[@data-lang-key='1']//a[@class='watchEpisode' and @itemprop='url' and @target='_blank']/h4");
            foreach (var item in res)
                folge.Hosts.Add(new Host() { URL = "https://s.to" + item[0], HostName = item[1] });
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
    }
}
