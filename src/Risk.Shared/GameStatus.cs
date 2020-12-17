using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Risk.Shared
{
    public class GameStatus
    {
        public IEnumerable<string> Players { get; set; }
        public Collection<PlayerStats> PlayerStats { get; set; }
        public GameState GameState { get; set; }
        public IEnumerable<BoardTerritory> Board { get; set; }

        public string PlayerActionDescription { get; set; }

        public GameStatus()
        {
            Players = new List<string>();
            PlayerStats = new Collection<PlayerStats>();
        }

        public GameStatus(IEnumerable<string> players, GameState gameState, IEnumerable<BoardTerritory> board, IEnumerable<PlayerStats> playerStats )
        {
            Players = players;
            GameState = gameState;
            Board = board;
            PlayerStats = new Collection<PlayerStats>(playerStats.ToList());
        }
    }

    public class PlayerStats
    {
        public string Name { get; set; }
        public int Armies { get; set; }
        public int Territories { get; set; }
        public int Score { get; set; }
        public int ContinentBonus { get; set; }
    }
}
