using System;
using System.Threading;
using ChessBotNPK.Chess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ChessBotNPK
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            LichessApi.ChessBot.StreamIncomingEvents(x =>
            {
                if((string)x["type"] == "challenge")
                {
                    if ((bool)x["challenge"]["rated"]) LichessApi.Challenges.DeclineAChallenge((string)x["challenge"]["id"], Tokens.FairBotToken);
                    else LichessApi.Challenges.AcceptAChallenge((string)x["challenge"]["id"], Tokens.FairBotToken);
                }
                else if((string)x["type"] == "gameStart")
                {
                    var game = new Game(new LichessAgent((string)x["game"]["id"]), new MiniMax.MiniMax());
                    new Thread(time => game.Play((TimeSpan)time)).Start(TimeSpan.FromSeconds(40));
                }
            }, cts.Token, Tokens.FairBotToken).Wait();
        }
    }
}
