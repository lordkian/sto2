using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;
using System.Runtime.Serialization;

namespace sto.DataObjects
{
    [DataContract]
    public abstract class Vater
    {
        private static Regex URLRegex = new Regex(@"(\b(https?|ftp|file)://)?[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
        private static Regex ReletiveURLregex = new Regex(@"^\/");
        public static string[] ListXpath { get; set; }
        public static string[] SingleXpath { get; set; }
        public string HTMLData { get; set; }
        public List<string> SingleResult { get; set; }
        public List<List<string>> ListResult { get; set; }
        public bool UnderOperation { get; set; } = false;
        public abstract List<Vater> children { get; }
        [DataMember]
        public bool Enable { get; set; } = true;
        [DataMember]
        public string Name { get; set; }
        private string url;
        [DataMember]
        public string URL
        {
            get { return url; }
            set
            {
                if (ReletiveURLregex.Match(value).Success) url = @"https://s.to" + value;
                else if (URLRegex.Match(value).Success) url = value;
            }
        }
        public Vater(string url)
        {
            if (ReletiveURLregex.Match(url).Success) this.url = @"https://s.to" + url;
            else if (URLRegex.Match(url).Success) this.url = url;
        }
        public abstract void ParsData();
        public void CleanCache()
        {
            HTMLData = null;
            ListResult = null;
            SingleResult = null;
        }
        public abstract string[] GetListXpath();
        public abstract string[] GetSingleXpath();
        public abstract List<Host> GetPreferdHosts();
        #region Grouped Nodes

        /// <summary>
        /// You can get all Ordered data you want , from web page.
        /// Each row is an Item.
        /// Each column is property of that item. 
        /// </summary>
        /// <param name="URL">URL of web page</param>
        /// <param name="XPathes">XPath patterns of Columns</param>
        /// <returns>Grouped List</returns>
        public static List<List<string>> GroupedNodes(string URL, params string[] XPathes)
        {
            //Load HTML Source
            HtmlAgilityPack.HtmlDocument doc;

            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(client.DownloadString(URL));
            }


            //Declear Variables
            List<List<string>> firstDraft = new List<List<string>>();
            int rowsCount = XPathes.Length;
            HtmlNodeCollection nodes;


            //First Draft
            for (int i = 0; i < rowsCount; i++)
            {
                firstDraft.Add(new List<string>());
                Match m = Regex.Match(XPathes[i], @"/@\w+");
                nodes = doc.DocumentNode.SelectNodes(XPathes[i]);

                if (m.Success == false)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        firstDraft[i].Add(node.InnerText.Trim());
                    }
                }
                else
                {
                    string attribute = m.Value.Substring(2);
                    foreach (HtmlNode node in nodes)
                    {
                        firstDraft[i].Add(node.GetAttributeValue(attribute, "Attribute not found !"));
                    }
                }

            }

            //add by kian
            int max = 0;
            foreach (List<string> item in firstDraft)
                if (item.Count > max)
                    max = item.Count;
            foreach (List<string> item in firstDraft)
                while (max > item.Count)
                    item.Add("");

            //Transpose
            List<List<string>> grouped = new List<List<string>>();
            for (int i = 0; i < firstDraft[0].Count; i++)
            {
                grouped.Add(new List<string>());
                for (int j = 0; j < firstDraft.Count; j++)
                {
                    grouped[i].Add(firstDraft[j][i]);
                }
            }


            // Return
            return grouped;
        }

        #endregion Grouped Nodes
    }
}
