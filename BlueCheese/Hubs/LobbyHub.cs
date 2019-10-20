using BlueCheese.HostedServices.Game;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public class LobbyHub : Hub<ILobbyHub>
    {
        private readonly IGameManager _gameManager;
        private readonly ILogger<LobbyHub> _logger;

        public LobbyHub(IGameManager gameManager, ILogger<LobbyHub> logger)
        {
            _gameManager = gameManager;
            _logger = logger;
        }

        public async Task ClientStartedNewGame(string user, string cheeseCount)
        {
            _logger.LogInformation("starting new game {user} {cheeseCount}", user, cheeseCount);

            await _gameManager.StartNewGameAsync(Context.ConnectionId, user, int.Parse(cheeseCount));
        }

        public async Task ClientJoinedGame(string user, Guid gameId)
        {
            _logger.LogInformation("joining game {user} {gameId}", user, gameId);
            await _gameManager.JoinGameAsync(Context.ConnectionId, user, gameId);
        }
    }
}