using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSBKW.LRUCache
{
    public class LRUCacheLL
    {
        private DoublyLinkedList list;
        private Dictionary<string, Node> hash;
        private int capacity;
        //private static readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private static readonly SemaphoreSlim cacheLock = new SemaphoreSlim(1, 1);
        private int count;

        public LRUCacheLL(int capacity)
        {
            this.capacity = capacity;
            this.list = new DoublyLinkedList();
            this.hash = new Dictionary<string, Node>(); // Inicijalizacija recnika uz pomoc konstruktora
            this.count = 0;
        }


        //SemaphoreSlim umesto ReaderWriterLockSlim, jer SemaphoreSlim podržava asinhrono čekanje.
        //koristimo SemaphoreSlim sa maksimalnim brojem konkurentnih pristupa postavljenim na 1, 
        //što znači da samo jedan thread može da pristupi cache-u u jednom trenutku.
        //WaitAsync metoda se koristi da se asinhrono čeka da se lock oslobodi, a Release metoda se koristi da se oslobodi lock kada je pristup cache-u završen.
        public async Task<string> GetAsync(string key)
        {
            await cacheLock.WaitAsync();
            try
            {
                Node node;
                if (hash.TryGetValue(key, out node))
                {
                    list.MoveToFront(node);
                    return node.Value;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            finally 
            { 
                cacheLock.Release(); 
            }
        }

        public async Task PutAsync(string key, string value)
        {
            await cacheLock.WaitAsync();
            try
            {
                Node node;
                if (hash.TryGetValue(key, out node))
                {
                    node.Value = value;
                    list.MoveToFront(node);
                }
                else
                {
                    if (count >= capacity)
                    {
                        Node tail = list.Tail;
                        list.Remove(tail);
                        hash.Remove(tail.Key);
                        count--;
                    }
                    node = new Node(key, value);
                    list.AddToFront(node);
                    hash.Add(key, node);
                    count++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;

            }
            finally { cacheLock.Release(); }
        }

        //Ako je Cache veliki, brisanje elemenata može potrajati, pa je korisno da i ova operacija bude asinhrona.
        public async Task RemoveAsync(string key)
        {
            await cacheLock.WaitAsync();
            try
            {
                Node node;
                if (hash.TryGetValue(key, out node))
                {
                    list.Remove(node);
                    hash.Remove(key);
                    count--;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                cacheLock.Release();
            }
        }

        //I/O operacija koja moze potencijalno blokirati izvrsenje programa, pa je korisno da i ova operacija bude asinhrona.
        public async Task WriteOutCatcheToConsoleAsync()
        {
            await cacheLock.WaitAsync();
            try
            {
                Node node = list.Head;
                Console.WriteLine("Trenutni sadrzaj Cache memorije: ");

                do
                {
                    Console.WriteLine($"Keyword: '{node.Key}' - Cached Response: {node.Value}");
                    node = node.Next;
                } while (node != list.Head);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                cacheLock.Release();
            }
        }
    }
}
