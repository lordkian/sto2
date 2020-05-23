using System;
using System.Collections.Generic;
using System.Text;

namespace Library
{
    public class Serie
    {
        public List<Staffel> Staffeln { get; private set; } = new List<Staffel>();
        public string URL { get; set; }
    }
}
