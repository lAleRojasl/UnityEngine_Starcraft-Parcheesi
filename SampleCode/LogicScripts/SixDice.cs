using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets
{
    public class SixDice : Dice
    {
        private static Random r = new Random();

        public int Roll()
        {
            return r.Next(1,7);
        }
    }
}
