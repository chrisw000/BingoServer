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

        //TODO: remove these 2
        public Guid PlayerId => _userIdentity.PlayerId;
        public string User => _userIdentity.User;

        public bool HasWon => _cheeseCount == _draws.Count(d=>d.Matched==true);

        public Player(IEndPlayerInfo userIdentity, int cheeseCount, NumberCollection numbers)
        {
            if(userIdentity==null) throw new ArgumentNullException(nameof(userIdentity));
            if(numbers==null) throw new ArgumentNullException(nameof(numbers));
            if(cheeseCount>=numbers.Count) throw new ArgumentOutOfRangeException(nameof(cheeseCount), "You can't have more cheese than there is in the game.");
            if(cheeseCount<=0) throw new ArgumentOutOfRangeException(nameof(cheeseCount), "You need some cheese in the game to be a player.");

            _userIdentity = userIdentity;

            _cheeseCount = cheeseCount;
            _draws.AddRange(from i in ThreadSafeRandom.Pick(cheeseCount, numbers.CountInUse)
                            select new Draw(i, numbers[i].Name));
        }

        public bool CheckNumber(int number, int gameRound) {

            //return _draws.Exists(d=>d.IsMatched(number));
            foreach(var d in _draws)
            {
                if (d.IsMatched(number, gameRound))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
