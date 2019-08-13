using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sto
{
    public class BiDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();
        public ICollection<TSecond> Values { get { return secondToFirst.Keys; } }
        public ICollection<TFirst> Keys { get { return firstToSecond.Keys; } }
        public void Add(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) ||
                secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("Duplicate first or second");
            }
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }
        public TFirst this[TSecond second]
        {
            get
            {
                return secondToFirst[second];
            }
            set
            {
                secondToFirst[second] = value;
            }
        }
        public TSecond this[TFirst first]
        {
            get
            {
                return firstToSecond[first];
            }
            set
            {
                firstToSecond[first] = value;
            }
        }
    }
}
