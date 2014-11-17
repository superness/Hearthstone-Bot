using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    public class CardCollectionJson
    {
        public JsonCard[] Basic;
        public JsonCard[] Credits;
        [JsonProperty("Curse of Naxxramas")]
        public JsonCard[] CurseOfNaxxramas;
        public JsonCard[] Debug;
        public JsonCard[] Expert;
        public JsonCard[] Missions;
        public JsonCard[] Promotion;
        public JsonCard[] Reward;
        public JsonCard[] System;

        public JsonCard GetCardFromCardId(String cardId)
        {
            foreach (JsonCard card in Basic)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in Credits)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in CurseOfNaxxramas)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in Debug)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in Expert)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in Missions)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in Promotion)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in Reward)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }
            foreach (JsonCard card in System)
            {
                if (card.id == cardId)
                {
                    return card;
                }
            }

            return null;
        }
    }
}
