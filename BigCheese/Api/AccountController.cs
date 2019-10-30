using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlueCheese.HostedServices.Bingo;

namespace BigCheese.Api
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private const string Cheese = "Manchego";

        private readonly IGameManager _gameManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IGameManager gameManager, ILogger<AccountController> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Post(string lobbyUsername)
        {    
            if (HttpContext.Session.Get<Guid>(Cheese) == default)
            {
                var id = _gameManager.GeneratePlayerId(lobbyUsername);

                if(id!=Guid.Empty) 
                {
                    HttpContext.Session.Set<Guid>(Cheese, id);                
                    return Ok(id);
                }
            }

            // Either you're trying to select a username twice, or the username is already taken
            return Forbid(); 
        }
    }
}
