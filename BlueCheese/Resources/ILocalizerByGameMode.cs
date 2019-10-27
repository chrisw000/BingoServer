using System.Collections.Generic;
using System.Globalization;
using BlueCheese.HostedServices.Bingo;

namespace BlueCheese.Resources
{
    public interface ILocalizerByGameMode
    {
        string GetName(GameMode mode, int ball);
        List<string> NamesFor(GameMode mode);
    }
}