using ProjectSBKW.LRUCache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSBKW.Services
{
    public static class Handler
    {
        
        public static async Task HandleRequestAsync(object contextObj, LRUCacheLL cache)
        {
            HttpListenerContext context = (HttpListenerContext)contextObj;

            string requestUrl = context.Request.Url.AbsolutePath;

            // Čitanje ključne reči iz URL-a
            string keyword = context.Request.Url.AbsolutePath.TrimStart('/');

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // Pocinjemo sa merenjem vremena

            string rootDirectory = Directory.GetCurrentDirectory();// Putanja do root direktorijuma

            //Asinhrono pretrazivanje fajlova
            //Citanje iz file sistema je operacija koja može biti spora, pa je bolje da se izvršava asinhrono
            //Task.Run metoda se koristi za pokretanje sinhronih operacija u asinhronom okruženju koristeci ThreadPool, oslobadjajuci glavni thread da nastavi sa radom
            string[] files = await Task.Run(() => Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories)); // Pretraga svih fajlova u root direktorijumu

            // Kreiranje liste matchingFiles van bloka koda zahteva
            List<string> matchingFiles = new List<string>();

            // Iteracija kroz sve fajlove i dodavanje odgovarajucih fajlova u listu
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string fileExt = Path.GetExtension(file);

                if (fileName.Contains(keyword))
                {
                    string fileLink = $"{fileName}{fileExt}";
                    matchingFiles.Add(fileLink);

                }
            }

            try
            {
                //Provera da li postoji vec zahtev sa kljucnom reci u cache-u
                //Razlog zasto koristimo asinhrone metode je taj sto koristi SemaphoreSlim koji obezbedjuje da samo jedan thread moze da pristupi cache-u u jednom trenutku
                string cacheRezultat = await cache.GetAsync(keyword);

                if (cacheRezultat != null) //Slanje odgovora iz cache-a i prikaz trenutnog sadrzaja
                {

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(cacheRezultat);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();
                    Console.WriteLine($"Fajl sa kljucnom reci '{keyword}' je procitan iz Cache-a.");

                    // Ispisivanje trenutnog sadrzaj Cache-a
                    cache.WriteOutCatcheToConsoleAsync();
                }
                else // Ako je cache prazan, dodaj trenutni zahtev u cache memoriju i prikazi mu trenutni sadrzaj
                {
                    List<string> filteredFiles = matchingFiles.Where(fileName => fileName.Contains(keyword)).ToList();
                    if (filteredFiles.Count > 0)
                    {
                        // Kreiranje HTML-a koji sadrži linkove ka pronađenim fajlovima
                        StringBuilder htmlBuilder = new StringBuilder();
                        htmlBuilder.Append("<html><body><ul>");
                        foreach (string file in filteredFiles)
                        {
                            htmlBuilder.Append($"<li><a href='http://localhost:5050/{file}'>{file}</a></li>");
                        }
                        htmlBuilder.Append("</ul></body></html>");

                        string htmlResponse = htmlBuilder.ToString();

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(htmlResponse);
                        context.Response.ContentType = "text/html"; // Postavljanje ContentType-a na HTML
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();

                        // Dodavanje odgovora u keš memoriju
                        //PutAsync metoda biti pozvana asinhrono, što znači da će ostatak koda nastaviti sa izvršavanjem dok se PutAsync metoda ne završi.
                        await cache.PutAsync(keyword, htmlResponse);
                        Console.WriteLine($"Fajlovi sa ključnom reči '{keyword}' su pronađeni i poslati kao odgovor. Dodati u Cache!");

                        // Ispisivanje trenutnog sadrzaj Cache-a
                        cache.WriteOutCatcheToConsoleAsync();
                    }
                    else
                    {
                        // Ako nema pronađenih fajlova, šaljemo odgovor da nema rezultata
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        string responseMessage = $"Nema fajlova sa kljucnom reci '{keyword}'.";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                        Console.WriteLine(responseMessage);
                    }
                }
                stopwatch.Stop(); // Prekidamo merenje vremena izvrsenja i ispisujemo ga
                Console.WriteLine($"Vreme potrebno za izvrsenje je: {stopwatch.Elapsed}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Greska pri obradi zahteva: {e.Message}");
            }
        }
    }
}
