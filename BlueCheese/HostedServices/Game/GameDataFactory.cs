using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Game
{
    public class GameDataFactory
    {
        private IServiceProvider _serviceProvider;

        public GameDataFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IGameData> SpawnNewGameAsync(string connectionId, string user, int cheeseCount)
        {
            var g = _serviceProvider.GetRequiredService<IGameData>();
            await g.SpawnAsync(connectionId, user, cheeseCount);

            return g;
        }
    }
}
