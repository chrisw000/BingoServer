using BlueCheese.HostedServices.Bingo.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace BlueCheese.HostedServices.Bingo
{
    public class EndPlayerManager : IEndPlayerManager
    {
        private ILogger<EndPlayerManager> _logger;

        private readonly ConcurrentDictionary<string, Guid> _connections = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<Guid, EndPlayerInfo> _players = new ConcurrentDictionary<Guid, EndPlayerInfo>();
        private readonly ConcurrentDictionary<string, Guid> _playerUsernames = new ConcurrentDictionary<string, Guid>();

        public EndPlayerManager(ILogger<EndPlayerManager> logger)
        {
            _logger = logger;
        }

        public IEndPlayerInfo SpawnEndPlayer(string username)
        {
            if(string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));

            var id = Guid.NewGuid();
            var endPlayerInfo = new EndPlayerInfo(id, username);

            if(_playerUsernames.TryAdd(username, endPlayerInfo.PlayerId))
            {
                if(_players.TryAdd(endPlayerInfo.PlayerId, endPlayerInfo)) // TODO identity id & connection id
                {
                    if(string.IsNullOrEmpty(endPlayerInfo.ConnectionId) 
                        || _connections.TryAdd(endPlayerInfo.ConnectionId, endPlayerInfo.PlayerId))
                    {
                        return endPlayerInfo;  
                    }
                    _players.TryRemove(endPlayerInfo.PlayerId, out _);
                }

                _playerUsernames.TryRemove(username, out _);
            }

            _logger.LogWarning("Cannot spawn end player {@endPlayerInfo}", endPlayerInfo);
            
            return null;
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

            if (_players.TryGetValue(endPlayerInfo.PlayerId, out var player))
            {
                if(string.IsNullOrEmpty(player.ConnectionId) 
                        || _connections.TryRemove(player.ConnectionId, out var id))
                {
                    if(_connections.TryAdd(endPlayerInfo.ConnectionId, endPlayerInfo.PlayerId))
                    {
                        player.ConnectionId = endPlayerInfo.ConnectionId;
                        return player;
                    }
                }
                _logger.LogWarning("Cannot find cached connection to store connection {@endPlayerInfo}", endPlayerInfo);
            }
            else
            {
                _logger.LogWarning("Cannot find cached _player to store connection {@endPlayerInfo}", endPlayerInfo);
            }

            return player;
        }

        public IEndPlayerInfo GetBy(Guid playerId) => _players.TryGetValue(playerId, out var p) ? p : null;


        public IEndPlayerInfo GetBy(IHoldUserIdentity userIdentity)
        {
            if(userIdentity==null) throw new ArgumentNullException(nameof(userIdentity));

            return GetBy(userIdentity.PlayerId);
        }

        public IEndPlayerInfo GetBy(string connectionId)
        {
            if(_connections.TryGetValue(connectionId, out var id))
            {
                return GetBy(id);
            }
            return null;
        }
    }
}
