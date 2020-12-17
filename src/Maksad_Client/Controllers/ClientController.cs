using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Risk.Shared;

namespace Maksad_Client.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        //private static string serverName = "http://localhost:5000";

        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;

        }


        [HttpGet("AreYouThere")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return createDeployResponse(deployArmyRequest);
        }

        private DeployArmyResponse createDeployResponse(DeployArmyRequest deployArmyRequest)
        {

            Location attacklocation = new Location();

            //DeployArmyResponse response = new DeployArmyResponse();

            foreach (BoardTerritory space in deployArmyRequest.Board)
            {
                if ((space.OwnerName == null || space.OwnerName == "Maksad") && space.Armies < 2)
                {
                    attacklocation = space.Location;
                    break;
                }
                else
                {
                    continue;
                }

            }
            
            return new DeployArmyResponse { DesiredLocation = attacklocation };

        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return createAttackResponse(beginAttackRequest);
        }
        private BeginAttackResponse createAttackResponse(BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();
            var attackerLocation = new Location();
            var neighbour = new BoardTerritory();
            //from is the attacker to is the defender
            foreach (BoardTerritory space in beginAttackRequest.Board)
            {
                if (space.OwnerName == "Maksad")
                {
                    attackerLocation = new Location(space.Location.Row, space.Location.Column);
                    
    
                    for (int i = space.Location.Column - 1; i <= (space.Location.Column + 1); i++)
                    {
                        for (int j = space.Location.Row - 1; j <= (space.Location.Row + 1); j++)
                        {
                            if (j < 0)
                            {
                                continue;
                            }
                            
 
                            neighbour = beginAttackRequest.Board.FirstOrDefault(t => t.Location == new Location(i,j));

                            if (neighbour != null && neighbour.OwnerName != "Maksad" && neighbour.Armies >= 1)
                            {
                                response.From = attackerLocation;
                                response.To = neighbour.Location;
                                return response;
                            }
                        }
                    }

                }
            }
            return null;
        }


        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }


        //The next two functions handle pacifism randomly.
        [HttpPost("beginAction")]
        public ActionResponse BeginAction([FromBody] ActionRequest actionRequest)
        {
            return createActionResponse(actionRequest);
        }
        private ActionResponse createActionResponse(ActionRequest actionRequest)
        {
            Random rnd = new Random();
            ActionResponse response = new ActionResponse();
            if (rnd.Next(1, 3) == 1)
            {
                response.userAction = UserAction.Attack;
            }
            else
            {
                response.userAction = UserAction.Pacifism;
            }
            return response;

        }


        //The next two functions handle continue attacking randomly.
        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            return createContinueAttackResponse(continueAttackRequest);
        }
        private ContinueAttackResponse createContinueAttackResponse(ContinueAttackRequest continueAttackRequest)
        {
            Random rnd = new Random();
            ContinueAttackResponse response = new ContinueAttackResponse();
            if (rnd.Next(1, 3) == 1)
            {
                response.ContinueAttacking = false;
            }
            else
            {
                response.ContinueAttacking = true;
            }
            return response;

        }
    }
}
