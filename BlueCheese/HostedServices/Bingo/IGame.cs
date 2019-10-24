using BlueCheese.Hubs;
using System;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IGame : IGameData
    {
//        DateTime EndedUtc {get;}

        Task<bool> UpdateAsync();
        Task SpawnAsync(string connectionId, NewGameStarted newGameStarting);
        Task AddPlayerAsync(string connectionId, string user);
    }
}
