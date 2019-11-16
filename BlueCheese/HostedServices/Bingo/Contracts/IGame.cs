using BlueCheese.Hubs;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IGame : IGameData
    {
        Task<bool> UpdateAsync();
        Task SpawnAsync(NewGameStarted newGameStarting);
        Task AddPlayerAsync(IEndPlayerInfo endPlayerInfo);
        Task PushSelectionAsync(IEndPlayerInfo endPlayerInfo, int draw);
    }
}
