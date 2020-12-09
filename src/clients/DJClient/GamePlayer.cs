using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Risk.Shared;

namespace DJClient
{
    public class GamePlayer
    {
        public IPlayer Player{ get; set; }

        public DeployArmyResponse DeployArmy(DeployArmyRequest deployArmyRequest)
        {


            foreach(var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName == null)
                {
                    return new DeployArmyResponse { DesiredLocation = territory.Location };
                }
            }

            foreach (var territory in deployArmyRequest.Board)
            {
                if (territory.OwnerName == Player.Name)
                {
                    return new DeployArmyResponse { DesiredLocation = territory.Location };
                }
            }

            throw new Exception("Cannot place army");
        }

        public BeginAttackResponse DecideBeginAttack(BeginAttackRequest beginAttackRequest)
        {
            var ownedTerritories = beginAttackRequest.Board.Where(t => t.OwnerName == Player.Name);
            var enemyTerritories = beginAttackRequest.Board.Where(t => t.OwnerName != null && t.OwnerName != Player.Name);

            foreach(var ownedTerritory in ownedTerritories)
            {
                foreach(var enemyTerritory in enemyTerritories)
                {
                    if (areAdjacent(ownedTerritory, enemyTerritory))
                    {
                        return new BeginAttackResponse { From = ownedTerritory.Location, To = enemyTerritory.Location };
                    }
                }
            }

            throw new Exception("Cannot attack");

        }

        public ContinueAttackResponse DecideContinueAttackResponse(ContinueAttackRequest continueAttackRequest)
        {
            return new ContinueAttackResponse { ContinueAttacking = true };
        }

        public bool areAdjacent(BoardTerritory territory1, BoardTerritory territory2)
        {
            int rowDistance = Math.Abs(territory1.Location.Row - territory2.Location.Row);
            int colDistance = Math.Abs(territory1.Location.Column - territory2.Location.Column);

            return (colDistance <= 1 && rowDistance <= 1) && (colDistance == 1 || rowDistance == 1);
        }

    }
}
