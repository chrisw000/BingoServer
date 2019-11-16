using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Globalization;
using BlueCheese.HostedServices.Bingo.Contracts;

namespace BlueCheese.HostedServices.Bingo
{
    public abstract class Game : IGame
    {
        public Guid GameId { get; private set; }
        public DateTime StartedUtc { get; private set; }
        public DateTime EndedUtc { get; protected set; }
        public string StartedByUser { get; private set; }
        public string Name { get; private set; }
        public int CheeseCount { get; private set; }
        public int Size { get; private set; }
        public GameStatus Status { get; protected set; } = GameStatus.WaitingForPlayers;
        public GameMode Mode {get; private set;}
        public int GameRound => _drawnNumbers.Count;

        public IEnumerable<IDrawData> Numbers => _allNumbers;
        public IEnumerable<IPlayerData> Players => PlayerDictionary.Values.ToList();

        protected List<int> DrawnNumbers => _drawnNumbers;
        protected IHubContext<LobbyHub, ILobbyHub> LobbyHubContext => _lobbyHubContext;
        protected NumberCollection AllNumbers => _allNumbers;
        protected ILogger<IGame> Logger => _logger;

        private bool _isSpawned = false;

        private List<int> _drawnNumbers {get;set; } = new List<int>();

        protected ConcurrentDictionary<Guid, Player> PlayerDictionary => _players;

        private readonly ConcurrentDictionary<Guid, Player> _players = new ConcurrentDictionary<Guid, Player>();

        private readonly IHubContext<LobbyHub, ILobbyHub> _lobbyHubContext;
        private readonly NumberCollection _allNumbers;
        private readonly ILogger<IGame> _logger;

        public Game(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, NumberCollection numbers, ILogger<IGame> logger)
        {
            _lobbyHubContext = lobbyHubContext;
            _allNumbers = numbers;
            _logger = logger;
        }

        public virtual async Task SpawnAsync(NewGameStarted newGameStarting)
        {
            if (newGameStarting == null) throw new ArgumentNullException(nameof(newGameStarting));
            if (_isSpawned) throw new InvalidOperationException($"GameData {GameId} is already Spawned.");

            GameId = Guid.NewGuid();
            StartedUtc = DateTime.UtcNow;
            StartedByUser = newGameStarting.User;
            CheeseCount = newGameStarting.CheeseCount;
            Size = newGameStarting.Size;
            Name = newGameStarting.Name;
            
            try {
                Mode = (GameMode)newGameStarting.Mode;
            } catch {
                Mode = GameMode.Bingo;
            }
            _isSpawned = true;

            var callerCultureInfo = new CultureInfo("en-GB"); // TODO persist to setup game culture
            _allNumbers.Generate(Mode, callerCultureInfo);

            Logger.LogInformation("Spawning game {gameId} started on {connectionId} with {@newGameStarting}", GameId, newGameStarting.ConnectionId, newGameStarting);

            var joinGame = new JoinGame(newGameStarting, GameId);
           
            await AddPlayerAsync(joinGame).ConfigureAwait(false);
            await LobbyHubContext.Clients.All.LobbyNewGameHasStarted(this).ConfigureAwait(false);
        }

        public async Task AddPlayerAsync(IEndPlayerInfo endPlayerInfo)
        {
            Logger.LogInformation("Game.AddPlayer {@endPlayerInfo}", endPlayerInfo);

            var newPlayer = (Status ==GameStatus.WaitingForPlayers)
                ? new Player(endPlayerInfo, CheeseCount, _allNumbers) 
                : new Player(endPlayerInfo);

            if (PlayerDictionary.TryAdd(endPlayerInfo.PlayerId, newPlayer))
            {
                if(Status==GameStatus.WaitingForPlayers && PlayerDictionary.Count >= Size)
                    Status = GameStatus.Playing;

                await LobbyHubContext.Groups.AddToGroupAsync(endPlayerInfo.ConnectionId, GameId.ToString()).ConfigureAwait(false);

                if(newPlayer.Status == PlayerStatus.Playing)
                {
                    // Tell the player their numbers
                    await LobbyHubContext.Clients.Client(endPlayerInfo.ConnectionId).LobbyPlayerNumbers(this, newPlayer as IPlayerData).ConfigureAwait(false);
                    // Tell everyone else in the game the text message version
                    await LobbyHubContext.Clients.GroupExcept(GameId.ToString(), endPlayerInfo.ConnectionId).LobbyUserJoinedGame(this, $"{newPlayer.Info.User} joined game with numbers {string.Join(",", newPlayer.Draws.Select(d => d.Number))}").ConfigureAwait(false);
                }
            }
            else
            {
                Logger.LogWarning("Unable to add player {user} on {connectionId} to {gameId}",
                                   newPlayer.Info.User,
                                   endPlayerInfo.ConnectionId,
                                   GameId);
            }
        }

        public async Task<bool> UpdateAsync()
        {
            Logger.LogDebug("Game.Update called {status:G} round: {round}", Status, GameRound);
                
            if (Status == GameStatus.Ended)
            {
                var removeGame = (DateTime.UtcNow - EndedUtc).TotalMinutes >= 5;
                Logger.LogDebug("Game.Ended at {endedUtc}, remove game {removeGame}", EndedUtc, removeGame);

                return removeGame;
            }

            Logger.LogInformation("Update {gameId}", GameId);

            string msg;

            switch (Status)
            {
                case GameStatus.WaitingForPlayers:
                    msg = $"Waiting... got {PlayerDictionary.Keys.Count}/{Size} players...";
                    Logger.LogDebug(msg);
                    break;

                case GameStatus.Playing:
                    msg = await ActivePlayingAsync().ConfigureAwait(false);
                    break;

                default:
                    msg = $"Unknown game status {Status:G}";
                    Logger.LogError(msg);
                    break;
            }

            await LobbyHubContext.Clients.Group(GameId.ToString())
                .LobbyUpdateGame(this, msg)
                .ConfigureAwait(false);

            return false;
        }

        public abstract Task PushSelectionAsync(IEndPlayerInfo endPlayerInfo, int draw);

        protected abstract Task<string> ActivePlayingAsync();

        protected async Task<string> PlayRoundAsync(int number)
        {
            DrawnNumbers.Add(number);
            AllNumbers[number].IsMatched(number, GameRound);

            var winners = string.Empty;

            foreach (var p in PlayerDictionary)
            {
                if (p.Value.CheckNumber(number, GameRound))
                {
                    Logger.LogDebug("{user} has matched {number}", p.Value.Info.User, AllNumbers[number].Name);
                    await LobbyHubContext.Clients.Client(p.Value.Info.ConnectionId).LobbyPlayerMessage(this, $"You have matched {AllNumbers[number].Name}").ConfigureAwait(false);
                }

                if (p.Value.Status==PlayerStatus.Winner)
                {
                    winners += $"{p.Value.Info.User}! ";
                }
            }

            if (winners.Length > 0)
            {
                foreach (var p in PlayerDictionary.Where(p=>p.Value.Status==PlayerStatus.Playing))
                {
                    p.Value.Status = PlayerStatus.Loser;
                }

                winners = $"There are winners! {winners}";
                Status = GameStatus.Ended;
                EndedUtc = DateTime.UtcNow;
                Logger.LogInformation("{status:G} {endUtc} {winners}", Status, EndedUtc, winners);
            }

            var msg = $"-> {AllNumbers[number].Name} {winners}";
            Logger.LogDebug(msg);
            return msg;
        }
    }
}
