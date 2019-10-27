using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueCheese.HostedServices.Bingo
{
    public class Player : IPlayer
    {
        private readonly int _cheeseCount;
        private int _matchCount;

        public IReadOnlyList<int> Numbers {get;}
        public string ConnectionId {get;}
        public string User {get;}
        public bool HasWon => _cheeseCount == _matchCount;

        public Player(JoinGame joinGame, int cheeseCount)
        {
            if(joinGame==null) throw new ArgumentNullException(nameof(joinGame));
            _cheeseCount = cheeseCount;
            Numbers = ThreadSafeRandom.Pick(cheeseCount, 75);
            ConnectionId = joinGame.ConnectionId;
            User = joinGame.User;
        }

        public bool CheckNumber(int number)
        {
            if(Numbers.Contains(number))
            {
                _matchCount++;
                return true;
            }
            return false;
        }
    }
}
