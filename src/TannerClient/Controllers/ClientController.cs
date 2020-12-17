using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace TannerClient.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        //private static string serverAdress;
        private GamePlayer gamePlayer;

        public ClientController(IHttpClientFactory httpClientFactory, IPlayer player)
        {
            this.httpClientFactory = httpClientFactory;
            gamePlayer = new GamePlayer { Player = player };
        }

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return gamePlayer.DeployArmy(deployArmyRequest);
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return gamePlayer.DecideBeginAttack(beginAttackRequest);
        }

     

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }


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
