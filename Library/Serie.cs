using System;
using System.Collections.Generic;
using System.Text;

namespace Library
{
    public class Serie
    {
        public List<Staffel> Staffeln { get; private set; } = new List<Staffel>();
        public string URL { get; set; }
        public string jahr { get; set; }
        public string Info { get; set; }
        public string Name { get; set; }
        public string Status()
        {
            if (Staffeln.Count == 0)
                return "Ready";
            int folgenCount = 0, staffelnCount = 0, finishedFolgen = 0;
            foreach (var item in Staffeln)
            {
                folgenCount += item.Folgen.Count;
                if (item.Folgen.Count > 0)
                    staffelnCount++;
                finishedFolgen += item.Status();
            }
            return $"{Name} ({jahr}): {staffelnCount}/{Staffeln.Count} {finishedFolgen}/{folgenCount}";
        }
    }
}
