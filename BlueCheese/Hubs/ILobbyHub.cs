using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlueCheese.Hubs
{
    public interface ILobbyHub
    {
        Task LobbyNewGameHasStarted(string user, int cheeseCount, Guid gameId);
        Task LobbyUserJoinedGame(string user, string message, Guid gameId);
        Task LobbyUpdateGame(Guid gameId, string message);
        Task LobbyPlayerMessage(Guid gameId, string message);
        Task LobbyPlayerNumbers(Guid gameId, IReadOnlyList<int> numbers);
    }
}