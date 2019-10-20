using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BigCheese.Models;

namespace BigCheese.Controllers
{
    public class LobbyController : Controller
    {
        private readonly ILogger<LobbyController> _logger;

        public LobbyController(ILogger<LobbyController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}
