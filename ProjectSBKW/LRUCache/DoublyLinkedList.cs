using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSBKW.LRUCache
{
    public class DoublyLinkedList
    {
        public Node Head;
        public Node Tail;

        public void AddToFront(Node node)
        {
            if (Head == null)
            {
                Head = node;
                Tail = node;
                node.Next = node; // Zato sto je cirkularna pokazuje sama na sebe
                node.Prev = node;
            }
            else
            {
                node.Next = Head; // Za sledecu postavlja sta je bio head pre
                node.Prev = Tail; // Za prethodnu postavlja tail
                Head.Prev = node;
                Tail.Next = node;
                Head = node;
            }
        }

        public void MoveToFront(Node node)
        {
            if (node == Head)
            {
                return;
            }
            if (node == Tail)
            {
                Tail = Tail.Prev;
            }
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            AddToFront(node);
        }

        public Node Find(string key)
        {
            Node node = Head;
            do
            {
                if (node.Key == key)
                {
                    return node;
                }
                node = node.Next;
            } while (node != Head);
            return null;
        }
        public void Remove(Node node)
        {
            if (node == Head)
            {
                Head = Head.Next;
                if (Head != null)
                {
                    Head.Prev = Tail;
                }
            }
            else if (node == Tail)
            {
                Tail = Tail.Prev;
                if (Tail != null)
                {
                    Tail.Next = Head;
                }
            }
            else
            {
                node.Prev.Next = node.Next;
                node.Next.Prev = node.Prev;
            }
        }

    }
}
