using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ChessBotNPK.Chess;

namespace ChessBotNPK.MiniMax
{
    public class MiniMax : Agent
    {
        private int alpha = int.MaxValue, beta = int.MinValue;
        public Dictionary<int, int> Answers = new Dictionary<int, int>();
        bool UseHash = false;

        public MiniMax(bool UseHashing = false)
        {
            UseHash = UseHashing;
        }


        int DFS(Field curField, int Alpha, int Beta, bool IsWhite, int d)
        {
            if (d == 0)
                return curField.Val();
            var MoveList = curField.GetAvailableMoves(IsWhite);
            if (MoveList.Count == 0)
            {
                if (curField.KingIsAttacked(IsWhite))
                    return (IsWhite ? int.MinValue + 500 : int.MaxValue - 500);
                else
                    return 0;
            }

            foreach (var i in MoveList)
            {
                var res = 0;

                if (true || curField.IsCastling(i) || curField.IsAisleTaking(i) || i.transformTo != Figure.FiguresType.None)
                {
                    Field NewField = (Field)curField.Clone();
                    NewField.MakeTempMove(i);

                    res = DFS(NewField, Alpha, Beta, !IsWhite, d - 1);
                }
                else
                {
                    Position NewPosition = i.To;
                    Figure figure = curField.Matrix[i.From.Col, i.From.Row];
                    Figure OldFigure = curField.Matrix[NewPosition.Col, NewPosition.Row];
                    bool prevDes = false;
                    if (OldFigure != null)
                    {
                        prevDes = OldFigure.IsDestroyed;
                        OldFigure.IsDestroyed = true;
                    }

                    Position OldPosition = figure.Position;
                    curField.Matrix[OldPosition.Col, OldPosition.Row] = null;
                    curField.Matrix[NewPosition.Col, NewPosition.Row] = figure;
                    figure.Position = NewPosition;

                    res = DFS(curField, Alpha, Beta, !IsWhite, d - 1);

                    figure.Position = OldPosition;
                    curField.Matrix[OldPosition.Col, OldPosition.Row] = figure;
                    curField.Matrix[NewPosition.Col, NewPosition.Row] = OldFigure;
                    if (OldFigure != null)
                    {
                        OldFigure.IsDestroyed = prevDes;
                    }
                }

                if (IsWhite)
                {
                    Alpha = Math.Max(Alpha, res);
                    if (Alpha >= Beta)
                        return Alpha;
                }
                else
                {
                    Beta = Math.Min(Beta, res);
                    if (Alpha >= Beta)
                        return Beta;
                }

            }

            return (IsWhite ? Alpha : Beta);
        }

        public override Move GetMove(TimeSpan maxTime)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Field curField = (Field)Game.GameState.Clone();
            var MoveList = curField.GetAvailableMoves(IsWhite);
            Move BestMove = MoveList[0];

            for (int deapth = 0; ; deapth++)
            {
                if (stopwatch.Elapsed > maxTime)
                    return BestMove;
                var BestVal = int.MinValue;
                Move CurBestMove = MoveList[0];
                alpha = int.MinValue; beta = int.MaxValue;
                var st = DFS(curField, alpha, beta, !IsWhite, deapth);
                if (IsWhite)
                    alpha = Math.Max(alpha, st);
                else
                    beta = Math.Min(beta, st);

                foreach (var i in MoveList)
                {

                    Field NewField = (Field)curField.Clone();
                    NewField.MakeTempMove(i);

                    var res = DFS(NewField, alpha, beta, !IsWhite, deapth);
                    if (IsWhite)
                        alpha = Math.Max(alpha, res);
                    else
                        beta = Math.Min(beta, res);
                    if (res * (IsWhite ? 1 : -1) > BestVal)
                    {
                        BestVal = res * (IsWhite ? 1 : -1);
                        CurBestMove = i;
                    }
                    if (stopwatch.Elapsed > maxTime)
                        return BestMove;
                }
                BestMove = CurBestMove;
            }
        }

        public override void Initialize()
        {

        }

#pragma warning disable CS0465 // Введение метода Finalize может помешать вызову деструктора
        public override void Finalize()
#pragma warning restore CS0465 // Введение метода Finalize может помешать вызову деструктора
        {

        }
    }
}
