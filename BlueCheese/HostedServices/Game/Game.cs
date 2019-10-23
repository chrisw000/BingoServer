using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace BlueCheese.HostedServices.Game
{
    public class Game : IGame
    {
        public Guid GameId {get; private set;}
        public DateTime StartedUtc {get;private set;}
        public string StartedByUser {get;private set;}
        public int CheeseCount {get;private set;}
        public int GameSize {get;private set;}
        public GameStatus Status {get;private set;} = GameStatus.WaitingForPlayers;
        public int GameRound => _drawnNumbers.Count();
        public IReadOnlyList<int> NumbersDrawn => _drawnNumbers;
        public IReadOnlyList<IPlayerData> Players => _players.Values.ToList();

        private bool _isSpawned = false;

        private List<int> _gameNumbers {get; set;}
        private List<int> _drawnNumbers = new List<int>();

        private readonly ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();

        private readonly IHubContext<LobbyHub, ILobbyHub> _lobbyHubContext;
        private readonly ILogger<Game> _logger;
                
        public Game(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, ILogger<Game> logger)
        {
            _lobbyHubContext = lobbyHubContext;
            _logger = logger;            
        }

        public async Task SpawnAsync(string connectionId, NewGameStarted newGameStarting)
        {
            if (_isSpawned) throw new InvalidOperationException($"GameData {GameId} is already Spawned.");

            GameId = Guid.NewGuid();
            StartedUtc = DateTime.UtcNow;
            StartedByUser = newGameStarting.StartedByUser;
            CheeseCount = newGameStarting.CheeseCount;
            GameSize = newGameStarting.GameSize;
            _isSpawned = true;

            _gameNumbers = ThreadSafeRandom.Pick(75, 75).ToList();

            _logger.LogInformation("Spawning game {gameId} started on {connectionId} with {@newGameStarting}", GameId, connectionId, newGameStarting);
            
            await AddPlayerAsync(connectionId, newGameStarting.StartedByUser);
            await _lobbyHubContext.Clients.All.LobbyNewGameHasStarted(this).ConfigureAwait(false);
        }

        public async Task AddPlayerAsync(string connectionId, string user)
        {
            _logger.LogInformation("Adding player {user} on {connectionId} to {gameId}", user, connectionId, GameId);

            var newPlayer = new Player(connectionId, user, this.CheeseCount);

            if(_players.TryAdd(user, newPlayer))
            {
                await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, GameId.ToString());

                // Tell the player their numbers
                await _lobbyHubContext.Clients.Client(newPlayer.ConnectionId).LobbyPlayerNumbers(this, newPlayer.Numbers);
                // Tell everyone else in the game the text message version
                await _lobbyHubContext.Clients.GroupExcept(GameId.ToString(), newPlayer.ConnectionId).LobbyUserJoinedGame(this, user, $"joined game with numbers {string.Join(",", newPlayer.Numbers)}");
            }
            else
            {
                _logger.LogWarning("Unable to add player {user} on {connectionId} to {gameId}", user, connectionId, GameId);
            }
        }

        public async Task UpdateAsync()
        {
            if(Status==GameStatus.Ended) return;

            _logger.LogInformation("Update {gameId}", GameId);

            string msg;

            switch(Status)
            {
                case GameStatus.WaitingForPlayers:
                    if(_players.Count==GameSize)
                    {
                        Status = GameStatus.Playing;
                        goto case GameStatus.Playing; // YES! a goto statement for full cheese.
                    }
                    msg = $"Waiting... got {_players.Keys.Count}/{GameSize} players...";
                    break;

                case GameStatus.Playing:
                    _gameNumbers.Shuffle();

                    var number = _gameNumbers[0];
                    _gameNumbers.RemoveAt(0);
                    _drawnNumbers.Add(number);

                    var winners = string.Empty;

                    foreach(var p in _players)
                    {
                        if(p.Value.CheckNumber(number))
                        {
                            await _lobbyHubContext.Clients.Client(p.Value.ConnectionId).LobbyPlayerMessage(this, $"You have matched {number}");
                        }

                        if(p.Value.HasWon)
                        {
                            winners += $"{p.Key}! ";
                        }
                    }

                    if(winners.Length > 0) 
                    {
                        winners = $"There are winners! {winners}";
                        Status = GameStatus.Ended;
                    }

                    msg = $"-> next number {number} {winners}";
                    break;
  
                default:
                    msg = $"Unknown game status {Status:G}";
                    break;
            }

            await _lobbyHubContext.Clients.Group(GameId.ToString()).LobbyUpdateGame(this, $"time: {(DateTime.UtcNow - StartedUtc).Seconds} {msg}");
        }
    }
}
