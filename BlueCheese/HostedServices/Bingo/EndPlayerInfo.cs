using BlueCheese.HostedServices.Bingo.Contracts;
using System;

namespace BlueCheese.HostedServices.Bingo
{
    public class EndPlayerInfo : IEndPlayerInfo
    {
        public string User {get;protected set;}
        public Guid PlayerId {get;protected set;}

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

        public static implicit operator HoldUserIdentity(EndPlayerInfo info)
        {
            return new HoldUserIdentity(info);
        }

        public HoldUserIdentity ToHoldUserIdentity()
        {
            return new HoldUserIdentity(this);
        }
    }
}
