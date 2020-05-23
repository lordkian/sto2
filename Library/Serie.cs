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
    }
}
