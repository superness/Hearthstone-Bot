using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    public class JsonCard
    {
        public String name;
        public int cost;
        public String type;
        public String rarity;
        public String faction;
        public String text;
        public String[] mechanics;
        public String flavor;
        public String artist;
        public int attack;
        public int health;
        public bool collectible;
        public String id;
        public bool elite;
        /*
         * name : "Leeroy Jenkins",

             cost : 4,

             type : "Minion",
           rarity : "Legendary",
          faction : "Alliance",

             text : "<b>Charge</b>. <b>Battlecry:</b> Summon two 1/1 Whelps for your opponent.",
        mechanics : ["Battlecry", "Charge"],

           flavor : "At least he has Angry Chicken.",

           artist : "Gabe from Penny Arcade",

           attack : 6,
           health : 2,

      collectible : true,
               id : "EX1_116",
            elite : true
        */
    }
}
