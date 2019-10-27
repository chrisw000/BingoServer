using System;

namespace BlueCheese.HostedServices.Bingo
{
    public class JoinGame
    {
        public string ConnectionId {get;set;}
        public string User {get;set;}
        public Guid GameId {get;set;}
    }
}
