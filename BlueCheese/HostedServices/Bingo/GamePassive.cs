using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

using BlueCheese.Hubs;
using BlueCheese.HostedServices.Bingo.Contracts;

namespace BlueCheese.HostedServices.Bingo
{
    public class GamePassive : Game, IGame
    {
        public GamePassive(IHubContext<LobbyHub, ILobbyHub> lobbyHubContext, NumberCollection numbers, ILogger<GamePassive> logger) 
            : base(lobbyHubContext, numbers, logger)
        {
        }

        protected override async Task<string> ActivePlayingAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return string.Empty;
        }

        public override async Task PushSelectionAsync(IEndPlayerInfo endPlayerInfo, int draw)
        {
            await PlayRoundAsync(draw).ConfigureAwait(false);
        }
    }
}
