using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ChessBotNPK.Chess
{
    public class Game
    {
        public Game(Agent whiteAgent, Agent blackAgent)
        {
            WhiteAgent = whiteAgent;
            BlackAgent = blackAgent;
            WhiteAgent.Game = this;
            BlackAgent.Game = this;
            WhiteAgent.IsWhite = true;
            BlackAgent.IsWhite = false;
            GameState = new Field();
            MovesHistory = new List<Move>();
            WhiteAgent.Initialize();
            BlackAgent.Initialize();
        }

        public void Play(TimeSpan maxTime)
        {
            while(true)
            {
                if (!GameState.GetAvailableMoves(true).Any()) break;
                var move = WhiteAgent.GetMove(maxTime);
                GameState.MakeMove(move);
                MovesHistory.Add(move);
                if (!GameState.GetAvailableMoves(false).Any()) break;
                move = BlackAgent.GetMove(maxTime);
                GameState.MakeMove(move);
                MovesHistory.Add(move);
            }
            WhiteAgent.Finalize();
            BlackAgent.Finalize();
        }

        public List<Move> MovesHistory { get; set; }
        public Field GameState { get; set; }
        public Agent WhiteAgent { get; private set; }
        public Agent BlackAgent { get; private set; }
    }
}
