using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ChessBotNPK.Chess
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Move : IEquatable<Move>, IEqualityComparer<Move>
    {
        public Move(Position from, Position to) : this()
        {
            From = from;
            To = to;
            transformTo = Figure.FiguresType.None;
        }
        [FieldOffset(0)]
        public Position From;
        [FieldOffset(8)]
        public Position To;
        [FieldOffset(16)]
        public Figure.FiguresType transformTo;
        public bool Equals(Move other) => From.Equals(other.From) && To.Equals(other.To) && transformTo == other.transformTo;

        public bool Equals(Move x, Move y) => x.Equals(y);

        public int GetHashCode(Move obj) => obj.GetHashCode();

        public override string ToString()
        {
            switch (transformTo)
            {
                case Figure.FiguresType.None:
                    return From.ToString() + To.ToString();
                case Figure.FiguresType.Bishop:
                    return From.ToString() + To.ToString() + "b";
                case Figure.FiguresType.Knight:
                    return From.ToString() + To.ToString() + "n";
                case Figure.FiguresType.Queen:
                    return From.ToString() + To.ToString() + "q";
                case Figure.FiguresType.Rook:
                    return From.ToString() + To.ToString() + "r";
            }
            return From.ToString() + To.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = -1330770945;
            hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(From);
            hashCode = hashCode * -1521134295 + EqualityComparer<Position>.Default.GetHashCode(To);
            hashCode = hashCode * -1521134295 + transformTo.GetHashCode();
            return hashCode;
        }
    }
}
