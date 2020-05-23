using System;
using System.Collections.Generic;
using System.Text;

namespace Library
{
    public class Folge
    {
        public List<Host> Hosts { get; private set; } = new List<Host>();
        public string URL { get; set; }
        public string Number { get; set; }
    }
}
