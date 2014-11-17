using HearthstoneBot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    class Program
    {
        public static CardCollectionJson Cards;

        static void Main(string[] args)
        {
            string jsonfile = @"C:\Users\TheGreatCaptainPatri\Documents\GitHub\Hearthstone-Bot\AllSets.json";
            using (StreamReader r = new StreamReader(jsonfile))
            {
                string json = r.ReadToEnd();
                Program.Cards = JsonConvert.DeserializeObject<CardCollectionJson>(json);
            }

            Console.SetWindowPosition(0, 0);

            while(true)
            {
                PlayAI.Global.Update();
            }
        }
    }
}
