using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace sto.DataObjects
{
    [DataContract]
    public class Staffel : Vater
    {
        [DataMember]
        public readonly List<Folge> folgen = new List<Folge>();
        [DataMember]
        public string Title { get; set; }
        public static new string[] ListXpath { get; set; }
        public static new string[] SingleXpath { get; set; }
        public override List<Vater> children { get { return folgen.Cast<Vater>().ToList(); } }
        [DataMember]
        public string Number { get; set; }
        public Serie Serie { get; set; }
        public Staffel(string url) : base(url) { }
        public override string ToString()
        {
            return $"{Title} : {Number} \"{URL}\"";
        }
        public void GetFolgen()
        {
            var res = from i in GroupedNodes(URL, ListXpath) where i[1].Contains("Episode") select i;
            Writer($"found {res.Count()} folgen at schafel{Number}");
            foreach (var item in res)
                folgen.Add(new Folge(item[2]) { Number = item[0], Title = item[1], Writer = Writer, Staffel = this });
        }

        public override void ParsData()
        {
            var res = from i in ListResult where i[1].Contains("Episode") select i;
            Writer($"{res.Count()} folgen wurden im schafel{Number}:{Title} gefunden");
            foreach (var item in res)
                folgen.Add(new Folge(item[2]) { Number = item[0], Title = item[1], Writer = Writer, Staffel = this });
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
            foreach (var item in folgen)
                res.AddRange(item.GetPreferdHosts());
            return res;
        }
    }
}
