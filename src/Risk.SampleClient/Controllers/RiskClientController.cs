using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Risk.SampleClient.Controllers
{
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;
        private static string serverAdress;

        public RiskClientController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
        }

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            var myTerritory = deployArmyRequest.Board.FirstOrDefault(t => t.OwnerName == config["PlayerName"]) ??  deployArmyRequest.Board.Skip(deployArmyRequest.Board.Count() / 2).First(t => t.OwnerName == null);
            response.DesiredLocation = myTerritory.Location;
            return response;
        }

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

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody]BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();

            foreach(var myTerritory in beginAttackRequest.Board.Where(t => t.OwnerName == config["PlayerName"]).OrderByDescending(t => t.Armies))
            {
                var myNeighbors = GetNeighbors(myTerritory, beginAttackRequest.Board);
                var destination = myNeighbors.Where(t => t.OwnerName != config["PlayerName"]).OrderBy(t => t.Armies).FirstOrDefault();
                if(destination != null)
                {
                    response.From = myTerritory.Location;
                    response.To = destination.Location;
                    return response;
                }
            }
            throw new Exception("No territory I can attack");
        }

       

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody]GameOverRequest gameOverRequest)
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
