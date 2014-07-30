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
            while (true)
            {
                PlayTracker.Global.Update();

                UpdateDisplay();


                if (PlayTracker.Global.State == PlayTracker.GameState.NotInitialized || PlayTracker.Global.State == PlayTracker.GameState.Idle)
                {
                    Thread.Sleep(5000);
                    continue;
                }

                PlayAI.Global.Update();
            }
        }

        public static void UpdateDisplay()
        {
            Console.Clear();

            Console.WriteLine(String.Format("STATE - {0}", PlayTracker.Global.State));
            Console.WriteLine(String.Format("MANA - {0} \\ {1}", PlayTracker.Global.Mana, PlayTracker.Global.MaxMana));

            if (PlayTracker.Global.State == PlayTracker.GameState.NotInitialized || PlayTracker.Global.State == PlayTracker.GameState.Idle)
            {
                return;
            }

            GameCards gc = PlayTracker.Global.Cards;

            PrintLabel(String.Format("PLAYER [{0}]", gc.PlayerHero.Name));
            PrintLabel("HAND");
            PrintCards(gc.PlayerHand.CardsInList);
            PrintLabel("PLAY");
            PrintCards(gc.PlayerPlay.CardsInList);

            PrintLabel(String.Format("OPPONENT [{0}]", gc.OpponentHero != null ? gc.OpponentHero.Name : "NOT FOUND"));
            PrintLabel("HAND");
            PrintCards(gc.OpponentZonedCards, GameCards.Zones.HAND);
            PrintLabel("PLAY");
            PrintCards(gc.OpponentPlay.CardsInList);
            PrintLabel("GRAVEYARD");
            PrintCards(gc.OpponentZonedCards, GameCards.Zones.GRAVEYARD);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            GameCards unfilteredCards = new GameCards();
            unfilteredCards.Init(PlayTracker.Global.unfiltered, null, false);

            PrintLabel(String.Format("PLAYER [{0}]", unfilteredCards.PlayerHero.Name));
            PrintLabel("HAND");
            PrintCards(unfilteredCards.PlayerZonedCards, GameCards.Zones.HAND);
            PrintLabel("PLAY");
            PrintCards(unfilteredCards.PlayerZonedCards, GameCards.Zones.PLAY);

            PrintLabel(String.Format("OPPONENT [{0}]", unfilteredCards.OpponentHero != null ? unfilteredCards.OpponentHero.Name : "NOT FOUND"));
            PrintLabel("HAND");
            PrintCards(unfilteredCards.OpponentZonedCards, GameCards.Zones.HAND);
            PrintLabel("PLAY");
            PrintCards(unfilteredCards.OpponentZonedCards, GameCards.Zones.PLAY);
        }

        private static void PrintCards(List<CardWrapper>[] cardGroup, GameCards.Zones zone)
        {
            foreach (CardWrapper card in cardGroup[(int)zone])
            {
                Console.WriteLine(String.Format("{0} @ {1} | {2}", card.Name, card.ZonePos, card.Id));
            }
        }

        private static void PrintCards(List<CardWrapper> cards)
        {
            foreach (CardWrapper card in cards)
            {
                Console.WriteLine(String.Format("{0} @ {1} | {2}", card.Name, card.ZonePos, card.Id));
            }
        }

        private static void PrintLabel(String s)
        {
            Console.WriteLine("");
            Console.WriteLine(String.Format("============{0}============", s));
        }
    }
}
