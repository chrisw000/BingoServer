using BlueCheese.HostedServices.Bingo.Contracts;
using System;

namespace BlueCheese.HostedServices.Bingo
{

    public class JoinGame : IEndPlayerInfo
    {
        public string ConnectionId {get;set;}
        public string User {get;set;}
        public Guid PlayerId {get;set;}
        public Guid GameId {get;set;}
    }
}
