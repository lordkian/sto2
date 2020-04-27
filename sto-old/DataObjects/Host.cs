using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace sto.DataObjects
{
    [DataContract]
    public class Host : Vater, IComparable<Host>
    {
        [DataMember]
        public string HostURL { get; set; }
        public Host(string url) : base(url) { }
        public HostPriority HostPriority { get; set; }
        public bool HastHostURL { get { return (HostURL != null && HostURL.Length > 0); } }
        public override List<Vater> children { get { return null; } }
        public static new string[] ListXpath { get; set; }
        public static new string[] SingleXpath { get; set; }
        public new bool Enable
        {
            get { return HostPriority.Enable; }
            set { HostPriority.Enable = value; }
        }
        public override void ParsData() { }
        public override string ToString()
        {
            return Name + " : " + URL;
        }
        public override string[] GetListXpath()
        {
            return ListXpath;
        }
        public override string[] GetSingleXpath()
        {
            return SingleXpath;
        }
        public void SetHostPriority()
        {
            if (HostPriority == null)
                HostPriority = HostPriority.GetByName(Name);
        }
        public int CompareTo(Host other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return HostPriority.Priority.CompareTo(other.HostPriority.Priority);
        }
        public override List<Host> GetPreferdHosts()
        {
            throw new NotImplementedException();
        }
    }
}
