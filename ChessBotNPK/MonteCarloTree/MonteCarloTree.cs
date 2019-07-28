using ChessBotNPK.Chess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChessBotNPK.MonteCarloTree
{
    public class MonteCarloTree : Agent
    {

        private MonteCarloTreeVertex root;
        private readonly int Height;
        private readonly int Width;
        private const int INF = 10000;
        
        public MonteCarloTree(int height, int width)
        {
            Height = height;
            Width = width;
        }

        public void DoMove(Move move)
        {
            if (root.moves == null) return;
            if (!root.moves.ContainsKey(move)) return;
            root = root.moves[move];
        }

        public override Move GetMove(TimeSpan maxTime)
        {
            if (Game.MovesHistory.Any())
                DoMove(Game.MovesHistory.Last());
            var rand = new Random();

            var MoveList = Game.GameState.GetAvailableMoves(IsWhite);
            var MoveSum = Enumerable.Repeat(0, MoveList.Count).ToList();
            int BestVal = int.MinValue, BestMoveIndex = -1;
            
            for (int i = 0; i < Width; i++)
            {
                int MoveIndex = rand.Next(MoveList.Count());
                int MoveVal = root.GoDown(Height, IsWhite);
                MoveSum[MoveIndex] += MoveVal;
            }
            
            for(int i = 0; i < MoveList.Count; i++)
            {
                if (MoveSum[i] * (IsWhite ? 1 : -1) > BestVal)
                    BestMoveIndex = i;
            }

            DoMove(MoveList[BestMoveIndex]);
            return MoveList[BestMoveIndex];
        }

        public override void Initialize()
        {
            root = new MonteCarloTreeVertex(Game.GameState);
        }

#pragma warning disable CS0465 // Введение метода Finalize может помешать вызову деструктора
        public override void Finalize()
#pragma warning restore CS0465 // Введение метода Finalize может помешать вызову деструктора
        {

        }
    }
}
