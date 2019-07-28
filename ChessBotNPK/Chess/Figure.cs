using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChessBotNPK.Chess
{
    [DebuggerDisplay("{FigureType} {Position}")]
    public class Figure : ICloneable
    {
        public Figure(FiguresType figureType, Position position, Field field, bool isWhite, bool isDestroyed = false)
        {
            FigureType = figureType;
            Position = position;
            Field = field;
            IsDestroyed = isDestroyed;
            IsWhite = isWhite;
        }

        public enum FiguresType
        {
            None = -1,
            King = 0,
            Queen = 1,
            Rook = 2,
            Bishop = 3,
            Knight = 4,
            Pawn = 5,
        }
        public FiguresType FigureType { get; set; }
        public Position Position { get; set; }
        public Field Field { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsWhite { get; private set; }
        public object Clone() => new Figure(FigureType, Position, Field, IsWhite, IsDestroyed);
    }
}
