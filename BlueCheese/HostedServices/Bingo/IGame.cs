using BlueCheese.Hubs;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IGame : IGameData
    {
        Task<bool> UpdateAsync();
        Task SpawnAsync(string connectionId, NewGameStarted newGameStarting);
        Task AddPlayerAsync(JoinGame joinGame);
    }
}
