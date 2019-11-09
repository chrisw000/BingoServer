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
        private readonly IEndPlayerManager _endPlayerManager;
        private readonly ILogger<LobbyHub> _logger;

        public LobbyHub(IGameManager gameManager, IEndPlayerManager endPlayerManager, ILogger<LobbyHub> logger)
        {
            _gameManager = gameManager;
            _endPlayerManager = endPlayerManager;
            _logger = logger;
        }

        #region Chat
        // Send to user
        public async Task SendDirectMessage(string user, Guid id, string message)
        {
            var toUser = new EndPlayerInfo(id, user) as IHoldUserIdentity;

            if (_endPlayerManager.CheckUserAgainstId(toUser))
            { 
                var fromEndPlayer = _endPlayerManager.GetBy(Context.ConnectionId);
                if(fromEndPlayer==null)
                {
                    _logger.LogWarning("{class}.{method} cannot find fromEndPlayer for ConnectionId {connectionId}"
                        , nameof(LobbyHub), nameof(SendDirectMessage), Context.ConnectionId);
                }
                else
                {
                    await Clients.Client(_endPlayerManager.GetBy(toUser).ConnectionId).ReceiveChatMessage(fromEndPlayer as IHoldUserIdentity, message).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogWarning("{class}.{method} cannot find toEndPlayer for {toUser}"
                    , nameof(LobbyHub), nameof(SendDirectMessage), toUser);
            }
        }

        // Send to game
        public async Task SendGameMessage(Guid gameId, string message)
        {
            var fromEndPlayer = _endPlayerManager.GetBy(Context.ConnectionId);
            if(fromEndPlayer==null)
            {
                _logger.LogWarning("{class}.{method} cannot find fromEndPlayer for ConnectionId {connectionId}"
                    , nameof(LobbyHub), nameof(SendDirectMessage), Context.ConnectionId);
            }
            else
            {
                await Clients.Group(gameId.ToString()).ReceiveChatMessage(fromEndPlayer as IHoldUserIdentity, message).ConfigureAwait(false);
            }
        }

        // Send to all
        public async Task SendGlobalMessage(string message)
        {
            var fromEndPlayer = _endPlayerManager.GetBy(Context.ConnectionId);
            if(fromEndPlayer==null)
            {
                _logger.LogWarning("{class}.{method} cannot find fromEndPlayer for ConnectionId {connectionId}"
                    , nameof(LobbyHub), nameof(SendDirectMessage), Context.ConnectionId);
            }
            else
            {
                await Clients.All.ReceiveChatMessage(fromEndPlayer as IHoldUserIdentity, message).ConfigureAwait(false);
            }
        }
        #endregion

        #region Game
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

        public async Task ClientPushSelection(PushSelection pushSelection)
        {
            if(pushSelection==null) throw new ArgumentNullException(nameof(pushSelection));
            pushSelection.ConnectionId = Context.ConnectionId;

            _logger.LogInformation("LobbyHub.ClientPushSelection {@pushSelection}", pushSelection);

            await _gameManager.PushSelectionAsync(pushSelection).ConfigureAwait(false);
        }
        #endregion

        #region Connection
        public async Task ClientReconnected(IEndPlayerInfo endPlayerInfo)
        {
            if(endPlayerInfo==null) throw new ArgumentNullException(nameof(endPlayerInfo));
            endPlayerInfo.ConnectionId = Context.ConnectionId;

            _logger.LogInformation("LobbyHub.ClientReconnected {@endPlayerInfo}", endPlayerInfo);

            await _gameManager.ClientReconnectedAsync(endPlayerInfo).ConfigureAwait(false);
        }
        #endregion

    }
}