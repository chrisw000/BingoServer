using BlueCheese.Hubs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo
{
    public class GameFactory
    {
        private IServiceProvider _serviceProvider;

        public GameFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IGame> SpawnNewGameAsync(string connectionId, NewGameStarted newGameStarting)
        {
            var g = _serviceProvider.GetRequiredService<IGame>();
            await g.SpawnAsync(connectionId, newGameStarting).ConfigureAwait(false);

            return g;
        }
    }
}
