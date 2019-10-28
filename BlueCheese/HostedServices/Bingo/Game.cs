using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using BlueCheese.Resources;
using System.Globalization;
using System.Threading;

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
        public IReadOnlyList<string> NumberNames => _numberNames.ToList();
        public IReadOnlyList<int> NumbersDrawn => _drawnNumbers;
        public IReadOnlyList<IPlayerData> Players => _players.Values.ToList();

        private bool _isSpawned = false;

        private List<int> _gameNumbers { get; set; }
        private List<int> _drawnNumbers = new List<int>();
        private List<string> _numberNames = new List<string>();

        private readonly ConcurrentDictionary<string, Player> _players = new ConcurrentDictionary<string, Player>();

        private readonly IHubContext<LobbyHub, ILobbyHub> _lobbyHubContext;
        private readonly ILocalizerByGameMode _localizer;
        private readonly ILogger<Game> _logger;

        public Game(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, ILocalizerByGameMode localizer, ILogger<Game> logger)
        {
            _lobbyHubContext = lobbyHubContext;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task SpawnAsync(string connectionId, NewGameStarted newGameStarting)
        {
            if (newGameStarting == null) throw new ArgumentNullException(nameof(newGameStarting));
            if (_isSpawned) throw new InvalidOperationException($"GameData {GameId} is already Spawned.");

            GameId = Guid.NewGuid();
            StartedUtc = DateTime.UtcNow;
            StartedByUser = newGameStarting.StartedByUser;
            CheeseCount = newGameStarting.CheeseCount;
            Size = newGameStarting.Size;
            Name = newGameStarting.Name;
            
            try{
                Mode = (GameMode)newGameStarting.Mode;
            } catch
            {
                Mode = GameMode.Bingo;
            }
            _isSpawned = true;

            var callerCultureInfo = new CultureInfo("en-GB"); // TODO persist to setup game culture
            Thread.CurrentThread.CurrentUICulture = callerCultureInfo;
            Thread.CurrentThread.CurrentCulture = callerCultureInfo;

            _numberNames = _localizer.NamesFor(Mode);
            _gameNumbers = ThreadSafeRandom.Pick(_numberNames.Count-1, _numberNames.Count-1).ToList();

            _logger.LogInformation("Spawning game {gameId} started on {connectionId} with {@newGameStarting}", GameId, connectionId, newGameStarting);

            var joinGame = new JoinGame()
            {
                User = newGameStarting.StartedByUser,
                ConnectionId = connectionId,
                GameId = GameId
            };
            
            await AddPlayerAsync(joinGame).ConfigureAwait(false);
            await _lobbyHubContext.Clients.All.LobbyNewGameHasStarted(this).ConfigureAwait(false);
        }

        public async Task AddPlayerAsync(JoinGame joinGame)
        {
            _logger.LogInformation("Game.AddPlayer {@joinGame}", joinGame);

            var newPlayer = new Player(joinGame, CheeseCount);

            if (_players.TryAdd(newPlayer.User, newPlayer))
            {
                await _lobbyHubContext.Groups.AddToGroupAsync(newPlayer.ConnectionId, GameId.ToString()).ConfigureAwait(false);

                // Tell the player their numbers
                await _lobbyHubContext.Clients.Client(newPlayer.ConnectionId).LobbyPlayerNumbers(this, newPlayer.Numbers).ConfigureAwait(false);
                // Tell everyone else in the game the text message version
                await _lobbyHubContext.Clients.GroupExcept(GameId.ToString(), newPlayer.ConnectionId).LobbyUserJoinedGame(this, newPlayer.User, $"joined game with numbers {string.Join(",", newPlayer.Numbers)}").ConfigureAwait(false);
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
            _logger.LogDebug("Game.Update called {status:G} round: {round} {drawnCount}", Status, GameRound, NumbersDrawn.Count);
                
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

                    var winners = string.Empty;

                    foreach (var p in _players)
                    {
                        if (p.Value.CheckNumber(number))
                        {
                            _logger.LogDebug("{user} has matched {number}", p.Value.User, BallName(number));
                            await _lobbyHubContext.Clients.Client(p.Value.ConnectionId).LobbyPlayerMessage(this, $"You have matched {BallName(number)}").ConfigureAwait(false);
                        }

                        if (p.Value.HasWon)
                        {
                            winners += $"{p.Key}! ";
                        }
                    }

                    if (winners.Length > 0)
                    {
                        winners = $"There are winners! {winners}";
                        Status = GameStatus.Ended;
                        EndedUtc = DateTime.UtcNow;
                        _logger.LogInformation("{status:G} {endUtc} {winners}", Status, EndedUtc, winners);
                    }

                    msg = $"-> {BallName(number)} {winners}";

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

        private string BallName(int number)
        {
            try
            {
                return $"{_numberNames[number]}";
            }
            catch
            {
                _logger.LogWarning("Cant find name for {number}", number);
                return $"{number}";
            }
        }
    }
}
