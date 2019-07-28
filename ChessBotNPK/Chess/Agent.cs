using System;
using System.Collections.Generic;
using System.Text;

namespace ChessBotNPK.Chess
{
    public abstract class Agent
    {
        public bool IsWhite;
        public Game Game;
        public abstract Move GetMove(TimeSpan maxTime);

        public abstract void Initialize();
#pragma warning disable CS0465 // Введение метода Finalize может помешать вызову деструктора
        public abstract void Finalize();
#pragma warning restore CS0465 // Введение метода Finalize может помешать вызову деструктора
    }
}
