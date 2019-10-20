using System;
using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public interface ILobbyHub
    {
        Task LobbyNewGameHasStarted(string user, int cheeseCount, Guid gameId);
        Task LobbyUserJoinedGame(string user, string message, Guid gameId);
        Task LobbyUpdateGame(string message);
    }
}