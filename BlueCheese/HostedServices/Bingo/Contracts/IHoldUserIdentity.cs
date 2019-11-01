using System;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IHoldUserIdentity
    {
        string User { get; }
        Guid PlayerId { get; }
    }
}
