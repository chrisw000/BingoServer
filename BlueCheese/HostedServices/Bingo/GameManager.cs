using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueCheese.Hubs;
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

            var newGame = await _gameFactory.SpawnNewGameAsync(connectionId, newGameStarting).ConfigureAwait(false);

            if(!_games.TryAdd(newGame.GameId, newGame))
            {
                _logger.LogError("unable to add new game {gameId} {newGame} for {user} on {connectionID}", newGame.GameId, newGame, newGameStarting.StartedByUser, connectionId);
            }
        }

        public async Task JoinGameAsync(string connectionId, string user, Guid gameId)
        {
            _games.TryGetValue(gameId, out var game);

            if (game == null)
            {
                _logger.LogWarning("unable to find game {gameId} for {user} on {connectionId}", gameId, user, connectionId);
            }
            else
            {
                await game.AddPlayerAsync(connectionId, user).ConfigureAwait(false);
            }
        }
    }
}
