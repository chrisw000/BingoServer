using System;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IEndPlayerInfo
    {
        Guid PlayerId {get;}
        string User {get;}
        string ConnectionId {get; set;}
    }
}
