using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Risk.HMClient.Controllers
{
    public class ClientController : Controller
    {

        private readonly IHttpClientFactory clientFactory;
        //private static string serverName = "http://localhost:5000";

        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;

        }
        private static string serverAdress;

        [HttpGet("joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync(string server)
        {
            serverAdress = server;
            var client = clientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "braindead client"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAdress}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("joinServer")]
        public async Task<IActionResult> JoinAsync_Post(string server)
        {
            await JoinAsync(server);
            return RedirectToPage("/GameStatus", new { servername = server });
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
                if ((space.OwnerName == null || space.OwnerName == "HectoritoBonito") && space.Armies < 2)
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
        [HttpPost("beginAction")]
        public ActionResponse BeginAction ([FromBody] ActionRequest actionRequest)
        {
            return createActionResponse(actionRequest);
        }
        private ActionResponse createActionResponse (ActionRequest actionRequest)
        {
            Random rnd = new Random();
            ActionResponse response = new ActionResponse();
            if (rnd.Next(1,3) == 1)
            {
                response.userAction = UserAction.Attack;
            }
            else
            {
                response.userAction = UserAction.Pacifism;
            }
            return response;

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
                if (space.OwnerName == "HectoritoBonito")
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


                            neighbour = beginAttackRequest.Board.FirstOrDefault(t => t.Location == new Location(i, j));

                            if (neighbour != null && neighbour.OwnerName != "HectoritoBonito" && neighbour.Armies >= 1)
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

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
