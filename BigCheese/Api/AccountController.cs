using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlueCheese.HostedServices.Bingo.Contracts;

namespace BigCheese.Api
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private const string Cheese = "Manchego";

        private readonly IEndPlayerManager _endPlayerManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IEndPlayerManager endPlayerManager, ILogger<AccountController> logger)
        {
            _endPlayerManager = endPlayerManager;
            _logger = logger;
        }

        [HttpPost]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public IActionResult Post(string username)
        {
            IHoldUserIdentity userIdentity;

            if (HttpContext.Session.Get<Guid>(Cheese) == default)
            {
                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogDebug("{class}.{method} {parameter} is empty",
                                     nameof(AccountController),
                                     nameof(Post),
                                     nameof(username));

                    return BadRequest("username not supplied.");
                }

                userIdentity = _endPlayerManager.SpawnEndPlayer(username);

                if (userIdentity == null)
                {
                    _logger.LogWarning("{class}.{method} {parameter} unable to spawn player",
                                     nameof(AccountController),
                                     nameof(Post),
                                     nameof(username));

                    return Forbid();
                }

                _logger.LogDebug("{class}.{method} {parameter} spawned new player for {@userIdentity}",
                                    nameof(AccountController),
                                    nameof(Post),
                                    nameof(username),
                                    userIdentity);

                HttpContext.Session.Set<Guid>(Cheese, userIdentity.PlayerId); 
            }
            else
            {
                userIdentity = _endPlayerManager.GetBy(HttpContext.Session.Get<Guid>(Cheese)).ToHoldUserIdentity();

                if (userIdentity == null)
                {
                    _logger.LogWarning("{class}.{method} {parameter} to retrieve player via session id {sessionId}",
                                nameof(AccountController),
                                nameof(Post),
                                nameof(username),
                                HttpContext.Session.Get<Guid>(Cheese));

                    HttpContext.Session.Set<Guid>(Cheese, default);
                    return Forbid();
                }
            }

            return Ok(userIdentity); 
        }
    }
}
