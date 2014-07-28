using HearthstoneMemorySearchCLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    class Program
    {

        static void Main(string[] args)
        {
            PlayTracker tracker = new PlayTracker();
            while (true)
            {
                tracker.Update();

                Console.Clear();

                Console.WriteLine(String.Format("STATE - {0}", tracker.State));
                Console.WriteLine(String.Format("MANA - {0} \\ {1}", tracker.Mana, tracker.MaxMana));

                if(tracker.State == PlayTracker.GameState.NotInitialized || tracker.State == PlayTracker.GameState.Idle)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                GameCards gc = tracker.Cards;

                PrintLabel(String.Format("PLAYER [{0}]", gc.PlayerHero.Name));
                PrintLabel("HAND");
                PrintCards(gc.PlayerZonedCards, GameCards.Zones.HAND);
                PrintLabel("PLAY");
                PrintCards(gc.PlayerZonedCards, GameCards.Zones.PLAY);

                PrintLabel(String.Format("OPPONENT [{0}]", gc.OpponentHero != null ? gc.OpponentHero.Name : "NOT FOUND"));
                PrintLabel("HAND");
                PrintCards(gc.OpponentZonedCards, GameCards.Zones.HAND);
                PrintLabel("PLAY");
                PrintCards(gc.OpponentZonedCards, GameCards.Zones.PLAY);
            }
        }

        private static void PrintCards(List<CardWrapper>[] cardGroup, GameCards.Zones zone)
        {
            foreach (CardWrapper card in cardGroup[(int)zone])
            {
                Console.WriteLine(String.Format("{0} @ {1}", card.Name, card.ZonePos));
            }
        }

        private static void PrintLabel(String s)
        {
            Console.WriteLine("");
            Console.WriteLine(String.Format("============{0}============", s));
        }
    }
}
