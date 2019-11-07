using BlueCheese.HostedServices.Bingo.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueCheese.HostedServices.Bingo
{

    public class Player : IPlayer
    {
        private readonly IEndPlayerInfo _userIdentity;
        private readonly int _cheeseCount;
        private List<Draw> _draws = new List<Draw>();
        
        public IEnumerable<IDrawData> Draws => _draws;

        public IEndPlayerInfo Info => _userIdentity;
        IHoldUserIdentity IPlayerData.Info => Info as IHoldUserIdentity;

        public PlayerStatus Status {get; internal set;}

        public Player(IEndPlayerInfo userIdentity)
        {
            if(userIdentity==null) throw new ArgumentNullException(nameof(userIdentity));
            _userIdentity = userIdentity;
            Status = PlayerStatus.Spectator;
        }

        public Player(IEndPlayerInfo userIdentity, int cheeseCount, NumberCollection numbers)
        {
            if(userIdentity==null) throw new ArgumentNullException(nameof(userIdentity));
            if(numbers==null) throw new ArgumentNullException(nameof(numbers));
            if(cheeseCount>=numbers.Count) throw new ArgumentOutOfRangeException(nameof(cheeseCount), "You can't have more cheese than there is in the game.");
            if(cheeseCount<=0) throw new ArgumentOutOfRangeException(nameof(cheeseCount), "You need some cheese in the game to be a player.");

            _userIdentity = userIdentity;
            Status = PlayerStatus.Playing;

            _cheeseCount = cheeseCount;
            _draws.AddRange(from i in ThreadSafeRandom.Pick(cheeseCount, numbers.CountInUse)
                            select new Draw(i, numbers[i].Name));
        }

        public bool CheckNumber(int number, int gameRound) {

            foreach(var d in _draws)
            {
                if (d.IsMatched(number, gameRound))
                {
                    if(_cheeseCount == _draws.Count(d=>d.Matched==true))
                    {
                        Status = PlayerStatus.Winner;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
