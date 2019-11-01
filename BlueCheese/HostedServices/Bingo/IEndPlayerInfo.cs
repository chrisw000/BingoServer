namespace BlueCheese.HostedServices.Bingo
{ 
    public interface IEndPlayerInfo : IHoldUserIdentity
    {
        string ConnectionId {get; set;}
    }
}
