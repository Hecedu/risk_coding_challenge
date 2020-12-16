using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Risk.Shared;


namespace VisualizerController.Controllers
{
    public class VisualizeController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        

        public VisualizeController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;

        }


        [HttpGet("AreYouThere")]
        public string AreYouThere()
        {
            return "yes";
        }


        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
