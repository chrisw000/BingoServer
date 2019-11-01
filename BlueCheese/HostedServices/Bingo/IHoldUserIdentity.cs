using System;

namespace BlueCheese.HostedServices.Bingo
{
    public interface IHoldUserIdentity
    {
        string User {get;}
        Guid PlayerId {get; }
    }
}
