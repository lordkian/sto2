using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace sto
{
    [DataContract]
    public class HostPriority : IComparable<HostPriority>
    {
        [DataMember]
        public string HostName { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public string SiteAddr { get; set; }
        [DataMember]
        public bool Enable { get; set; }
        public string ListViewItem { get { try { return ToString(); } catch (NullReferenceException) { return ""; } } }
        public static readonly Dictionary<int, HostPriority> PriorityToHostPriority = new Dictionary<int, HostPriority>();
        public static readonly Dictionary<string, HostPriority> HostNameToHostPriority = new Dictionary<string, HostPriority>();
        public static readonly List<string> HostNames = new List<string>();
        public static readonly List<string> SiteAddrs = new List<string>();
        public static int MaxPriority = 0;
        public int CompareTo(HostPriority other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            return Priority.CompareTo(other.Priority);
        }
        public static void Import(List<HostPriority> hostPriorities)
        {
            PriorityToHostPriority.Clear();
            HostNameToHostPriority.Clear();
            HostNames.Clear();
            SiteAddrs.Clear();
            foreach (var item in hostPriorities)
            {
                PriorityToHostPriority.Add(item.Priority, item);
                HostNameToHostPriority.Add(item.HostName, item);
                HostNames.Add(item.HostName);
                SiteAddrs.Add(item.SiteAddr);
                MaxPriority = Math.Max(MaxPriority, item.Priority);
            }
        }
        public static HostPriority GetByName(string hostName)
        {
            if (HostNames.Contains(hostName))
                return HostNameToHostPriority[hostName];
            else
            {
                MaxPriority++;
                var hp = new HostPriority() { HostName = hostName, Priority = MaxPriority, SiteAddr = "" };
                PriorityToHostPriority.Add(hp.Priority, hp);
                HostNameToHostPriority.Add(hp.HostName, hp);
                HostNames.Add(hp.HostName);
                SiteAddrs.Add(hp.SiteAddr);
                return hp;
            }
        }
        public override string ToString()
        {
            return $"{Priority}:{HostName}";
        }
    }
}
