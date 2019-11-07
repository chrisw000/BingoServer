using BlueCheese.HostedServices.Bingo.Contracts;
using System;

namespace BlueCheese.HostedServices.Bingo
{
    public class EndPlayerInfo : IEndPlayerInfo
    {
        public Guid PlayerId {get;private set;}
        public string User {get;}
        public string ConnectionId {get; set;}

        public EndPlayerInfo(Guid playerId, string user)
        {
            PlayerId = playerId;
            User = user;
        }
    }
}
