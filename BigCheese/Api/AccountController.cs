using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlueCheese.HostedServices.Bingo.Contracts;

namespace BigCheese.Api
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private const string Cheese = "Manchego";

        private readonly IEndPlayerManager _endPlayerManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IEndPlayerManager endPlayerManager, ILogger<AccountController> logger)
        {
            _endPlayerManager = endPlayerManager;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(string username)
        {
            if (HttpContext.Session.Get<Guid>(Cheese) == default)
            {
                if(string.IsNullOrEmpty(username))
                {
                    return BadRequest("username not supplied.");
                }

                var endPlayer = _endPlayerManager.SpawnEndPlayer(username);

                if(endPlayer==null) 
                {
                    return Forbid();
                }

                HttpContext.Session.Set<Guid>(Cheese, endPlayer.PlayerId);                
                return Ok(endPlayer);
            }

            return Ok(_endPlayerManager.GetBy(HttpContext.Session.Get<Guid>(Cheese)));
        }
    }
}
