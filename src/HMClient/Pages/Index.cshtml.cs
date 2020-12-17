using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Risk.HMClient.Controllers;
using System.Net.Http.Json;
using Risk.Shared;

namespace Risk.HMClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        //[HttpGet("/")]
        //public string Info()
        //{
        //    return "Submit a get request to /joinServer/{serverAddress} to join a game.";
        //}

        [HttpGet("joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync_Get(string server)
        {
            var client = httpClientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "HectoritoBonito"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{server}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("joinServer")]
        public async Task<IActionResult> JoinAsync_Post(string server)
        {
            await JoinAsync_Get(server);
            return RedirectToPage("/GameStatus", new { servername = server });
        }

        public void OnGet()
        {
        }
    }
}
