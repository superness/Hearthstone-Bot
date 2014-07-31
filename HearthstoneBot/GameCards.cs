using HearthstoneMemorySearchCLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    public class GameCards
    {
        public enum Zones
        {
            PLAY,
            HAND,
            GRAVEYARD,
            SETASIDE,
            DECK,
            REMOVEDFROMGAME,

            COUNT
        }

        public List<CardWrapper>[] PlayerZonedCards = new List<CardWrapper>[(int)Zones.COUNT];
        public List<CardWrapper>[] OpponentZonedCards = new List<CardWrapper>[(int)Zones.COUNT];

        public UniqueCardList PlayerHand = null;
        public UniqueCardList PlayerPlay = null;
        public UniqueCardList OpponentPlay = null;

        public CardWrapper PlayerHero
        {
            get;
            set;
        }
        public CardWrapper OpponentHero
        {
            get;
            set;
        }

        private void AddCardToZone(Zones zone, CardWrapper card, bool removeDuplicates = true)
        {
            if(card.Id == 4)
            {
                this.PlayerHero = card;
            }
            else if(card.Id == 36)
            {
                this.OpponentHero = card;
            }
            else if(card.ZonePos != 0 || card.Zone == "GRAVEYARD" || card.Zone == "SETASIDE")
            {
                if (card.Id <= 35 || card.Name == "The Coin")
                {
                    //CardWrapper existing = this.PlayerZonedCards[(int)zone].FirstOrDefault(c => c.ZonePos == card.ZonePos);
                    //if (existing != null && zone != Zones.HAND)
                    //{
                    //    this.PlayerZonedCards[(int)zone].Remove(existing);
                    //}
                    this.PlayerZonedCards[(int)zone].Add(card);
                }
                else
                {
                    //CardWrapper existing = this.OpponentZonedCards[(int)zone].FirstOrDefault(c => c.ZonePos == card.ZonePos);
                    //if (existing != null)
                    //{
                    //    this.OpponentZonedCards[(int)zone].Remove(existing);
                    //}
                    this.OpponentZonedCards[(int)zone].Add(card);
                }
            }

            //if (removeDuplicates)
            //{
            //    this.RemoveDuplicateIds();
            //}
        }

        private void RemoveDuplicateIds()
        {
            foreach(List<CardWrapper> cards in this.PlayerZonedCards)
            {
                List<CardWrapper> toRemove = new List<CardWrapper>();
                for (int i = cards.Count - 1; i >= 0; --i)
                {
                    CardWrapper card = cards[i];

                    if(toRemove.Contains(card))
                    {
                        continue; // Already removing this guy, ignore
                    }

                    CardWrapper duplicate = cards.FirstOrDefault(c => c.Id == card.Id && c != card);

                    if (duplicate != null)
                    {
                        toRemove.Add(card);
                    }
                }
                foreach(CardWrapper card in toRemove)
                {
                    cards.Remove(card);
                }
            }
        }

        public void Init(List<CardWrapper> cards, List<CardWrapper> handShouldBe = null, bool removeDuplicates = true)
        {
            for (int i = (int)Zones.PLAY; i < (int)Zones.COUNT; ++i)
            {
                PlayerZonedCards[i] = new List<CardWrapper>();
                OpponentZonedCards[i] = new List<CardWrapper>();
            }

            cards.Sort((x, y) => { return x.ZonePos - y.ZonePos; });

            foreach (CardWrapper card in cards)
            {
                if (card.Zone == "PLAY")
                {
                    this.AddCardToZone(Zones.PLAY, card, removeDuplicates);
                }
                else if (card.Zone == "HAND")
                {
                    this.AddCardToZone(Zones.HAND, card, removeDuplicates);
                }
                else if (card.Zone == "GRAVEYARD")
                {
                    this.AddCardToZone(Zones.GRAVEYARD, card, removeDuplicates);
                }
                else if (card.Zone == "SETASIDE")
                {
                    this.AddCardToZone(Zones.SETASIDE, card, removeDuplicates);
                }
                else if (card.Zone == "DECK")
                {
                    this.AddCardToZone(Zones.DECK, card, removeDuplicates);
                }
                else if(card.Zone == "REMOVEDFROMGAME")
                {
                    this.AddCardToZone(Zones.REMOVEDFROMGAME, card, removeDuplicates);
                }
                else
                {
                    //throw new Exception("A NEW ZONE");
                }
            }

            this.PlayerHand = new UniqueCardList(Zones.HAND, this.PlayerZonedCards);
            foreach (CardWrapper card in this.PlayerZonedCards[(int)Zones.HAND])
            {
                this.PlayerHand.AddCardToList(card);
            }
            foreach (CardWrapper card in this.PlayerZonedCards[(int)Zones.HAND])
            {
                if(this.PlayerHand.CardsInList.FirstOrDefault(c => c.ZonePos == card.ZonePos) == null)
                {
                    this.PlayerHand.AddCardToList(card);
                }
            }
            this.PlayerHand.CardsInList.Sort((x, y) => x.ZonePos - y.ZonePos);

            this.PlayerPlay = new UniqueCardList(Zones.PLAY, this.PlayerZonedCards);
            foreach (CardWrapper card in this.PlayerZonedCards[(int)Zones.PLAY])
            {
                this.PlayerPlay.AddCardToList(card);
            }
            foreach (CardWrapper card in this.PlayerZonedCards[(int)Zones.PLAY])
            {
                if (this.PlayerPlay.CardsInList.FirstOrDefault(c => c.ZonePos == card.ZonePos) == null)
                {
                    this.PlayerPlay.AddCardToList(card);
                }
            }
            this.PlayerPlay.CardsInList.Sort((x, y) => x.ZonePos - y.ZonePos);

            this.OpponentPlay = new UniqueCardList(Zones.PLAY, this.OpponentZonedCards);
            foreach (CardWrapper card in this.OpponentZonedCards[(int)Zones.PLAY])
            {
                this.OpponentPlay.AddCardToList(card);
            }
            foreach (CardWrapper card in this.OpponentZonedCards[(int)Zones.PLAY])
            {
                if (this.OpponentPlay.CardsInList.FirstOrDefault(c => c.ZonePos == card.ZonePos) == null)
                {
                    this.OpponentPlay.AddCardToList(card);
                }
            }
            this.OpponentPlay.CardsInList.Sort((x, y) => x.ZonePos - y.ZonePos);
        }
    }
}
