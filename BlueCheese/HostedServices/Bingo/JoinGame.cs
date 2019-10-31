using System;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IHoldUserIdentity
    {
        string User {get;set;}
        Guid PlayerId {get;set;}
    }

    public class JoinGame : IHoldUserIdentity
    {
        public string ConnectionId {get;set;}
        public string User {get;set;}
        public Guid PlayerId {get;set;}
        public Guid GameId {get;set;}
    }
}
