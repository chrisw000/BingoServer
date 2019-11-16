using BlueCheese.HostedServices.Bingo;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace BlueCheese.Resources
{
    public class LocalizerByGameMode : ILocalizerByGameMode
    {
        private readonly IStringLocalizer<Bingo> _bingo;
        private readonly IStringLocalizer<Cheesy> _cheesy;
        private readonly IStringLocalizer<McCluskyPD> _mcClusky;

        private readonly ILogger<LocalizerByGameMode> _logger;

        public LocalizerByGameMode(IStringLocalizer<Bingo> bingo, IStringLocalizer<Cheesy> cheesy, IStringLocalizer<McCluskyPD> mcClusky, ILogger<LocalizerByGameMode> logger)
        {
            _bingo = bingo;
            _cheesy = cheesy;
            _mcClusky = mcClusky;
            _logger = logger;
        }

        public string GetName(GameMode mode, int ball)
        {
            var key = $"{ball:G}";

            return mode switch
            {
                GameMode.NotSet => key,

                GameMode.Bingo => $"{_bingo.GetString(key)}",

                GameMode.Cheesy => $"{_cheesy.GetString(key)}",

                GameMode.McCluskyPD => $"{_mcClusky.GetString(key)}",

                _ => $"{mode:G} {ball} resource not made.",
            };
        }

        private int BallCount(GameMode mode)
        {
            return mode switch
            {
                GameMode.Bingo => 90,

                GameMode.Cheesy => 42,

                GameMode.McCluskyPD => 99,

                _ => throw new InvalidOperationException($"Need to pick the game balls for mode {mode:G}"),
            };
        }

        public List<string> NamesFor(GameMode mode)
        {
            var rc = new List<string>();
            rc.Add(string.Empty); // fill in the zero index so ball number == index position
            for(int i=1; i<=BallCount(mode); i++)
            {
                rc.Add(GetName(mode, i));
            }
            return rc;
        }
    }
}
