using HtmlAgilityPack;
using sto.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace sto
{
    public delegate void VoidFunc(Vater vater);
    public class Process
    {
        public Vater Vater { get; set; }
        public VoidFunc VoidFunc { get; set; }
        // public static EventHandler onFinished { get; set; }
        public static int DownloadThreadNumber { get; set; } = 7;
        public static int ProcessThreadNumber { get; set; } = 7;
        private static List<Thread> DownloadThreads = new List<Thread>();
        private static List<Thread> ProcessThreads = new List<Thread>();
        private static List<Process> DownloadQueue = new List<Process>();
        private static List<Process> ProcessQueue = new List<Process>();
        public Process(Vater vater)
        {
            Vater = vater;
        }
        public static Process Start(Vater vater, VoidFunc voidFunc)
        {
            if (vater is Host)
                return null;
            var p = new Process(vater) { VoidFunc = voidFunc };
            p.Start();
            return p;
        }
        public void Start()
        {
            DownloadQueue.Add(this);
            if (DownloadThreads.Count < DownloadThreadNumber)
            {
                var t = new Thread(DownloadMethod);
                DownloadThreads.Add(t);
                t.Start();
            }
        }
        private void StartProcessing()
        {
            ProcessQueue.Add(this);
            if (ProcessThreads.Count < ProcessThreadNumber)
            {
                var t = new Thread(ProcessMethod);
                ProcessThreads.Add(t);
                t.Start();
            }
        }
        private static void DownloadMethod()
        {
            while (true)
            {
                Process process = null;

                lock (DownloadQueue)
                {
                    if (DownloadQueue.Count > 0)
                    {
                        process = DownloadQueue.First();
                        DownloadQueue.Remove(process);
                    }
                }

                if (process == null)
                {
                    DownloadThreads.Remove(Thread.CurrentThread);
                    return;
                }
                var client = new WebClient();
                client.Encoding = Encoding.UTF8;
                process.Vater.HTMLData = client.DownloadString(process.Vater.URL);
                process.StartProcessing();
                if (DownloadThreads.Count > DownloadThreadNumber)
                {
                    DownloadThreads.Remove(Thread.CurrentThread);
                    return;
                }
            }
        }
        private static void ProcessMethod()
        {
            while (true)
            {
                Process process = null;

                lock (ProcessQueue)
                {
                    if (ProcessQueue.Count > 0)
                    {
                        process = ProcessQueue.First();
                        ProcessQueue.Remove(process);
                    }
                }

                if (process == null)
                {
                    ProcessThreads.Remove(Thread.CurrentThread);
                    return;
                }

                try
                {
                    if (process.Vater.GetSingleXpath() != null)
                        process.Vater.SingleResult = GroupedNodes(process.Vater.HTMLData, process.Vater.GetSingleXpath())[0];
                    if (process.Vater.GetListXpath() != null)
                        process.Vater.ListResult = GroupedNodes(process.Vater.HTMLData, process.Vater.GetListXpath());
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Test");
                }
                process.Vater.ParsData();
                process.Vater.CleanCache();
                process.VoidFunc(process.Vater);

                if (ProcessThreads.Count > ProcessThreadNumber)
                {
                    ProcessThreads.Remove(Thread.CurrentThread);
                    return;
                }
            }
        }

        public static void End()
        {
            foreach (var item in ProcessThreads)
                item.Abort();
            foreach (var item in DownloadThreads)
                item.Abort();
            ProcessThreadNumber = 0;
            DownloadThreadNumber = 0;
        }
        #region Grouped Nodes

        /// <summary>
        /// You can get all Ordered data you want , from web page.
        /// Each row is an Item.
        /// Each column is property of that item. 
        /// </summary>
        /// <param name="URL">URL of web page</param>
        /// <param name="XPathes">XPath patterns of Columns</param>
        /// <returns>Grouped List</returns>
        public static List<List<string>> GroupedNodes(string HTML, params string[] XPathes)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(HTML);
            var firstDraft = new List<List<string>>();
            int rowsCount = XPathes.Length;
            HtmlNodeCollection nodes = null;
            var regex = new Regex(@"/@\w+$");

            for (int i = 0; i < rowsCount; i++)
            {
                var list = new List<string>();
                var m = regex.Match(XPathes[i]);
                nodes = doc.DocumentNode.SelectNodes(XPathes[i]);
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

        #endregion Grouped Nodes
    }
}
