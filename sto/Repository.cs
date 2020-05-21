using sto.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sto
{
    public class Repository
    {
        List<RepositoryLine> RepositoryLines = new List<RepositoryLine>();
        public List<RepositoryLine> AllLines { get { return RepositoryLines.ToList(); } }
        public void Add(Staffel staffel, params Host[] hosts)
        {
            foreach (var item in RepositoryLines)
                if (item.Staffel == staffel)
                {
                    item.Hosts.AddRange(hosts);
                    return;
                }
            var rl = new RepositoryLine() { Staffel = staffel };
            rl.Hosts.AddRange(hosts);
            RepositoryLines.Add(rl);
        }
        public void Add(Staffel staffel)
        {
            RepositoryLines.Add(new RepositoryLine() { Staffel = staffel });
        }
        public bool IstFertig(Serie serie)
        {
            foreach (var item in RepositoryLines)
                if (item.Hosts.Count > 0 && item.Staffel.Serie == serie)
                    return false;
            return true;
        }
        public void Remove(Staffel staffel)
        {
            foreach (var item in RepositoryLines.ToArray())
                if (item.Staffel == staffel)
                {
                    RepositoryLines.Remove(item);
                    return;
                }
        }
        public void Remove(Serie serie)
        {
            foreach (var item in RepositoryLines.ToArray())
                if (item.Staffel.Serie == serie)
                    RepositoryLines.Remove(item);
        }
    }
    public class RepositoryLine
    {
        public Staffel Staffel { get; set; }
        public List<Host> Hosts { get; set; } = new List<Host>();
        public override string ToString()
        {
            if (Staffel.Serie != null)
                return $"{Staffel.Serie.Name}: {Staffel.Name} ({Hosts.Count})";
            else
                return $"{Staffel.Name} ({Hosts.Count})";
        }
    }
}
