using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;

namespace BlueCheese.HostedServices.Game
{

    public class GameData : IGameData
    {
        public Guid Key {get; private set;}
        public DateTime Started {get;private set;}
        public string StartedByUser {get;private set;}
        public int CheeseCount {get;private set;}

        private bool isSpawned = false;

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

            _logger.LogInformation("Spawning game {gameId} started by {user} on {connectionId}", Key, user, connectionId);

            await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, Key.ToString());
            
            await _lobbyHubContext.Clients.All.LobbyNewGameHasStarted(user, CheeseCount, Key).ConfigureAwait(false);
        }

        public async Task AddPlayerAsync(string connectionId, string user)
        {
            _logger.LogInformation("Adding player {user} on {connectionId} to {gameId}", user, connectionId, Key);

            await _lobbyHubContext.Groups.AddToGroupAsync(connectionId, Key.ToString());

            await _lobbyHubContext.Clients.Group(Key.ToString()).LobbyUserJoinedGame(user, $"{connectionId} has joined game", Key);
        }

        public async Task UpdateAsync()
        {
            _logger.LogInformation("Update {gameId}", Key);

            await _lobbyHubContext.Clients.Group(Key.ToString()).LobbyUpdateGame($"game has been running {(DateTime.UtcNow - Started).TotalSeconds} seconds");
        }
    }
}
