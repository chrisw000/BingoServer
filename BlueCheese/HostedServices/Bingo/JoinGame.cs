using BlueCheese.HostedServices.Bingo.Contracts;
using BlueCheese.Hubs;
using System;

namespace BlueCheese.HostedServices.Bingo
{

    public class JoinGame : IEndPlayerInfo
    {
        public string ConnectionId {get;set;}
        public string User {get;set;}
        public Guid PlayerId {get;set;}
        public Guid GameId {get;set;}

        public JoinGame(NewGameStarted newGameStarted, Guid gameId)
        {
            if(newGameStarted == null) throw new ArgumentNullException(nameof(newGameStarted));

            User = newGameStarted.User;
            ConnectionId = newGameStarted.ConnectionId;
            PlayerId = newGameStarted.PlayerId;
            GameId = gameId;
        }
    }
}
