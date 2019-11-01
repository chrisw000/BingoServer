using BlueCheese.HostedServices.Bingo;
using BlueCheese.HostedServices.Bingo.Contracts;
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
            if(newGame==null) throw new ArgumentNullException(nameof(newGame));
            newGame.ConnectionId = Context.ConnectionId;

            _logger.LogInformation("LobbyHub.ClientStartedNewGame {@newGame}", newGame);

            await _gameManager.StartNewGameAsync(newGame).ConfigureAwait(false);
        }

        public async Task ClientJoinedGame(JoinGame joinGame)
        {
            if(joinGame==null) throw new ArgumentNullException(nameof(joinGame));
            joinGame.ConnectionId = Context.ConnectionId;

            _logger.LogInformation("LobbyHub.ClientJoinedGame {@joinGame}", joinGame);

            await _gameManager.JoinGameAsync(joinGame).ConfigureAwait(false);
        }

        public async Task ClientReconnected(IEndPlayerInfo endPlayerInfo)
        {
            if(endPlayerInfo==null) throw new ArgumentNullException(nameof(endPlayerInfo));
            endPlayerInfo.ConnectionId = Context.ConnectionId;

            _logger.LogInformation("LobbyHub.ClientReconnected {@endPlayerInfo}", endPlayerInfo);

            await _gameManager.ClientReconnectedAsync(endPlayerInfo).ConfigureAwait(false);
        }

    }
}