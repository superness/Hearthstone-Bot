using HearthstoneBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    public class InPlayCard
    {
        public JsonCard CardData;
        public int Attack;
        public int Health;
        public int Id;
        public int ZonePos;
        public bool CanAttack;
        public int CreationId;

        static int creationId = 0;

        public InPlayCard(JsonCard json, int id)
        {
            this.CardData = json;
            this.Attack = json.attack;
            this.Health = json.health;
            this.Id = id;
            this.ZonePos = 0;
            this.CanAttack = this.HasMechanic("Charge");

            this.CreationId = creationId;
            creationId++;
        }

        public bool HasMechanic(string mechanic)
        {
            if(this.CardData.mechanics == null)
            {
                return false;
            }

            foreach(string s in this.CardData.mechanics)
            {
                if(s == mechanic)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
