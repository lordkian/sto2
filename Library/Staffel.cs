using System;
using System.Collections.Generic;
using System.Text;

namespace Library
{
    public class Staffel
    {
        public List<Folge> Folgen { get; private set; } = new List<Folge>();
        public string URL { get; set; }
        public string Number { get; set; }
    }
}
