using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ChessBotNPK.Chess;
using ChessBotNPK.LichessApi;
using System.Linq;
namespace ChessBotNPK
{
    class LichessAgent : Agent
    {
        public LichessAgent(string gameId)
        {
            cancellationTokenSource = new CancellationTokenSource();
            GameId = gameId;

            Mutex gameStateReceived = new Mutex();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ChessBot.StreamGameState(GameId, evt =>
            {
                switch((string)evt["type"])
                {
                    case "gameFull":
                        //TODO: FixIt
                        break;
                    case "gameState":
                        var mvs = ((string)evt["moves"]).Split(" ");
                        if (mvs.Length > (Game.MovesHistory).Count)
                        {
                            var mv = mvs.Last();
                            var from = new Position('8' - mv[1], mv[0] - 'a');
                            var to = new Position('8' - mv[3], mv[2] - 'a');
                            var transformToFigure= Figure.FiguresType.None;
                            if(mv.Length > 4)
                            {
                                switch(mv[4])
                                {
                                    case 'r':
                                        transformToFigure = Figure.FiguresType.Rook;
                                        break;
                                    case 'n':
                                        transformToFigure = Figure.FiguresType.Knight;
                                        break;
                                    case 'b':
                                        transformToFigure = Figure.FiguresType.Bishop;
                                        break;
                                    case 'q':
                                        transformToFigure = Figure.FiguresType.Queen;
                                        break;
                                }
                            }
                            MoveWanted.WaitOne();
                            MoveWanted.Reset();
                            passed = new Move(from, to) { transformTo = transformToFigure};
                            MoveMade.Set();
                        }
                        break;
                }
            }, cancellationTokenSource.Token, Tokens.FairBotToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public string GameId { get; private set; }
        private CancellationTokenSource cancellationTokenSource;

        private ManualResetEvent MoveMade = new ManualResetEvent(false);
        private ManualResetEvent MoveWanted = new ManualResetEvent(false);
        private Move passed;

        public override Move GetMove(TimeSpan maxTime)
        {
            if(Game.MovesHistory.Any())
                LichessApi.ChessBot.MakeMove(GameId, Game.MovesHistory.Last().ToString(), Tokens.FairBotToken);
            MoveWanted.Set();
            MoveMade.WaitOne();
            MoveMade.Reset();
            return passed;
        }

        public override void Initialize()
        {

        }

#pragma warning disable CS0465 // Введение метода Finalize может помешать вызову деструктора
        public override void Finalize()
#pragma warning restore CS0465 // Введение метода Finalize может помешать вызову деструктора
        {
            var lastMove = Game.MovesHistory.Last();
            if (Game.GameState.Matrix[lastMove.To.Col, lastMove.To.Row].IsWhite != IsWhite)
            {
                LichessApi.ChessBot.MakeMove(GameId, lastMove.ToString(), Tokens.FairBotToken);
            }
            cancellationTokenSource.Cancel();
        }
    }
}
