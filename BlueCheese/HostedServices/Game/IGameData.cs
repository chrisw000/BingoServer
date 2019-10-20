using System;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices.Game
{
    public interface IGameData
    {
        Guid Key {get;}
        DateTime Started {get;}
        string StartedByUser {get;}
        int CheeseCount {get;}

        Task UpdateAsync();
        Task SpawnAsync(string connectionId, string user, int cheeseCount, int numberOfPlayersRequired);
        Task AddPlayerAsync(string connectionId, string user);
    }
}
