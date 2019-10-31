using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Globalization;

namespace BlueCheese.HostedServices.Bingo
{
    public class Game : IGame
    {
        public Guid GameId { get; private set; }
        public DateTime StartedUtc { get; private set; }
        public DateTime EndedUtc { get; private set; }
        public string StartedByUser { get; private set; }
        public string Name { get; private set; }
        public int CheeseCount { get; private set; }
        public int Size { get; private set; }
        public GameStatus Status { get; private set; } = GameStatus.WaitingForPlayers;
        public GameMode Mode {get; private set;}
        public int GameRound => _drawnNumbers.Count;

        public IEnumerable<IDrawData> Numbers => _allNumbers;
        public IEnumerable<IPlayerData> Players => _players.Values.ToList();

        private bool _isSpawned = false;

        private List<int> _gameNumbers { get; set; }
        private List<int> _drawnNumbers = new List<int>();

        private readonly ConcurrentDictionary<Guid, Player> _players = new ConcurrentDictionary<Guid, Player>();

        private readonly IHubContext<LobbyHub, ILobbyHub> _lobbyHubContext;
        private readonly NumberCollection _allNumbers;
        private readonly ILogger<Game> _logger;

        public Game(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, NumberCollection numbers, ILogger<Game> logger)
        {
            _lobbyHubContext = lobbyHubContext;
            _allNumbers = numbers;
            _logger = logger;
        }

        public async Task SpawnAsync(NewGameStarted newGameStarting)
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
            _gameNumbers = ThreadSafeRandom.Pick(_allNumbers.CountInUse, _allNumbers.CountInUse).ToList();

            _logger.LogInformation("Spawning game {gameId} started on {connectionId} with {@newGameStarting}", GameId, newGameStarting.ConnectionId, newGameStarting);

            var joinGame = new JoinGame()
            {
                User = newGameStarting.User,
                ConnectionId = newGameStarting.ConnectionId,
                GameId = GameId
            };
            
            await AddPlayerAsync(joinGame).ConfigureAwait(false);
            await _lobbyHubContext.Clients.All.LobbyNewGameHasStarted(this).ConfigureAwait(false);
        }

        public async Task AddPlayerAsync(JoinGame joinGame)
        {
            _logger.LogInformation("Game.AddPlayer {@joinGame}", joinGame);

            var newPlayer = new Player(joinGame, CheeseCount, _allNumbers);

            if (_players.TryAdd(newPlayer.PlayerId, newPlayer))
            {
                await _lobbyHubContext.Groups.AddToGroupAsync(newPlayer.ConnectionId, GameId.ToString()).ConfigureAwait(false);

                // Tell the player their numbers
                await _lobbyHubContext.Clients.Client(newPlayer.ConnectionId).LobbyPlayerNumbers(this, newPlayer as IPlayerData).ConfigureAwait(false);
                // Tell everyone else in the game the text message version
                await _lobbyHubContext.Clients.GroupExcept(GameId.ToString(), newPlayer.ConnectionId).LobbyUserJoinedGame(this, $"{newPlayer.User} joined game with numbers {string.Join(",", newPlayer.Draws.Select(d=>d.Number))}").ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning("Unable to add player {user} on {connectionId} to {gameId}",
                                   newPlayer.User,
                                   newPlayer.ConnectionId,
                                   GameId);
            }
        }

        public async Task<bool> UpdateAsync()
        {
            _logger.LogDebug("Game.Update called {status:G} round: {round}", Status, GameRound);
                
            if (Status == GameStatus.Ended)
            {
                var removeGame = (DateTime.UtcNow - EndedUtc).TotalMinutes >= 5;
                _logger.LogDebug("Game.Ended at {endedUtc}, remove game {removeGame}", EndedUtc, removeGame);

                return removeGame;
            }

            _logger.LogInformation("Update {gameId}", GameId);

            string msg;

            switch (Status)
            {
                case GameStatus.WaitingForPlayers:
                    if (_players.Count == Size)
                    {
                        Status = GameStatus.Playing;
                        goto case GameStatus.Playing; // YES! a goto statement for full cheese.
                    }
                    msg = $"Waiting... got {_players.Keys.Count}/{Size} players...";
                    _logger.LogDebug(msg);
                    break;

                case GameStatus.Playing:
                    _gameNumbers.Shuffle();

                    var number = _gameNumbers[0];
                    _gameNumbers.RemoveAt(0);
                    _drawnNumbers.Add(number);
                    _allNumbers[number].IsMatched(number, GameRound);

                    var winners = string.Empty;

                    foreach (var p in _players)
                    {
                        if (p.Value.CheckNumber(number, GameRound))
                        {
                            _logger.LogDebug("{user} has matched {number}", p.Value.User, _allNumbers[number].Name);
                            await _lobbyHubContext.Clients.Client(p.Value.ConnectionId).LobbyPlayerMessage(this, $"You have matched {_allNumbers[number].Name}").ConfigureAwait(false);
                        }

                        if (p.Value.HasWon)
                        {
                            winners += $"{p.Value.User}! ";
                        }
                    }

                    if (winners.Length > 0)
                    {
                        winners = $"There are winners! {winners}";
                        Status = GameStatus.Ended;
                        EndedUtc = DateTime.UtcNow;
                        _logger.LogInformation("{status:G} {endUtc} {winners}", Status, EndedUtc, winners);
                    }

                    msg = $"-> {_allNumbers[number].Name} {winners}";

                    break;

                default:
                    msg = $"Unknown game status {Status:G}";
                    break;
            }

            await _lobbyHubContext.Clients.Group(GameId.ToString())
                .LobbyUpdateGame(this, $"time: {(int)(DateTime.UtcNow - StartedUtc).TotalSeconds} {msg}")
                .ConfigureAwait(false);

            return false;
        }
    }
}
