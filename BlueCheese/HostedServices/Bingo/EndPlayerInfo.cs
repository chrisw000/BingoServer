using BlueCheese.HostedServices.Bingo.Contracts;
using System;

namespace BlueCheese.HostedServices.Bingo
{
    public class EndPlayerInfo : IEndPlayerInfo
    {
        public Guid PlayerId {get;private set;}
        public string User {get;}
        public string ConnectionId {get; set;}

        public EndPlayerInfo(string username) : this(Guid.NewGuid(), username)
        {            
        }

        public EndPlayerInfo(Guid playerId, string username)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Must supply a user", nameof(username));
            if (Guid.Empty.Equals(playerId)) throw new ArgumentException("Must supply a playerId", nameof(playerId));

            User = username;
            PlayerId = playerId;
        }
    }
}
