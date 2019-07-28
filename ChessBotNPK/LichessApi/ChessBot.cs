using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChessBotNPK.LichessApi
{
    public static class ChessBot
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static void UpgradeToBotAccount(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.PostAsync("https://lichess.org/api/bot/account/upgrade", new FormUrlEncodedContent(new Dictionary<string, string>())).Wait();
        }

        public static async Task StreamIncomingEvents(Action<JObject> onEventReceived, CancellationToken cancellationToken, string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using (var response = await httpClient.GetAsync("https://lichess.org/api/stream/event", HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var wrappedStream = new StreamReader(stream))
            while(!cancellationToken.IsCancellationRequested)
                {
                    string data = wrappedStream.ReadLine();
                    if (data == "") continue;
                    onEventReceived(JObject.Parse(data));
                }
        }

        public static async Task StreamGameState(string gameId, Action<JObject> onEventReceived, CancellationToken cancellationToken, string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            using (var response = await httpClient.GetAsync($"https://lichess.org/api/bot/game/stream/{gameId}", HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var wrappedStream = new StreamReader(stream))
                while (!cancellationToken.IsCancellationRequested)
                {
                    string data = wrappedStream.ReadLine();
                    if (data == "") continue;
                    onEventReceived(JObject.Parse(data));
                }
        }

        public static void MakeMove(string gameId, string move, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            retry:
            try
            {
                httpClient.PostAsync($"https://lichess.org/api/bot/game/{gameId}/move/{move}", new FormUrlEncodedContent(new Dictionary<string, string>())).Wait();
            }
            catch
            {
                goto retry;
            }
        }

        public static void AbortAGame(string gameId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.PostAsync($"https://lichess.org/api/bot/game/{gameId}/abort", new FormUrlEncodedContent(new Dictionary<string, string>())).Wait();
        }

        public static void ResignAGame(string gameId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.PostAsync($"https://lichess.org/api/bot/game/{gameId}/resign", new FormUrlEncodedContent(new Dictionary<string, string>())).Wait();
        }
    }
}
