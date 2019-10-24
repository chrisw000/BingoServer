using BlueCheese.HostedServices.Bingo;
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

        public async Task ClientStartedNewGame(NewGameStarted newGame)
        {
            _logger.LogInformation("starting new game {@newGame}", newGame);

            await _gameManager.StartNewGameAsync(Context.ConnectionId, newGame).ConfigureAwait(false);
        }

        public async Task ClientJoinedGame(string user, Guid gameId)
        {
            _logger.LogInformation("joining game {user} {gameId}", user, gameId);

            await _gameManager.JoinGameAsync(Context.ConnectionId, user, gameId).ConfigureAwait(false);
        }
    }
}