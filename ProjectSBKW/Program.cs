using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProjectSBKW.LRUCache;
using ProjectSBKW.Services;

namespace ProjectSBKW
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string url = "http://localhost:5050/";

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Server running at " + url);

            LRUCacheLL lru = new LRUCacheLL(5); // Poziv konstruktora kesa

            while(true)
            {
                HttpListenerContext context = listener.GetContext();

                if(context.Request.Url != null)
                {
                    if (context.Request.Url.AbsolutePath != "/favicon.ico")

                        await Handler.HandleRequestAsync(context, lru); //await dozvoljava serveru da nastavi da osluskuje druge zahteve dok se trenutni zahtev obradjuje

                        //asinhrona obrada HandleRequestAsync dozvoljava trenutnoj niti da se vrati u ThreadPool gde se moze koristiti za obavljanje nekog drugog posla
                }
            }
        }
    }
}