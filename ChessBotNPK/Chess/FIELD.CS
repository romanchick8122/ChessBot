using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ChessBotNPK.Chess
{
    public class Field : ICloneable, IEquatable<Field>, IEqualityComparer<Field>
    {
        [Flags]
        public enum FieldFlags
        {
            WhiteLongCastlingAvailable = 0b000001,
            WhiteShortCastlingAvailable = 0b000010,
            BlackLongCastlingAvailable = 0b000100,
            BlackShortCastlingAvailable = 0b001000,
            WhiteCastled = 0b010000,
            BlackCastled = 0b100000
        }

        public List<Figure> Figures;
        public FieldFlags Flags = FieldFlags.BlackLongCastlingAvailable | FieldFlags.BlackShortCastlingAvailable | FieldFlags.WhiteLongCastlingAvailable | FieldFlags.WhiteShortCastlingAvailable;
        public int MovesFromStart = 0;
        public int WhiteCastlingMove = int.MaxValue;
        public int BlackCastlingMove = int.MaxValue;
        private Position whiteKingPosition;
        private Position blackKingPosition;
        public Move LastWhiteMove, LastBlackMove;
        public Figure[,] Matrix = new Figure[8, 8];

        private void remakeMatrix()
        {
            foreach (var figure in Figures)
            {
                if (figure.IsDestroyed)
                    continue;
                Matrix[figure.Position.Col, figure.Position.Row] = figure;
            }
        }

        public Field()
        {
            Figures = new List<Figure>
            {
                new Figure(Figure.FiguresType.Rook, new Position(0, 0), this, false),
                new Figure(Figure.FiguresType.Rook, new Position(0, 7), this, false),
                new Figure(Figure.FiguresType.Rook, new Position(7, 0), this, true),
                new Figure(Figure.FiguresType.Rook, new Position(7, 7), this, true),

                new Figure(Figure.FiguresType.Knight, new Position(0, 1), this, false),
                new Figure(Figure.FiguresType.Knight, new Position(0, 6), this, false),
                new Figure(Figure.FiguresType.Knight, new Position(7, 1), this, true),
                new Figure(Figure.FiguresType.Knight, new Position(7, 6), this, true),

                new Figure(Figure.FiguresType.Bishop, new Position(0, 2), this, false),
                new Figure(Figure.FiguresType.Bishop, new Position(0, 5), this, false),
                new Figure(Figure.FiguresType.Bishop, new Position(7, 2), this, true),
                new Figure(Figure.FiguresType.Bishop, new Position(7, 5), this, true),

                new Figure(Figure.FiguresType.Queen, new Position(0, 3), this, false),
                new Figure(Figure.FiguresType.Queen, new Position(7, 3), this, true),

                new Figure(Figure.FiguresType.King, new Position(0, 4), this, false),
                new Figure(Figure.FiguresType.King, new Position(7, 4), this, true)
            };
            whiteKingPosition = new Position(7, 4);
            blackKingPosition = new Position(0, 4);
            for (var i = 0; i <= 7; i++)
            {
                Figures.Add(new Figure(Figure.FiguresType.Pawn, new Position(1, i), this, false));
                Figures.Add(new Figure(Figure.FiguresType.Pawn, new Position(6, i), this, true));
            }

            remakeMatrix();
            realHash = StaticData.GetHash(this, false);
        }
        public Field(IEnumerable<Figure> figures)
        {
            Figures = figures.ToList();
            foreach (var i in Figures)
                if (i.FigureType == Figure.FiguresType.King)
                    if (i.IsWhite)
                        whiteKingPosition = i.Position;
                    else
                        blackKingPosition = i.Position;
            remakeMatrix();
            realHash = StaticData.GetHash(this, false);
        }

        public int Val()
        {
            if (StaticData.FieldValue.ContainsKey(realHash))
                return StaticData.FieldValue[realHash];
            int WhiteVal = 0, BlackVal = 0;
            double remaining = (double)Figures.Count / 32.0;
            foreach (var i in Figures)
            {
                if (i.IsDestroyed)
                    continue;
                if (i.IsWhite)
                    WhiteVal += StaticData.FiguresPrices[(int)i.FigureType] * StaticData.figureCoefficient;
                else
                    BlackVal += StaticData.FiguresPrices[(int)i.FigureType] * StaticData.figureCoefficient;
            }

            //adding tables
            if (remaining > 0.7)
            {
                foreach (var figure in Figures)
                {
                    if (figure.IsDestroyed)
                        continue;
                    switch (figure.FigureType)
                    {
                        case Figure.FiguresType.Knight:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.knight_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.knight_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                        case Figure.FiguresType.King:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.king_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.king_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                        case Figure.FiguresType.Pawn:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.pawn_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.pawn_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                        case Figure.FiguresType.Rook:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.tower_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.tower_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                        case Figure.FiguresType.Queen:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.queen_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.queen_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                        case Figure.FiguresType.Bishop:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.bishop_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.bishop_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                    }
                }
            }

            //lategame bonuses
            if (remaining < 0.4)
            {
                foreach (var figure in Figures)
                {
                    if (figure.IsDestroyed)
                        continue;
                    switch (figure.FigureType)
                    {
                        case Figure.FiguresType.Pawn:
                            if (figure.IsWhite)
                            {
                                WhiteVal += StaticData.lategame_pawn_table[figure.Position.Col, figure.Position.Row];
                            }
                            else
                            {
                                BlackVal += StaticData.lategame_pawn_table[7 - figure.Position.Col, figure.Position.Row];
                            }
                            break;
                    }
                }
            }

            WhiteVal += Math.Max(0, StaticData.CastlingPoints - WhiteCastlingMove);
            BlackVal += Math.Max(0, StaticData.CastlingPoints - BlackCastlingMove);
            if (Flags.HasFlag(FieldFlags.WhiteLongCastlingAvailable) || Flags.HasFlag(FieldFlags.WhiteShortCastlingAvailable))
                WhiteVal += Math.Min(StaticData.AvailableCastlingPoints, StaticData.CastlingPoints - MovesFromStart);
            if (Flags.HasFlag(FieldFlags.BlackLongCastlingAvailable) || Flags.HasFlag(FieldFlags.BlackShortCastlingAvailable))
                BlackVal += Math.Min(StaticData.AvailableCastlingPoints, StaticData.CastlingPoints - MovesFromStart);
            StaticData.FieldValue[realHash] = WhiteVal - BlackVal;
            return WhiteVal - BlackVal;
        }

        public bool IsCastling(Move move)
        {
            if (Matrix[move.From.Col, move.From.Row].FigureType == Figure.FiguresType.King)
            {
                if (Matrix[move.From.Col, move.From.Row].IsWhite)
                {
                    if (move.From.Row == 4 && move.From.Col == 7)
                    {
                        if (move.To.Row == 6 && move.To.Col == 7)
                        {
                            return true;
                        }
                        else if (move.To.Row == 2 && move.To.Col == 7)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (move.From.Row == 4 && move.From.Col == 0)
                    {
                        if (move.To.Row == 6 && move.To.Col == 0)
                        {
                            return true;
                        }
                        else if (move.To.Row == 2 && move.To.Col == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsAisleTaking(Move move)
        {
            return Matrix[move.From.Col, move.From.Row].FigureType == Figure.FiguresType.Pawn
                && Matrix[move.To.Col, move.To.Row] == null
                && move.From.Row != move.To.Row
                && Matrix[move.From.Col, move.To.Row].FigureType == Figure.FiguresType.Pawn;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The field to be checked</param>
        /// <param name="IsWhite">YOUR color. If IsWhite, returns true if attacked by black.</param>
        /// <returns></returns>
        [Pure]
        public bool FieldIsAttacked(Position position, bool IsWhite)
        {
            var col = Figures.Count;

            for (var dirVer = -1; dirVer <= 1; dirVer += 2)
            {
                for (var dirHor = -1; dirHor <= 1; dirHor += 2)
                {
                    for (var len = 1; ; len++)
                    {
                        Position NewPosition = new Position(position.Col + len * dirVer, position.Row + len * dirHor);
                        if (!NewPosition.Valid())
                        {
                            break;
                        }
                        if (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed)
                        {
                            if (Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite)
                                break;
                            if (Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Bishop || Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Queen)
                                return true;
                            break;
                        }
                    }
                }
            }

            //going vertical
            for (int dir = -1; dir <= 1; dir += 2)
            {
                for (int len = 1; ; len++)
                {
                    Position NewPosition = new Position(position.Col + len * dir, position.Row);
                    if (!NewPosition.Valid())
                    {
                        break;
                    }
                    if (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed)
                    {
                        if (Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite)
                            break;
                        if (Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Rook || Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Queen)
                            return true;
                        break;
                    }
                }
            }

            //going horizontal
            for (int dir = -1; dir <= 1; dir += 2)
            {
                for (int len = 1; ; len++)
                {
                    Position NewPosition = new Position(position.Col, position.Row + len * dir);
                    if (!NewPosition.Valid())
                    {
                        break;
                    }
                    if (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed)
                    {
                        if (Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite)
                            break;
                        if (Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Rook || Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Queen)
                            return true;
                        break;
                    }
                }
            }

            for (var deltaHor = -2; deltaHor <= 2; deltaHor++)
            {
                for (var deltaVer = -2; deltaVer <= 2; deltaVer++)
                {
                    if (Math.Abs(deltaVer) == Math.Abs(deltaHor) || deltaVer == 0 || deltaHor == 0)
                    {
                        continue;
                    }

                    Position NewPosition = new Position(position.Col + deltaHor, position.Row + deltaVer);
                    if (!NewPosition.Valid())
                    {
                        continue;
                    }
                    if (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed)
                    {
                        if (Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite)
                            continue;
                        if (Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Knight)
                            return true;
                        continue;
                    }
                }
            }

            for (var verticalDir = -1; verticalDir <= 1; verticalDir++)
            {
                for (var horizontalDir = -1; horizontalDir <= 1; horizontalDir++)
                {
                    if (horizontalDir == 0 && verticalDir == 0)
                        continue;

                    Position NewPosition = new Position(position.Col + verticalDir, position.Row + horizontalDir);
                    if (!NewPosition.Valid())
                    {
                        continue;
                    }
                    if (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed)
                    {
                        if (Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite)
                            continue;
                        if (Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.King)
                            return true;
                        continue;
                    }
                }
            }

            for (var dir = -1; dir <= 1; dir += 2)
            {
                Position NewPosition = new Position(position.Col + (IsWhite ? 1 : -1), position.Row + dir);
                if (!NewPosition.Valid())
                {
                    continue;
                }
                if (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed)
                {
                    if (Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite)
                        continue;
                    if (Matrix[NewPosition.Col, NewPosition.Row].FigureType == Figure.FiguresType.Pawn)
                        return true;
                }
            }

            return false;
        }
        [Pure]
        public bool KingIsAttacked(bool IsWhite) => FieldIsAttacked(IsWhite ? whiteKingPosition : blackKingPosition, !IsWhite);

        private bool EvaluateMove(Figure figure, Position NewPosition, bool IsWhite)
        {
            if (!NewPosition.Valid())
            {
                return false;
            }
            Figure OldFigure = Matrix[NewPosition.Col, NewPosition.Row];
            if (figure.FigureType == Figure.FiguresType.Pawn
                && figure.Position.Row != NewPosition.Row
                && OldFigure == null
                && Matrix[figure.Position.Col, NewPosition.Row].FigureType == Figure.FiguresType.Pawn)
            {
                OldFigure = Matrix[figure.Position.Col, NewPosition.Row];
            }
            bool prevDes = false;
            if (OldFigure != null)
            {
                if (!OldFigure.IsDestroyed && OldFigure.IsWhite == IsWhite)
                    return false;
                prevDes = OldFigure.IsDestroyed;
                OldFigure.IsDestroyed = true;
            }

            Position OldPosition = figure.Position;
            Matrix[OldPosition.Col, OldPosition.Row] = null;
            Matrix[NewPosition.Col, NewPosition.Row] = figure;
            figure.Position = NewPosition;

            bool res = false;
            if (!KingIsAttacked(IsWhite))
                res = true;

            figure.Position = OldPosition;
            Matrix[OldPosition.Col, OldPosition.Row] = figure;
            Matrix[NewPosition.Col, NewPosition.Row] = OldFigure;
            if (OldFigure != null)
            {
                OldFigure.IsDestroyed = prevDes;
            }
            return res;
        }

        [Pure]
        public List<Move> GetAvailableMoves(bool IsWhite)
        {
            var moves = new List<Move>();
            var col = Figures.Count;

            foreach (var figure in Figures)
            {
                if (figure.IsWhite != IsWhite || figure.IsDestroyed)
                    continue;
                switch (figure.FigureType)
                {
                    case Figure.FiguresType.Bishop:
                        for (var dirVer = -1; dirVer <= 1; dirVer += 2)
                        {
                            for (var dirHor = -1; dirHor <= 1; dirHor += 2)
                            {
                                for (var len = 1; ; len++)
                                {
                                    Position NewPosition = new Position(figure.Position.Col + len * dirVer, figure.Position.Row + len * dirHor);
                                    if (EvaluateMove(figure, NewPosition, IsWhite))
                                        moves.Add(new Move(figure.Position, NewPosition));
                                    if (!NewPosition.Valid() || (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed))
                                        break;
                                }
                            }
                        }
                        break;

                    case Figure.FiguresType.Queen:
                        for (var dirVer = -1; dirVer <= 1; dirVer += 2)
                        {
                            for (var dirHor = -1; dirHor <= 1; dirHor += 2)
                            {
                                for (var len = 1; ; len++)
                                {
                                    Position NewPosition = new Position(figure.Position.Col + len * dirVer, figure.Position.Row + len * dirHor);
                                    if (EvaluateMove(figure, NewPosition, IsWhite))
                                        moves.Add(new Move(figure.Position, NewPosition));
                                    if (!NewPosition.Valid() || (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed))
                                        break;
                                }
                            }
                        }
                        //going vertical
                        for (int dir = -1; dir <= 1; dir += 2)
                        {
                            for (int len = 1; ; len++)
                            {
                                Position NewPosition = new Position(figure.Position.Col + len * dir, figure.Position.Row);
                                if (EvaluateMove(figure, NewPosition, IsWhite))
                                    moves.Add(new Move(figure.Position, NewPosition));
                                if (!NewPosition.Valid() || (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed))
                                    break;
                            }
                        }

                        //going horizontal
                        for (int dir = -1; dir <= 1; dir += 2)
                        {
                            for (int len = 1; ; len++)
                            {
                                Position NewPosition = new Position(figure.Position.Col, figure.Position.Row + len * dir);
                                if (EvaluateMove(figure, NewPosition, IsWhite))
                                    moves.Add(new Move(figure.Position, NewPosition));
                                if (!NewPosition.Valid() || (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed))
                                    break;
                            }
                        }
                        break;

                    case Figure.FiguresType.Knight:

                        for (var deltaHor = -2; deltaHor <= 2; deltaHor++)
                        {
                            for (var deltaVer = -2; deltaVer <= 2; deltaVer++)
                            {
                                if (Math.Abs(deltaVer) == Math.Abs(deltaHor) || deltaVer == 0 || deltaHor == 0)
                                {
                                    continue;
                                }

                                Position NewPosition = new Position(figure.Position.Col + deltaHor, figure.Position.Row + deltaVer);
                                if (EvaluateMove(figure, NewPosition, IsWhite))
                                    moves.Add(new Move(figure.Position, NewPosition));
                            }
                        }

                        break;
                    case Figure.FiguresType.Pawn:

                        //taking a step forward
                        for (int j = 1; j <= 2; j++)
                        {
                            if (j == 2 && figure.Position.Col != (IsWhite ? 6 : 1))
                                break;
                            Position NewPosition = new Position(figure.Position.Col + j * ((IsWhite ? -1 : 1)), figure.Position.Row);
                            if (!NewPosition.Valid() || (Matrix[NewPosition.Col, NewPosition.Row] != null && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed))
                                break;
                            if (EvaluateMove(figure, NewPosition, IsWhite))
                                if (NewPosition.Col == (IsWhite ? 0 : 7))
                                {
                                    moves.Add(new Move(figure.Position, NewPosition) { transformTo = Figure.FiguresType.Queen });
                                    moves.Add(new Move(figure.Position, NewPosition) { transformTo = Figure.FiguresType.Knight });
                                }
                                else moves.Add(new Move(figure.Position, NewPosition));
                        }

                        //taking an enemy figure
                        for (var dir = -1; dir <= 1; dir += 2)
                        {
                            Position NewPosition = new Position(figure.Position.Col + (IsWhite ? -1 : 1), figure.Position.Row + dir);
                            if (!NewPosition.Valid())
                                continue;
                            if (!(Matrix[NewPosition.Col, NewPosition.Row] != null && Matrix[NewPosition.Col, NewPosition.Row].IsWhite != IsWhite && !Matrix[NewPosition.Col, NewPosition.Row].IsDestroyed))
                                continue;
                            if (EvaluateMove(figure, NewPosition, IsWhite))
                                if (NewPosition.Col == (IsWhite ? 0 : 7))
                                {
                                    moves.Add(new Move(figure.Position, NewPosition) { transformTo = Figure.FiguresType.Queen });
                                    moves.Add(new Move(figure.Position, NewPosition) { transformTo = Figure.FiguresType.Knight });
                                }
                                else moves.Add(new Move(figure.Position, NewPosition));
                        }

                        break;

                    case Figure.FiguresType.King:

                        for (var verticalDir = -1; verticalDir <= 1; verticalDir++)
                        {
                            for (var horizontalDir = -1; horizontalDir <= 1; horizontalDir++)
                            {
                                if (horizontalDir == 0 && verticalDir == 0)
                                    continue;

                                Position NewPosition = new Position(figure.Position.Col + verticalDir, figure.Position.Row + horizontalDir);
                                if (EvaluateMove(figure, NewPosition, IsWhite))
                                    moves.Add(new Move(figure.Position, NewPosition));
                            }
                        }

                        //castling
                        if (!KingIsAttacked(IsWhite))
                        {
                            if (IsWhite)
                            {
                                if (Flags.HasFlag(FieldFlags.WhiteLongCastlingAvailable))
                                {
                                    bool ok = true;
                                    for (int i = 2; i <= 3; i++)
                                    {
                                        if (Matrix[7, i] != null || FieldIsAttacked(new Position(7, i), !IsWhite))
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                    if (Matrix[7, 1] != null)
                                        ok = false;
                                    if (ok)
                                        moves.Add(new Move(figure.Position, new Position(7, 2)));
                                }
                                if (Flags.HasFlag(FieldFlags.WhiteShortCastlingAvailable))
                                {
                                    bool ok = true;
                                    for (int i = 5; i <= 6; i++)
                                    {
                                        if (Matrix[7, i] != null || FieldIsAttacked(new Position(7, i), !IsWhite))
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                    if (ok)
                                        moves.Add(new Move(figure.Position, new Position(7, 6)));
                                }
                            }
                            else
                            {
                                if (Flags.HasFlag(FieldFlags.BlackLongCastlingAvailable))
                                {
                                    bool ok = true;
                                    for (int i = 2; i <= 3; i++)
                                    {
                                        if (Matrix[0, i] != null || FieldIsAttacked(new Position(0, i), !IsWhite))
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                    if (Matrix[0, 1] != null)
                                        ok = false;
                                    if (ok)
                                        moves.Add(new Move(figure.Position, new Position(0, 2)));
                                }
                                if (Flags.HasFlag(FieldFlags.BlackShortCastlingAvailable))
                                {
                                    bool ok = true;
                                    for (int i = 5; i <= 6; i++)
                                    {
                                        if (Matrix[0, i] != null || FieldIsAttacked(new Position(0, i), !IsWhite))
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                    if (ok)
                                        moves.Add(new Move(figure.Position, new Position(0, 6)));
                                }
                            }
                        }

                        break;
                    case Figure.FiguresType.Rook:

                        //going vertical
                        for (int dir = -1; dir <= 1; dir += 2)
                        {
                            for (int len = 1; ; len++)
                            {
                                Position NewPosition = new Position(figure.Position.Col + len * dir, figure.Position.Row);
                                if (EvaluateMove(figure, NewPosition, IsWhite))
                                    moves.Add(new Move(figure.Position, NewPosition));
                                if (!NewPosition.Valid() || Matrix[NewPosition.Col, NewPosition.Row] != null)
                                    break;
                            }
                        }

                        //going horizontal
                        for (int dir = -1; dir <= 1; dir += 2)
                        {
                            for (int len = 1; ; len++)
                            {
                                Position NewPosition = new Position(figure.Position.Col, figure.Position.Row + len * dir);
                                if (EvaluateMove(figure, NewPosition, IsWhite))
                                    moves.Add(new Move(figure.Position, NewPosition));
                                if (!NewPosition.Valid() || Matrix[NewPosition.Col, NewPosition.Row] != null)
                                    break;
                            }
                        }

                        break;
                }
            }

            //TODO: Why repeating? temporary fix
            return moves.Distinct().RandomShuffle();
        }

        public void MakeMove(Move move)
        {
            MakeTempMove(move);
            Figures = Figures.Where(x => !x.IsDestroyed).ToList();
            realHash = StaticData.GetHash(this, false);
        }
        public void MakeTempMove(Move move)
        {
            #region CASTLING
            if (Matrix[move.From.Col, move.From.Row].FigureType == Figure.FiguresType.King)
            {
                if (Matrix[move.From.Col, move.From.Row].IsWhite)
                {
                    if (move.From.Row == 4 && move.From.Col == 7)
                    {
                        if (move.To.Row == 6 && move.To.Col == 7)
                        {
                            Matrix[7, 5] = Matrix[7, 7];
                            Matrix[7, 7] = null;
                            Matrix[7, 5].Position = new Position(7, 6);
                            Flags |= FieldFlags.WhiteCastled;
                            WhiteCastlingMove = MovesFromStart;
                        }
                        else if (move.To.Row == 2 && move.To.Col == 7)
                        {
                            Matrix[7, 3] = Matrix[7, 0];
                            Matrix[7, 0] = null;
                            Matrix[7, 3].Position = new Position(7, 3);
                            Flags |= FieldFlags.WhiteCastled;
                            WhiteCastlingMove = MovesFromStart;
                        }
                    }
                }
                else
                {
                    if (move.From.Row == 4 && move.From.Col == 0)
                    {
                        if (move.To.Row == 6 && move.To.Col == 0)
                        {
                            Matrix[0, 5] = Matrix[0, 7];
                            Matrix[0, 7] = null;
                            Matrix[0, 5].Position = new Position(0, 5);
                            Flags |= FieldFlags.BlackCastled;
                            BlackCastlingMove = MovesFromStart;
                        }
                        else if (move.To.Row == 2 && move.To.Col == 0)
                        {
                            Matrix[0, 3] = Matrix[0, 0];
                            Matrix[0, 0] = null;
                            Matrix[0, 3].Position = new Position(0, 3);
                            Flags |= FieldFlags.BlackCastled;
                            BlackCastlingMove = MovesFromStart;
                        }
                    }
                }
                if (Matrix[move.From.Col, move.From.Row].IsWhite)
                    Flags &= ~(FieldFlags.WhiteLongCastlingAvailable | FieldFlags.WhiteShortCastlingAvailable);
                else
                    Flags &= ~(FieldFlags.BlackLongCastlingAvailable | FieldFlags.BlackShortCastlingAvailable);
            }
            else if (Matrix[move.From.Col, move.From.Row].FigureType == Figure.FiguresType.Rook)
            {
                if (Matrix[move.From.Col, move.From.Row].IsWhite && move.From.Col == 7)
                {
                    if (move.From.Row == 0)
                        Flags &= ~(FieldFlags.WhiteLongCastlingAvailable);
                    else if (move.From.Row == 7)
                        Flags &= ~(FieldFlags.WhiteShortCastlingAvailable);
                }
                else if (!Matrix[move.From.Col, move.From.Row].IsWhite && move.From.Col == 0)
                {
                    if (move.From.Row == 0)
                        Flags &= ~(FieldFlags.BlackLongCastlingAvailable);
                    else if (move.From.Row == 7)
                        Flags &= ~(FieldFlags.BlackShortCastlingAvailable);
                }
            }
            if (move.To.Row == 0 && move.To.Col == 0)
                Flags &= ~FieldFlags.BlackLongCastlingAvailable;
            else if (move.To.Row == 0 && move.To.Col == 7)
                Flags &= ~FieldFlags.WhiteLongCastlingAvailable;
            else if (move.To.Row == 7 && move.To.Col == 0)
                Flags &= ~FieldFlags.BlackShortCastlingAvailable;
            else if (move.To.Row == 7 && move.To.Col == 7)
                Flags &= ~FieldFlags.WhiteShortCastlingAvailable;
            #endregion CASTLING
            #region TAKING ON THE AISLE
            if (Matrix[move.From.Col, move.From.Row].FigureType == Figure.FiguresType.Pawn
                && Matrix[move.To.Col, move.To.Row] == null
                && move.From.Row != move.To.Row
                && Matrix[move.From.Col, move.To.Row].FigureType == Figure.FiguresType.Pawn)
            {
                Matrix[move.From.Col, move.To.Row].IsDestroyed = true;
            }
            #endregion TAKING ON THE AISLE
            if (Matrix[move.From.Col, move.From.Row].FigureType == Figure.FiguresType.King)
                if (Matrix[move.From.Col, move.From.Row].IsWhite)
                    whiteKingPosition = move.To;
                else
                    blackKingPosition = move.To;
            if (Matrix[move.From.Col, move.From.Row].IsWhite)
                LastWhiteMove = move;
            else
                LastBlackMove = move;
            if (Matrix[move.To.Col, move.To.Row] != null)
                Matrix[move.To.Col, move.To.Row].IsDestroyed = true;
            Matrix[move.From.Col, move.From.Row].Position = move.To;
            Matrix[move.To.Col, move.To.Row] = Matrix[move.From.Col, move.From.Row];
            Matrix[move.From.Col, move.From.Row] = null;
            if (move.transformTo != Figure.FiguresType.None)
                Matrix[move.To.Col, move.To.Row].FigureType = move.transformTo;
            MovesFromStart++;
        }

        public void DenyLastMove(bool IsWhite, Figure figure = null, bool prevdes = false)
        {
            Move move = (IsWhite ? LastWhiteMove : LastBlackMove);
            MovesFromStart--;
            if (Matrix[move.To.Col, move.To.Row].FigureType == Figure.FiguresType.King)
                if (IsWhite)
                    whiteKingPosition = move.From;
                else
                    blackKingPosition = move.From;
            Matrix[move.To.Col, move.To.Row].Position = move.From;
            Matrix[move.From.Col, move.From.Row] = Matrix[move.To.Col, move.To.Row];
            Matrix[move.To.Col, move.To.Row] = figure;
            if (figure != null)
            {
                figure.IsDestroyed = prevdes;
            }
        }

        public object Clone()
        {
            var field = new Field(Figures.Select(x => (Figure)x.Clone()).ToList());
            field.Flags = Flags;
            field.MovesFromStart = MovesFromStart;
            field.WhiteCastlingMove = WhiteCastlingMove;
            field.BlackCastlingMove = BlackCastlingMove;
            field.LastWhiteMove = LastWhiteMove;
            field.LastBlackMove = LastBlackMove;
            field.whiteKingPosition = whiteKingPosition;
            field.blackKingPosition = blackKingPosition;
            foreach (var i in field.Figures)
            {
                if (i.IsDestroyed)
                    continue;
                i.Field = field;
            }
            return field;
        }

        private long realHash;
        public override int GetHashCode() => (int)((realHash & int.MaxValue) ^ (realHash >> 32));

        public bool Equals(Field x, Field y) => x.Equals(y);

        public int GetHashCode(Field obj) => obj.GetHashCode();

        public bool Equals(Field other) => realHash == other.realHash;
    }
}
