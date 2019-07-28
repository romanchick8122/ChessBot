using ChessBotNPK.Chess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace ChessBotNPK.MonteCarloTree
{
    class MonteCarloTreeVertex
    {
        private Random random = new Random();
        private byte[] val = new byte[2];

        public Dictionary<Move, MonteCarloTreeVertex> moves;
        private Field field;

        public MonteCarloTreeVertex(Field field, Move move)
        {
            this.field = (Field)field.Clone();
            this.field.MakeMove(move);
        }

        public MonteCarloTreeVertex(Field field)
        {
            this.field = (Field)field.Clone();
        }

        public Field GetField(Move move)
        {
            throw new NotImplementedException();
        }

        public  int GoDown(int RemainingHeight, bool IsWhite)
        {
            if (moves == null)
            {
                moves = new Dictionary<Move, MonteCarloTreeVertex>();
                foreach(var move in field.GetAvailableMoves(IsWhite))
                {
                    moves.Add(move, new MonteCarloTreeVertex(field, move));
                }
            }

            if (moves.Keys.Count == 0)
                return (IsWhite ? int.MinValue : int.MaxValue);
            if (RemainingHeight == 0)
                return field.Val();
            
            var valueslist = moves.Values.ToList();
            return valueslist[random.Next(0, valueslist.Count)].GoDown(RemainingHeight - 1, !IsWhite);
        }
    }
}
