using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;
using System.Collections.Generic;
using System.Linq;
using BlueCheese.HostedServices.Bingo.Contracts;

namespace BlueCheese.HostedServices.Bingo
{
    public class GameActive : Game, IGame
    {
        private List<int> _gameNumbers { get; set; }

        public GameActive(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, NumberCollection numbers, ILogger<GameActive> logger) 
            : base(lobbyHubContext, numbers, logger)
        {
        }

        public override async Task SpawnAsync(NewGameStarted newGameStarting)
        {
            await base.SpawnAsync(newGameStarting).ConfigureAwait(false);
            _gameNumbers = ThreadSafeRandom.Pick(AllNumbers.CountInUse, AllNumbers.CountInUse).ToList();
        }

        protected override async Task<string> ActivePlayingAsync()
        {
            _gameNumbers.Shuffle();

            var number = _gameNumbers[0];
            _gameNumbers.RemoveAt(0);
            return await PlayRoundAsync(number).ConfigureAwait(false);
        }
        
        public override async Task PushSelectionAsync(IEndPlayerInfo endPlayerInfo, int draw)
        {
            Logger.LogWarning("{class}.{method} called when {gamemode} = {mode:G}", 
                    nameof(IGame), nameof(PushSelectionAsync), nameof(GameMode), Mode);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
