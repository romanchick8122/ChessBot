using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ChessBotNPK.LichessApi
{
    public static class Challenges
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static void AcceptAChallenge(string challengeId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.PostAsync($"https://lichess.org/api/challenge/{challengeId}/accept", new FormUrlEncodedContent(new Dictionary<string, string>())).Wait();
        }
        public static void DeclineAChallenge(string challengeId, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpClient.PostAsync($"https://lichess.org/api/challenge/{challengeId}/decline", new FormUrlEncodedContent(new Dictionary<string, string>())).Wait();
        }
    }
}
