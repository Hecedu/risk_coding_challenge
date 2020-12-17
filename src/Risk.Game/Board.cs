using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Risk.Shared;

namespace Risk.Game
{
    public class Board
    {
        public Board(IEnumerable<Territory> territiories)
        {
            Territories = territiories;
        }

        public IEnumerable<Territory> Territories { get; }

        public IEnumerable<BoardTerritory> SerializableTerritories =>
            Territories.Select(b => new BoardTerritory
            {
                OwnerName = b.Owner?.Name,
                Armies = b.Armies,
                Location = b.Location
            });

        public Territory GetTerritory(int row, int col)
        {
            return Territories.Single(t => t.Location.Row == row && t.Location.Column == col);
        }

        public Territory GetTerritory(Location location)
        {
            try
            {
                return Territories.Single(t => t.Location == location);
            }
            catch (Exception ex)
            {
                throw new TerritoryNotFoundException($"{location} does not exist in the Board", ex);
            }
        }

        public IEnumerable<Territory> GetNeighbors(Territory territory)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return Territories.Where(t => neighborLocations.Contains(t.Location));
        }

        public IEnumerable<Territory> GetContinent(Territory territory)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column),
            };
            return Territories.Where(t => neighborLocations.Contains(t.Location));
        }

        public int GetContinentBonus(IEnumerable<Territory> territories)
        {
            var bonus = 0;
            foreach(var territory in territories)
            {
                var continent = GetContinent(territory);
                var continentTerritories = territories.Where(t => continent.Contains(t));
                if(continentTerritories.Count() >=2)
                {
                    bonus += 1;
                }               
            }

            return bonus;
        }

        public bool AttackTargetLocationIsValid(Location attackSource, Location attackTarget)
        {
            int rowDistance = Math.Abs(attackSource.Row - attackTarget.Row);
            int colDistance = Math.Abs(attackSource.Column - attackTarget.Column);

            return (colDistance <= 1 && rowDistance <= 1) && (colDistance == 1 || rowDistance == 1);
        }

        internal IEnumerable<BoardTerritory> AsBoardTerritoryList()
        {
            return from t in Territories
                   select new BoardTerritory {
                       Armies = t.Armies,
                       Location = t.Location,
                       OwnerName = t.Owner?.Name
                   };
        }
    }
}
