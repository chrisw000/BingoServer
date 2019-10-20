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

    public class GameData : IGameData
    {
        public Guid Key {get; private set;}
        public DateTime Started {get;private set;}
        public string StartedByUser {get;private set;}
        public int CheeseCount {get;private set;}

        private bool isSpawned = false;
        private bool _gameOver = false;

        private List<int> _gameNumbers {get; set;}

        private readonly ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();

        private readonly IHubContext<LobbyHub, ILobbyHub> _lobbyHubContext;
        private readonly ILogger<GameData> _logger;
                
        public GameData(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, ILogger<GameData> logger)
        {
            _lobbyHubContext = lobbyHubContext;
            _logger = logger;            
        }

        public async Task SpawnAsync(string connectionId, string user, int cheeseCount)
        {
            if (isSpawned) throw new InvalidOperationException($"GameData {Key} is already Spawned.");

            Key = Guid.NewGuid();
            Started = DateTime.UtcNow;
            StartedByUser = user;
            CheeseCount = cheeseCount;
            isSpawned = true;

            _gameNumbers = ThreadSafeRandom.Pick(75, 75).ToList();

            _logger.LogInformation("Spawning game {gameId} started by {user} on {connectionId}", Key, user, connectionId);
                                   
            await _lobbyHubContext.Clients.All.LobbyNewGameHasStarted(user, CheeseCount, Key).ConfigureAwait(false);
            await AddPlayerAsync(connectionId, user);
        }

        public async Task AddPlayerAsync(string connectionId, string user)
        {
            _logger.LogInformation("Adding player {user} on {connectionId} to {gameId}", user, connectionId, Key);

            var newPlayer =  new Player(connectionId, user, this.CheeseCount);

            if(_players.TryAdd(user, newPlayer))
            {
                await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, Key.ToString());

                await _lobbyHubContext.Clients.Group(Key.ToString()).LobbyUserJoinedGame(user, $"{connectionId} has joined game with numbers {string.Join(",", newPlayer.Numbers)}", Key);
            }
            else
            {
                _logger.LogWarning("Unable to add player {user} on {connectionId} to {gameId}", user, connectionId, Key);
            }
        }

        public async Task UpdateAsync()
        {
            if(_gameOver) return;

            _logger.LogInformation("Update {gameId}", Key);

            string status;

            if(_players.Keys.Count == 3)
            {
                _gameNumbers.Shuffle();

                var number = _gameNumbers[0];
                _gameNumbers.RemoveAt(0);

                string winners = "";

                foreach(var p in _players)
                {
                    if(p.Value.CheckNumber(number))
                    {
                        await _lobbyHubContext.Clients.Client(p.Value.ConnectionId).LobbyPlayerMessage($"You have matched {number}");
                    }

                    if(p.Value.HasWon)
                    {
                        winners += $"{p.Key}! ";
                    }
                }

                if(winners.Length > 0) 
                {
                    winners = $"There are winners! {winners}";
                    _gameOver = true;
                }

                status = $"-> next number {number} {winners}";
                
            }
            else
            {
                status = $"Waiting for 3 players... currently there are {_players.Keys.Count} players";
            }


            await _lobbyHubContext.Clients.Group(Key.ToString()).LobbyUpdateGame($"time: {(DateTime.UtcNow - Started).Seconds} {status}");
        }
    }
}
