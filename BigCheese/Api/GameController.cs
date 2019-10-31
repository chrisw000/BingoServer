using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlueCheese.HostedServices.Bingo;

namespace BigCheese.Api
{
    [Route("api/[controller]")]
    public class GameController : Controller
    {
        private readonly IGameManager _gameManager;
        private readonly ILogger<GameController> _logger;

        public GameController(IGameManager gameManager, ILogger<GameController> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {      
            // for some reason this only returns values if there is a .Where clause on it....
            return Ok(_gameManager.GameData.Where(g=>g.StartedUtc > DateTime.MinValue));
        }

        [HttpGet("{status}")]
        public IActionResult Get(GameStatus status)
        {
            return Ok(_gameManager.GameData.Where(g=>g.Status == status));
        }
    }
}
