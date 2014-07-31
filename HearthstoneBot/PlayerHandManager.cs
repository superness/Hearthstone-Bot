using HearthstoneMemorySearchCLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    public class UniqueCardList
    {
        public List<CardWrapper> CardsInList = new List<CardWrapper>();

        public GameCards.Zones ZoneList
        {
            get;
            set;
        }

        public List<CardWrapper>[] CardsCompare
        {
            get;
            set;
        }

        public UniqueCardList(GameCards.Zones zone, List<CardWrapper>[] compare)
        {
            this.ZoneList = zone;
            this.CardsCompare = compare;
        }

        public void AddCardToList(CardWrapper card)
        {
            bool existsInOtherZone = false;

            // Check if card is in any other zone, if so do not add it
            for (int i = 0; i < this.CardsCompare.Count(); ++i)
            {
                if(i != (int)this.ZoneList && i != (int)GameCards.Zones.HAND)
                {
                    List<CardWrapper> cards = this.CardsCompare[i];
                    if (cards.FirstOrDefault(c => c.Id == card.Id) != null)
                    {
                        existsInOtherZone = true;
                        break;
                    }
                }
            }

            CardWrapper inPlayInZonePosCard = this.CardsInList.FirstOrDefault(c => c.ZonePos == card.ZonePos);
            if(inPlayInZonePosCard != null && this.ZoneList == GameCards.Zones.PLAY)
            {
                this.CardsInList.Remove(inPlayInZonePosCard);
            }

            if(existsInOtherZone == false)
            {
                // Check if it exists in our hand
                CardWrapper inHandCard = this.CardsInList.FirstOrDefault(c => c.Id == card.Id);
                if(inHandCard != null)
                {
                    this.CardsInList.Remove(inHandCard);
                }
                this.CardsInList.Add(card);
            }
        }
    }
}
