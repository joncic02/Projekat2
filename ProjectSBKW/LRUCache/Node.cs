using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSBKW.LRUCache
{
    public class Node
    {
        public string Key;
        public string Value;
        public Node Next;
        public Node Prev;

        public Node(string key, string val)
        {
            this.Key = key;
            this.Value = val;
            this.Next = null;
            this.Prev = null;
        }

    }
}
