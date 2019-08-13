using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace sto.DataObjects
{
    [DataContract]
    public class Folge : Vater
    {
        [DataMember]
        public readonly List<Host> Hosts = new List<Host>();
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Number { get; set; }
        public static new string[] ListXpath { get; set; }
        public static new string[] SingleXpath { get; set; }
        public override List<Vater> children { get { return Hosts.Cast<Vater>().ToList(); } }
        public bool HastHost
        {
            get
            {
                var b = false;
                foreach (var item in Hosts)
                    b |= item.HastHostURL;
                return b;
            }
        }
        public Staffel Staffel { get; set; }
        public Folge(string url) : base(url) { }
        public override string ToString()
        {
            return $"{Title} : {Number} \"{URL}\"";
        }
        public void GetInfo()
        {
            var res1 = GroupedNodes(URL, SingleXpath)[0][0];
            Name = Title.Replace("Episode", "Folge") + " " + res1;
            var res = GroupedNodes(URL, ListXpath);
            Writer($"found {res.Count} host at schafel{Staffel.Number} folge{Number}");
            foreach (var item in res)
                Hosts.Add(new Host(item[1]) { Name = item[0] });
        }

        public override void ParsData()
        {
            Name = Title.Replace("Episode", "Folge") + " " + SingleResult[0] + ".mp4";

            Writer($"{ListResult.Count} hosts wurden im schafel{Staffel.Number}:{Staffel.Title} folge{Number}:{Name} gefunden");
            foreach (var item in (from i in ListResult where i[1] != null && i[1].Count() > 0 select i))
                Hosts.Add(new Host(item[1]) { Name = item[0], HostPriority = HostPriority.GetByName(item[0]) });
        }
        public override string[] GetListXpath()
        {
            return ListXpath;
        }

        public override string[] GetSingleXpath()
        {
            return SingleXpath;
        }

        public override List<Host> GetPreferdHosts()
        {
            var res = new List<Host>();
            if (!Enable)
                return res;
            res.AddRange(from i in Hosts where i.Enable orderby i select i);
            if (res.Count > Main.KeepingHost)
                res.RemoveRange(Main.KeepingHost, res.Count - Main.KeepingHost);
            return res;
        }
    }
}
