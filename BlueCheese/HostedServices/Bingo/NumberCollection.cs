using System.Collections.Generic;
using System.Linq;
using BlueCheese.Resources;
using System.Globalization;
using System.Threading;

namespace BlueCheese.HostedServices.Bingo
{
    public class NumberCollection : List<Draw>
    {
        private readonly ILocalizerByGameMode _localizer;
        
        public NumberCollection(ILocalizerByGameMode localizer) : base()
        {
            _localizer = localizer;
        }

        public void Generate(GameMode mode, CultureInfo callerCultureInfo)
        {
            var uiC = Thread.CurrentThread.CurrentUICulture;
            var cuC = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentUICulture = callerCultureInfo;
            Thread.CurrentThread.CurrentCulture = callerCultureInfo;

             var name = _localizer.NamesFor(mode);
             AddRange(from i in Enumerable.Range(0, name.Count)
                            select new Draw(i, name[i]));

            Thread.CurrentThread.CurrentUICulture = uiC;
            Thread.CurrentThread.CurrentCulture = cuC;
        }

        public int CountInUse => Count -1;
    }
}
