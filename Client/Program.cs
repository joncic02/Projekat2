using System;
using System.Net.Http;
using System.Threading;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<string> keywords = new List<string>
                {
                    "dozvola",
                    "baza",
                    "pokusaj",
                    "proba",
                    "baza"
                };

            List<Task> tasks = new List<Task>();

            foreach (string keyword in keywords)
            {
                tasks.Add(Task.Run(() => SendRequest(keyword)));
            }
            await Task.WhenAll(tasks);
        }

        private static async Task SendRequest(string keyword)
        {
            string url = $"http://localhost:5050/{keyword}";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Odgovor servera na zahtev sa kljucnom reci '{keyword}':");
                    Console.WriteLine(responseBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greska prilikom slanja zahteva za fajlovima sa kljucnom reci '{keyword}': {ex.Message}");
                }
            }
        }
    }
}