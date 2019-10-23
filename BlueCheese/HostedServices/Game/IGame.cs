using BlueCheese.Hubs;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Game
{
    public interface IGame : IGameData
    {
        Task UpdateAsync();
        Task SpawnAsync(string connectionId, NewGameStarted newGameStarting);
        Task AddPlayerAsync(string connectionId, string user);
    }
}
