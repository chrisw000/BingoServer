using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueCheese.HostedServices.Bingo
{
    public class Player : IPlayer
    {
        private readonly int _cheeseCount;
        private List<Draw> _draws = new List<Draw>();

        internal string ConnectionId {get;}
        
        public IEnumerable<IDrawData> Draws => _draws;

        public Guid PlayerId {get;}
        public string User {get;}
        public bool HasWon => _cheeseCount == _draws.Count(d=>d.Matched==true);

        public Player(JoinGame joinGame, int cheeseCount, NumberCollection numbers)
        {
            if(joinGame==null) throw new ArgumentNullException(nameof(joinGame));
            if(numbers==null) throw new ArgumentNullException(nameof(numbers));
            if(cheeseCount>=numbers.Count) throw new ArgumentOutOfRangeException(nameof(cheeseCount), "You can't have more cheese than there is in the game.");
            if(cheeseCount<=0) throw new ArgumentOutOfRangeException(nameof(cheeseCount), "You need some cheese in the game to be a player.");

            PlayerId = Guid.NewGuid();
            ConnectionId = joinGame.ConnectionId;
            User = joinGame.User;

            _cheeseCount = cheeseCount;
            _draws.AddRange(from i in ThreadSafeRandom.Pick(cheeseCount, numbers.CountInUse)
                            select new Draw(i, numbers[i].Name));
        }

        public bool CheckNumber(int number) {

            //return _draws.Exists(d=>d.IsMatched(number));
            foreach(var d in _draws)
            {
                if (d.IsMatched(number))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
