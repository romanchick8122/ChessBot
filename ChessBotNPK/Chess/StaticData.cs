using System;
using System.Collections.Generic;
using System.Text;

namespace ChessBotNPK.Chess
{
    public static class StaticData
    {

        public static readonly int[] FiguresPrices = new[] { 0, 11, 5, 3, 3, 1 };
        public const int figureCoefficient = 100;
        public const int CastlingPoints = 30;
        public const int AvailableCastlingPoints = 5;
        public static long[,,] RandomNumber;

        private static long WhiteHash;
        public static Dictionary<long, int> FieldValue = new Dictionary<long, int>();

        public static long GetHash(Field field, bool IsWhite)
        {
            long res = 0;
            foreach (var figure in field.Figures)
            {
                var figureNum = (int)(figure.FigureType);
                if (figure.IsWhite)
                    figureNum += 6;
                res ^= RandomNumber[figureNum,figure.Position.Col,figure.Position.Row];
            }
            if (IsWhite)
                res ^= WhiteHash;
            return res;
        }

        static StaticData()
        {
            var rng = new Random();
            WhiteHash = rng.Next();
            RandomNumber = new long[15, 8, 8];
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 8; k++)
                        RandomNumber[i, j, k] = rng.Next();
                }
            }
        }

        public static readonly int[,] lategame_pawn_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 20, 20, 20, 20, 20, 20, 20, 20 },
            { 12, 12, 12, 12, 12, 12, 12, 12 },
            { 8, 8, 8, 8, 8, 8, 8, 8 },
            { 5, 5, 5, 5, 5, 5, 5, 5 },
            { 3, 3, 3, 3, 3, 3, 3, 3 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        public static readonly int[,] pawn_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 4, 4, 0, 0, 0 },
            { 0, 4, 4, 8, 8, 4, 4, 0 },
            { 4, 7, 7, 4, 4, 7, 7, 4 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        public static readonly int[,] knight_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { -4, 4, 5, 4, 4, 5, 4, 0 },
            { -4, 6, 6, 4, 4, 6, 6, -4 },
            { -4, 6, 8, 5, 5, 8, 6, -4 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        public static readonly int[,] bishop_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 3, 6, 8, 3, 3, 8, 6, 3 },
            { 2, 6, 3, 2, 2, 3, 6, 2 },
            { 0, 0, 0, 2, 2, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        public static readonly int[,] queen_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { -5, -5, -5, -5, -5, -5, -5, -5 },
            { -3, -3, -3, -3, -3, -3, -3, -3 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        public static readonly int[,] tower_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { -3, 0, 0, 0, 0, 0, 0, -3 },
            { 0, 0, 3, 4, 4, 3, 0, 0 }
        };

        public static readonly int[,] king_table =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { -8, -8, -8, -8, -8, -8, -8, -8 },
            { -5, -5, -5, -5, -5, -5, -5, -5 },
            { -5, -5, -5, -5, -5, -5, -5, -5 }
        };
    }
}
