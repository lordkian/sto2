using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace sto.DataObjects
{
    [DataContract]
    public class Serie : Vater
    {
        [DataMember]
        public readonly List<Staffel> Staffeln = new List<Staffel>();
        public override List<Vater> children { get { return Staffeln.Cast<Vater>().ToList(); } }
        public static new string[] ListXpath { get; set; }
        public static new string[] SingleXpath { get; set; }
        public string Description { get; set; }
        public Serie(string url) : base(url) { }
        public void GetStaffeln()
        {
            var res = GroupedNodes(URL, SingleXpath)[0];
            Name = res[0];
            Description = res[1];
            var res1 = from i in GroupedNodes(URL, ListXpath) where i[1].Contains("Staffel") && !i[1].Contains("Episode") select i;
            foreach (var item in res1)
                Staffeln.Add(new Staffel(item[2]) { Number = item[0], Title = item[1], Serie = this });
        }
        public override void ParsData()
        {
            Name = SingleResult[0];
            Description = SingleResult[1];
            var res = from i in ListResult where i[1].Contains("Staffel") && !i[1].Contains("Episode") select i;
            foreach (var item in res)
                Staffeln.Add(new Staffel(item[2]) { Number = item[0], Title = item[1], Serie = this, Name = item[1] });
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
            foreach (var item in Staffeln)
                res.AddRange(item.GetPreferdHosts());
            return res;
        }
    }
}
