using BlueCheese.HostedServices.Bingo.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace BlueCheese.HostedServices.Bingo
{
    public class EndPlayerManager : IEndPlayerManager
    {
        private readonly ILogger<EndPlayerManager> _logger;

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

            var endPlayerInfo = new EndPlayerInfo(username);

            if(_playerUsernames.TryAdd(username, endPlayerInfo.PlayerId))
            {
                if(_players.TryAdd(endPlayerInfo.PlayerId, endPlayerInfo)) // TODO identity id & connection id
                {
                    if(string.IsNullOrEmpty(endPlayerInfo.ConnectionId) 
                        || _connections.TryAdd(endPlayerInfo.ConnectionId, endPlayerInfo.PlayerId))
                    {
                        return endPlayerInfo;  
                    }
                    else
                    {
                        _logger.LogWarning("Cannot spawn end player {@endPlayerInfo} - duplicate connectionId", endPlayerInfo);
                    }
                    _players.TryRemove(endPlayerInfo.PlayerId, out _);
                }
                else
                {
                    _logger.LogWarning("Cannot spawn end player {@endPlayerInfo} - duplicate playerId", endPlayerInfo);
                }
                _playerUsernames.TryRemove(username, out _);
            }
            else
            {
                _logger.LogWarning("Cannot spawn end player {@endPlayerInfo} - duplicate username", endPlayerInfo);
            }           
            
            return null;
        }

        public bool CheckUserAgainstId(IHoldUserIdentity userIdentity)
        {
            if(userIdentity==null) return false;
            if(string.IsNullOrEmpty(userIdentity.User)) return false;
            if(userIdentity.PlayerId == Guid.Empty) return false;

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
                _logger.LogWarning("Cannot find cached _connections to store connection {@endPlayerInfo}", endPlayerInfo);
            }
            else
            {
                _logger.LogWarning("Cannot find cached _players to store connection {@endPlayerInfo}", endPlayerInfo);
            }

            return player;
        }

        public IEndPlayerInfo GetBy(Guid playerId) 
        {
            if(_players.TryGetValue(playerId, out var endPlayerInfo))
            {
                return endPlayerInfo;
            }

            _logger.LogDebug("{class}.{method} cannot get by playerId {playerId}"
                , nameof(EndPlayerManager), nameof(GetBy), playerId);

            return null;
        }


        public IEndPlayerInfo GetBy(IHoldUserIdentity userIdentity)
        {
            if(userIdentity==null) throw new ArgumentNullException(nameof(userIdentity));

            return GetBy(userIdentity.PlayerId);
        }

        public IEndPlayerInfo GetBy(string connectionId)
        {
            if(string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("Must supply a connectionId", nameof(connectionId));

            if(_connections.TryGetValue(connectionId, out var id))
            {
                return GetBy(id);
            }

            _logger.LogDebug("{class}.{method} cannot get by connectionId {connectionId}"
                    , nameof(EndPlayerManager), nameof(GetBy), connectionId);

            return null;
        }
    }
}
