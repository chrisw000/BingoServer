using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BlueCheese.HostedServices.Game
{
    public sealed class GameManager : AbstractHostedServiceProvider, IGameManager
    {
        private readonly GameDataFactory _gameFactory;
        private readonly ILogger<GameManager> _logger;

        private readonly ConcurrentDictionary<Guid, IGameData> _games = new ConcurrentDictionary<Guid, IGameData>();
        
        public IReadOnlyCollection<IGameData> GameData => _games.Values.OrderBy(d=>d.Started).ToList();

        public int Delay => (1000 * 1 * 1); // 1 second polling
        
        public GameManager(GameDataFactory gameFactory, ILogger<GameManager> logger)
        {
            _gameFactory = gameFactory;
            _logger = logger;
        }

        public override async Task DoPeriodicWorkAsync()
        {
            _logger.LogTrace("GameManger.DoPeriodicWorkAsync Started");
          
            foreach(var game in _games)
            {
                await game.Value.UpdateAsync();
            }

            _logger.LogTrace("GameManger.DoPeriodicWorkAsync Finished");
        }

        public async Task StartNewGameAsync(string connectionId, string user, int cheeseCount)
        {
            var newGame = await _gameFactory.SpawnNewGameAsync(connectionId, user, cheeseCount);

            if(!_games.TryAdd(newGame.Key, newGame))
            {
                _logger.LogError("unable to add new game {gameId} {newGame} for {user} on {connectionID}", newGame.Key, newGame, user, connectionId);
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
                await game.AddPlayerAsync(connectionId, user);
            }
        }
    }
}
