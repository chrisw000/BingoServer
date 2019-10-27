using BlueCheese.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo
{
    public class GameFactory
    {
        private IServiceProvider _serviceProvider;
        private ILogger<GameFactory> _logger;

        public GameFactory(IServiceProvider serviceProvider, ILogger<GameFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<IGame> SpawnNewGameAsync(string connectionId, NewGameStarted newGameStarting)
        {
            _logger.LogTrace("GameFactory.SpawnNewGame {newGameStarting}", newGameStarting);

            var g = _serviceProvider.GetRequiredService<IGame>();
            await g.SpawnAsync(connectionId, newGameStarting).ConfigureAwait(false);

            return g;
        }
    }
}
