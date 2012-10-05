using System;
using System.Collections.Generic;
using System.Text;

namespace MinMaxMoola
{
    class Program
    {
        static void Main(string[] args)
        {
          //  Console.SetWindowSize(60, 75);
        Game:
            GoldRush gr = new GoldRush();
            Console.Write("\n[a]gain? ");
            if (Console.ReadLine().ToLower() == "a")
            {
                goto Game;
            }
        }
    }
}
