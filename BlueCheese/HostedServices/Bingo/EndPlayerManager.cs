using BlueCheese.HostedServices.Bingo.Contracts;
using System;
using System.Collections.Concurrent;

namespace BlueCheese.HostedServices.Bingo
{
    public class EndPlayerManager : IEndPlayerManager
    {
        private readonly ConcurrentDictionary<Guid, EndPlayerInfo> _players = new ConcurrentDictionary<Guid, EndPlayerInfo>();
        private readonly ConcurrentDictionary<string, Guid> _playerUsernames = new ConcurrentDictionary<string, Guid>();

        public IEndPlayerInfo SpawnEndPlayer(string username)
        {
            if(string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

            var id = Guid.NewGuid();
            var endPlayerInfo = new EndPlayerInfo(id, username);

            if(_playerUsernames.TryAdd(username, endPlayerInfo.PlayerId))
            {
                if(_players.TryAdd(endPlayerInfo.PlayerId, endPlayerInfo)) // TODO identity id & connection id
                {
                    return endPlayerInfo;  
                }
                else
                {
                    _playerUsernames.TryRemove(username, out _);
                }
            }
            
            return null;
        }

        public IEndPlayerInfo GetByPlayerId(Guid playerId)
        {
            _players.TryGetValue(playerId, out var p);
            return p; // TODO: take connectionId off this interface
        }

        public bool CheckUserAgainstId(IHoldUserIdentity userIdentity)
        {
            if(userIdentity==null) throw new ArgumentNullException(nameof(userIdentity));

            // Move to object to track these?
            // Check the user / playerid combo
            if(_playerUsernames.TryGetValue(userIdentity.User, out var id))
            {
                if(id!=userIdentity.PlayerId)
                {
                    return false;
                }
                if(_players.TryGetValue(id, out var endPlayer))
                {
                    if(id!=endPlayer.PlayerId) // this 
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public IEndPlayerInfo StoreConnection(IEndPlayerInfo endPlayerInfo)
        {
            if(endPlayerInfo==null) throw new ArgumentNullException(nameof(endPlayerInfo));

            _players.TryGetValue(endPlayerInfo.PlayerId, out var player);
            player.ConnectionId = endPlayerInfo.ConnectionId;

            return player;
        }
    }
}
