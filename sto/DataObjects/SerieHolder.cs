using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace sto.DataObjects
{
    public enum Stat { Null, Loading, Ready, OnCaptcha, Done }
    public class SerieHolder
    {
        Main main;
        public Serie serie { get; private set; }
        public string URL { get; private set; }
        public string Path { get; set; }
        public string WorkinDir { get; set; }
        public Stat WorkingStat { get; set; } = Stat.Null;
        private int stafelnNunm = 1;
        private int stafeln = 0;
        private int folgen = 0;
        private int folgenNum = 1;
        private List<Host> PreferdHosts = new List<Host>(); // to remove
        private List<Host> hosts = new List<Host>(); // to remove
        private List<Folge> allfolgen = new List<Folge>();
        public SerieHolder(Main main, Serie serie, string addr) { this.main = main; this.serie = serie; URL = addr; }
        public SerieHolder(Main main, string URL) { this.main = main; this.URL = URL; WorkingStat = Stat.Null; }
        public override string ToString()
        {
            switch (WorkingStat)
            {
                case Stat.Null:
                    return URL;
                case Stat.Loading:
                    return $"{serie.Name}: Stafeln:{stafeln}/{stafelnNunm} Folgen:{folgen}/{folgenNum}";
                case Stat.Ready:
                    return $"{serie.Name}: {(from i in PreferdHosts where i.HastHostURL select i).Count()}/{PreferdHosts.Count}";
                case Stat.OnCaptcha:
                    return $"{serie.Name}: {(from i in PreferdHosts where i.HastHostURL select i).Count()}/{PreferdHosts.Count}";
                case Stat.Done:
                    return "Done";
                default:
                    return "";
            }
        }
        public static SerieHolder Load(string path, string workingDir)
        {
            var name = new DirectoryInfo(path).Name;
            StreamReader sr;
            if (File.Exists(path + "\\" + name + ".full.json"))
                sr = new StreamReader(path + "\\" + name + ".full.json");
            else if (File.Exists(path + "\\tmp_new.json"))
                sr = new StreamReader(path + "\\tmp_new.json");
            else if (File.Exists(path + "\\" + name + ".json"))
                sr = new StreamReader(path + "\\" + name + ".json");
            else
                throw new Exception("cannot find sutable json");

            var obj = JsonConvert.DeserializeObject(sr.ReadToEnd(), typeof(Serie));
            sr.Close();
            if (obj is Serie)
            {
                var res = new SerieHolder(obj as Serie, (obj as Serie).URL);
                res.WorkingStat = Stat.Ready;
                res.Path = path;
                res.Prepare();
                return res;
            }
            else
                throw new Exception("bad json");
        }
        public void Save()
        {
            lock (serie)
            {
                if (!File.Exists(Path))
                    Directory.CreateDirectory(Path);
                StreamWriter sw;
                if (!File.Exists(Path + "\\" + serie.Name + ".json"))
                {
                    sw = new StreamWriter(Path + "\\" + serie.Name + ".json");
                    sw.WriteLine(JsonConvert.SerializeObject(serie));
                    sw.Close();
                    mkdirs();
                }
                else
                {
                    if (File.Exists(Path + "\\tmp_old.json"))
                        File.Delete(Path + "\\tmp_old.json");
                    if (File.Exists(Path + "\\tmp_new.json"))
                        File.Move(Path + "\\tmp_new.json", Path + "\\tmp_old.json");
                    sw = new StreamWriter(Path + "\\tmp_new.json");
                    sw.WriteLine(JsonConvert.SerializeObject(serie));
                    sw.Close();

                    if (WorkingStat == Stat.OnCaptcha &&
                        PreferdHosts.Count == (from i in PreferdHosts where i.HastHostURL select i).Count())
                    {
                        WorkingStat = Stat.Done;
                        sw = new StreamWriter(Path + "\\" + serie.Name + ".full.json");
                        sw.WriteLine(JsonConvert.SerializeObject(serie, Formatting.Indented));
                        sw.Close();
                        sw = new StreamWriter(Path + "\\" + serie.Name + ".link.json");
                        var list = new List<List<string>>();
                        foreach (var item in allfolgen)
                        {
                            var row = (from i in item.Hosts where i.HastHostURL select i.HostURL).ToList();
                            row.Insert(0, item.Name);
                            list.Add(row);
                        }
                        sw.WriteLine(JsonConvert.SerializeObject(list, Formatting.Indented));
                        sw.Close();
                    }
                }
            }
        }
        private void SerieFinished(Vater vater)
        {
            serie.HTMLData = "";
            serie.ListResult = null;
            serie.SingleResult = null;
            Path = WorkinDir + "\\" + serie.Name;
            stafelnNunm = serie.Staffeln.Count;
            WorkingStat = Stat.Loading;

            var p = new List<Process>();
            foreach (var item in serie.Staffeln)
                p.Add(new Process(item) { VoidFunc = StaffelFinished });
            p[0].VoidFunc += (v) => { folgenNum--; };

            foreach (var item in p)
                item.Start();
        }
        private void StaffelFinished(Vater vater)
        {
            foreach (var item in vater.children)
                Process.Start(item, FolgenFinished);

            folgenNum += (vater as Staffel).folgen.Count;
            stafeln++;

        }
        private void FolgenFinished(Vater vater)
        {
            foreach (var item in vater.children)
                Process.Start(item, (vv) => { });
            folgen++;
            if (folgen == folgenNum)
            {
                Save();
                Prepare();
            }
        }
        public void FillHostPriorities()
        {
            foreach (var item in serie.Staffeln)
                foreach (var item2 in item.folgen)
                    foreach (var item3 in item2.Hosts)
                        item3.SetHostPriority();
        }
        private void mkdirs()
        {
            if (!Directory.Exists(Path + "\\" + serie.Staffeln[0].Title))
                foreach (var item in serie.Staffeln)
                    Directory.CreateDirectory(Path + "\\" + item.Title);
        }
        public void Prepare()
        {
            FillHostPriorities();
            foreach (var item in serie.Staffeln)
                allfolgen.AddRange(item.folgen);
            RefeshHosts();
            if (PreferdHosts.Count == 0)
                WorkingStat = Stat.Done;
            else
                WorkingStat = Stat.Ready;
        }
        public void RefeshHosts()
        {
            Main.Repo.Remove(serie);
            foreach (var item in serie.Staffeln)
            {
                Main.Repo.Add(item, item.GetPreferdHosts().ToArray());
                if (item.Serie == null)
                    item.Serie = serie;
            }


            hosts.Clear();
            PreferdHosts.Clear();
            hosts.AddRange(serie.GetPreferdHosts());
            PreferdHosts.AddRange(from i in hosts where !i.HastHostURL select i);
        }
        public void StartCaptcha()
        {
            if (WorkingStat == Stat.Ready)
            {
                Captcha.AddHosts(PreferdHosts);
                WorkingStat = Stat.OnCaptcha;
            }
        }
        public void Start()
        {
            if (serie == null)
            {
                serie = new Serie(URL);
                Process.Start(serie, SerieFinished);
            }
        }

    }
}
