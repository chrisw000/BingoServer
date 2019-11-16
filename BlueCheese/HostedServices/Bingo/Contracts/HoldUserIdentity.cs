using System;

namespace BlueCheese.HostedServices.Bingo.Contracts
{
    public class HoldUserIdentity : IHoldUserIdentity
    {
        public string User {get;protected set;}
        public Guid PlayerId {get;protected set;}

        public HoldUserIdentity(EndPlayerInfo endPlayerInfo)
        {
            if(endPlayerInfo==null) throw new ArgumentNullException(nameof(endPlayerInfo));

            this.User = endPlayerInfo.User;
            this.PlayerId = endPlayerInfo.PlayerId;
        }
    }
}
