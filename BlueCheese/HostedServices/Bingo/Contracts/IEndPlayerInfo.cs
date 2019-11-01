namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public interface IEndPlayerInfo : IHoldUserIdentity
    {
        string ConnectionId { get; set; }
    }
}
