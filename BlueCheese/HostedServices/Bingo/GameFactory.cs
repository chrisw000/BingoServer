using BlueCheese.HostedServices.Bingo.Contracts;
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

        public async Task<IGame> SpawnNewGameAsync(NewGameStarted newGameStarting)
        {
            if(newGameStarting==null) throw new ArgumentNullException(nameof(newGameStarting));

            _logger.LogTrace("GameFactory.SpawnNewGame {newGameStarting}", newGameStarting);

            IGame g;

            if(newGameStarting.Mode == (int)GameMode.Cheesy)
            {
                g = _serviceProvider.GetRequiredService<GamePassive>();
            }
            else
            {
                g = _serviceProvider.GetRequiredService<GameActive>();
            }

            await g.SpawnAsync(newGameStarting).ConfigureAwait(false);

            return g;
        }
    }
}
