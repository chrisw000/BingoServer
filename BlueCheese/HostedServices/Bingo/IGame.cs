using BlueCheese.Hubs;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IGame : IGameData
    {
        Task<bool> UpdateAsync();
        Task SpawnAsync(NewGameStarted newGameStarting);
        Task AddPlayerAsync(IEndPlayerInfo endPlayerInfo);
    }
}
