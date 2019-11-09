using System;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    // TODO: replace with IGamePlayerIdentity
    public interface IHoldUserIdentity
    {
        string User { get; }
        Guid PlayerId { get; }
    }
}
