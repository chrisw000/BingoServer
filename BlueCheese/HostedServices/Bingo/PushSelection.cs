using System;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IGamePlayerIdentity
    {
        string ConnectionId {get;set;}
        Guid GameId {get;set;}
    }

    public class PushSelection : IGamePlayerIdentity
    {
        public string ConnectionId {get;set;}
        public Guid GameId {get;set;}
        public int Draw {get;set;}
    }
}
