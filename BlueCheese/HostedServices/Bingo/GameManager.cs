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
    public interface IEndPlayerInfo
    {
        Guid PlayerId {get;}
        String User {get;}
    }

    public class EndPlayerInfo : IEndPlayerInfo
    {
        public Guid PlayerId {get;private set;}
        public string User {get;}

        public EndPlayerInfo(Guid playerId, string user)
        {
            PlayerId = playerId;
            User = user;
        }
    }

    public sealed class GameManager : AbstractHostedServiceProvider, IGameManager
    {
        private readonly GameFactory _gameFactory;
        private readonly ILogger<GameManager> _logger;

        private readonly ConcurrentDictionary<Guid, EndPlayerInfo> _players = new ConcurrentDictionary<Guid, EndPlayerInfo>();
        private readonly ConcurrentDictionary<string, Guid> _playerUsernames = new ConcurrentDictionary<string, Guid>();

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

        public async Task<IGame> StartNewGameAsync(string connectionId, NewGameStarted newGameStarting)
        {
            if(newGameStarting==null) throw new ArgumentNullException(nameof(newGameStarting));
            _logger.LogTrace("GameManager.StartNewGame {newGameStarting}", newGameStarting);

            // Move to object to track these?
            // Check the user / playerid combo
            if(_playerUsernames.TryGetValue(newGameStarting.StartedByUser, out var id))
            {
                if(id!=newGameStarting.PlayerId)
                {
                    return null; // TODO
                }
                if(_players.TryGetValue(id, out var endPlayer))
                {
                    if(id!=endPlayer.PlayerId) // this 
                    {
                        return null; // TODO
                    }
                }
            }
            else
            {
                return null;
            }
            
            var newGame = await _gameFactory.SpawnNewGameAsync(connectionId, newGameStarting).ConfigureAwait(false);

            if (_games.TryAdd(newGame.GameId, newGame))
            {
                _logger.LogDebug("GameManager.StartNewGame {newGame}", newGame);
            }
            else
            {
                _logger.LogError("unable to add new game {gameId} {newGame} for {user} on {connectionID}", newGame.GameId, newGame, newGameStarting.StartedByUser, connectionId);
            }

            return newGame;
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

        public Guid GeneratePlayerId(string username)
        {
            var id = Guid.NewGuid();
            var endPlayerInfo = new EndPlayerInfo(id, username);

            if(_playerUsernames.TryAdd(username, endPlayerInfo.PlayerId))
            {
                if(_players.TryAdd(endPlayerInfo.PlayerId, endPlayerInfo)) // TODO identity id & connection id
                {
                    return id;  
                }
                else
                {
                    _playerUsernames.TryRemove(username, out _);
                }
            }
            
            return Guid.Empty;
        }
    }
}
