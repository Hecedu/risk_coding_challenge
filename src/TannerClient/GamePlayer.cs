using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;
using Microsoft.Extensions.Configuration;


namespace TannerClient
{
    public class GamePlayer
    {
        public IPlayer Player { get; set; }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> board)
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
            return board.Where(t => neighborLocations.Contains(t.Location));
        }

        public DeployArmyResponse DeployArmy(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            var myTerritory = deployArmyRequest.Board.FirstOrDefault(t => t.OwnerName == Player.Name) ?? deployArmyRequest.Board.Skip(deployArmyRequest.Board.Count() / 2).First(t => t.OwnerName == null);
            response.DesiredLocation = myTerritory.Location;
            return response;
        }

        public BeginAttackResponse DecideBeginAttack(BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();

            foreach (var myTerritory in beginAttackRequest.Board.Where(t => t.OwnerName == Player.Name).OrderByDescending(t => t.Armies))
            {
                var myNeighbors = GetNeighbors(myTerritory, beginAttackRequest.Board);
                var destination = myNeighbors.Where(t => t.OwnerName != Player.Name).OrderBy(t => t.Armies).FirstOrDefault();
                if (destination != null)
                {
                    response.From = myTerritory.Location;
                    response.To = destination.Location;
                    return response;
                }
            }
            throw new Exception("No territory I can attack");

        }

        public ContinueAttackResponse DecideContinueAttackResponse(ContinueAttackRequest continueAttackRequest)
        {
            return new ContinueAttackResponse { ContinueAttacking = true };
        }

    }
}
