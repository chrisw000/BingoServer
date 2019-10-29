using System;

namespace BlueCheese.HostedServices.Bingo
{
    public class Draw : IDraw
    {
        public int Number { get; }
        public string Name { get; }
        public bool Matched { get; private set; }

        public Draw(int number, string name)
        {
            Number = number;
            Name = name;
            Matched = false;
        }

        public bool IsMatched(int number)
        {
            if (number == Number)
            {
                if (Matched) throw new InvalidOperationException(nameof(number), new Exception("You can't pick the same number twice innit"));
                Matched = true;
                return true;
            }

            return false;
        }
    }
}
