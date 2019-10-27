using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueCheese.Hubs;
using EnsureThat;
using Microsoft.Extensions.Logging;

namespace BlueCheese.HostedServices.Bingo
{
    public sealed class GameManager : AbstractHostedServiceProvider, IGameManager
    {
        private readonly GameFactory _gameFactory;
        private readonly ILogger<GameManager> _logger;

        private readonly ConcurrentDictionary<Guid, IGame> _games = new ConcurrentDictionary<Guid, IGame>();
        
        public IEnumerable<IGameData> GameData => _games.Values.OrderBy(d=>d.StartedUtc).ToList();

        public int Delay => (1000 * 1 * 1); // 1 second polling
        
        public GameManager(GameFactory gameFactory, ILogger<GameManager> logger)
        {
            _gameFactory = gameFactory;
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

        public async Task StartNewGameAsync(string connectionId, NewGameStarted newGameStarting)
        {
            if(newGameStarting==null) throw new ArgumentNullException(nameof(newGameStarting));
            _logger.LogTrace("GameManager.StartNewGame {newGameStarting}", newGameStarting);
            
            var newGame = await _gameFactory.SpawnNewGameAsync(connectionId, newGameStarting).ConfigureAwait(false);

            if (_games.TryAdd(newGame.GameId, newGame))
            {
                _logger.LogDebug("GameManager.StartNewGame {newGame}", newGame);
            }
            else
            {
                _logger.LogError("unable to add new game {gameId} {newGame} for {user} on {connectionID}", newGame.GameId, newGame, newGameStarting.StartedByUser, connectionId);
            }
        }

        public async Task JoinGameAsync(JoinGame joinGame)
        {
            if(joinGame==null) throw new ArgumentNullException(nameof(joinGame));

            _logger.LogTrace("GameManager.JoinGame {@joinGame}", joinGame);

            if(_games.TryGetValue(joinGame.GameId, out var game))
            {
                await game.AddPlayerAsync(joinGame).ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("GameManager.JoinGame unable to join game {@joinGame}", joinGame);
            }
        }
    }
}
