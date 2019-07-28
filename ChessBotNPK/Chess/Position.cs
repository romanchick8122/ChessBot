using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ChessBotNPK.Chess
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Position : IEquatable<Position>, IEqualityComparer<Position>
    {
        [FieldOffset(0)]
        public int Col;
        [FieldOffset(4)]
        public int Row;

        public Position(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public bool Equals(Position other) => Col == other.Col && Row == other.Row;

        public bool Equals(Position x, Position y) => x.Equals(y);

        public override int GetHashCode() => Col * 8 + Row;

        public int GetHashCode(Position obj) => obj.GetHashCode();

        public override string ToString() => "" + (char)(Row + 'a') + (char)('8' - Col);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Valid() => (Col & ~0b111) == 0 && (Row & ~0b111) == 0;
    }
}
