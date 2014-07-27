using HearthstoneMemorySearchCLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    class Program
    {

        static void Main(string[] args)
        {
            HearthstoneMemorySearchWrapper searcher = new HearthstoneMemorySearchWrapper();

            List<CardWrapper> cards = searcher.GetCardList();

            GameCards gc = new GameCards();
            gc.Init(cards);

            PrintLabel(String.Format("PLAYER [{0}]", gc.PlayerHero.Name));
            PrintLabel("HAND");
            PrintCards(gc.PlayerZonedCards, GameCards.Zones.HAND);
            PrintLabel("PLAY");
            PrintCards(gc.PlayerZonedCards, GameCards.Zones.PLAY);
            //PrintLabel("DECK");
            //PrintCards(gc.PlayerZonedCards, GameCards.Zones.DECK);
            //PrintLabel("GRAVEYARD");
            //PrintCards(gc.PlayerZonedCards, GameCards.Zones.GRAVEYARD);
            //PrintLabel("STASIDE");
            //PrintCards(gc.PlayerZonedCards, GameCards.Zones.SETASIDE);

            PrintLabel(String.Format("OPPONENT [{0}]", gc.OpponentHero.Name));
            PrintLabel("HAND");
            PrintCards(gc.OpponentZonedCards, GameCards.Zones.HAND);
            PrintLabel("PLAY");
            PrintCards(gc.OpponentZonedCards, GameCards.Zones.PLAY);
            //PrintLabel("DECK");
            //PrintCards(gc.OpponentZonedCards, GameCards.Zones.DECK);
            //PrintLabel("GRAVEYARD");
            //PrintCards(gc.OpponentZonedCards, GameCards.Zones.GRAVEYARD);
            //PrintLabel("STASIDE");
            //PrintCards(gc.OpponentZonedCards, GameCards.Zones.SETASIDE);

            Console.ReadLine();
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
