using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueCheese.HostedServices.Bingo.Contracts;
using BlueCheese.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BlueCheese.HostedServices.Bingo
{
    public sealed class GameManager : AbstractHostedServiceProvider, IGameManager
    {
        private readonly GameFactory _gameFactory;
        private readonly IEndPlayerManager _endPlayerManager;
        private readonly IHubContext<LobbyHub, ILobbyHub> _lobbyHubContext;
        private readonly ILogger<GameManager> _logger;

        private readonly ConcurrentDictionary<Guid, IGame> _games = new ConcurrentDictionary<Guid, IGame>();
        
        public IEnumerable<IGameData> GameData => _games.Values.OrderBy(d=>d.StartedUtc).ToList();

        public int Delay => (1000 * 1 * 1); // 1 second polling

        public GameManager(GameFactory gameFactory, IEndPlayerManager endPlayerManager, IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, ILogger<GameManager> logger)
        {
            _gameFactory = gameFactory;
            _endPlayerManager = endPlayerManager;
            _lobbyHubContext = lobbyHubContext;
            _logger = logger;
        }

        public override async Task DoPeriodicWorkAsync()
        {
            _logger.LogTrace("GameManger.DoPeriodicWorkAsync Started");
          
            foreach(var game in _games)
            {
                if(await game.Value.UpdateAsync().ConfigureAwait(false))
                {
                    _games.TryRemove(game.Key, out var removed);
                }
            }

            _logger.LogTrace("GameManger.DoPeriodicWorkAsync Finished");
        }

        public async Task<IGame> StartNewGameAsync(NewGameStarted newGameStarting)
        {
            if(newGameStarting==null) throw new ArgumentNullException(nameof(newGameStarting));
            _logger.LogTrace("GameManager.StartNewGame {newGameStarting}", newGameStarting);

            if(!_endPlayerManager.CheckUserAgainstId(newGameStarting))
                return null; // TODO - return some error state?
            
            var newGame = await _gameFactory.SpawnNewGameAsync(newGameStarting).ConfigureAwait(false);

            if (_games.TryAdd(newGame.GameId, newGame))
            {
                _logger.LogDebug("GameManager.StartNewGame {newGame}", newGame);
            }
            else
            {
                _logger.LogError("unable to add new game {gameId} {newGame} for {user} on {connectionID}", newGame.GameId, newGame, newGameStarting.User, newGameStarting.ConnectionId);
            }

            return newGame;
        }

        public async Task JoinGameAsync(JoinGame joinGame)
        {
            if(joinGame==null) throw new ArgumentNullException(nameof(joinGame));

            _logger.LogTrace("GameManager.JoinGame {@joinGame}", joinGame);

            if(!_endPlayerManager.CheckUserAgainstId(joinGame))
                return; // TODO - return some error state?

            var endPlayerInfo = _endPlayerManager.StoreConnection(joinGame);

            if(_games.TryGetValue(joinGame.GameId, out var game))
            {
                await game.AddPlayerAsync(endPlayerInfo).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("GameManager.JoinGame unable to join game {@joinGame}", joinGame);
            }
        }
        
        public async Task PushSelectionAsync(PushSelection pushSelection)
        {
            if(pushSelection==null) throw new ArgumentNullException(nameof(pushSelection));

            _logger.LogTrace("GameManager.PushSelection {@pushSelection}", pushSelection);

            var endPlayerInfo = _endPlayerManager.GetByConnection(pushSelection.ConnectionId);

            if(endPlayerInfo==null)
                return; // TODO - return some error state?

            if(_games.TryGetValue(pushSelection.GameId, out var game))
            {
                await game.PushSelectionAsync(endPlayerInfo, pushSelection.Draw).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("GameManager.PushSelection unable to find game {@pushSelection}", pushSelection);
            }
        }

        public async Task ClientReconnectedAsync(IEndPlayerInfo endPlayerInfo)
        {
            if(endPlayerInfo==null) throw new ArgumentNullException(nameof(endPlayerInfo));

            _logger.LogTrace("GameManager.ClientReconnected {@endPlayerInfo}", endPlayerInfo);

            if(!_endPlayerManager.CheckUserAgainstId(endPlayerInfo))
                return; // TODO - return some error state?

            foreach (var (g, p) in from g in _games.Values
                                   from p in g.Players.Where(p => p.Info.PlayerId == endPlayerInfo.PlayerId)
                                   select (g, p))
            {
                await _lobbyHubContext.Groups.RemoveFromGroupAsync(((IEndPlayerInfo)p).ConnectionId, g.GameId.ToString()).ConfigureAwait(false);
            }

            _endPlayerManager.StoreConnection(endPlayerInfo);

            foreach (var (g, p) in from g in _games.Values
                                   from p in g.Players.Where(p => p.Info.PlayerId == endPlayerInfo.PlayerId)
                                   select (g, p))
            {
                await _lobbyHubContext.Groups.AddToGroupAsync(((IEndPlayerInfo)p).ConnectionId, g.GameId.ToString()).ConfigureAwait(false);
            }
        }
    }
}
