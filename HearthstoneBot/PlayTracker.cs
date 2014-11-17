using HearthstoneMemorySearchCLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    public class PlayTracker : Singleton<PlayTracker>
    {
        public enum GameState
        {
            Mulliganing,
            MyTurn,
            OpponentTurn,
            Idle,

            NotInitialized
        }

        public HearthstoneMemorySearchWrapper searcher = new HearthstoneMemorySearchWrapper();
        private List<CardWrapper> lastTurnHand = null;

        public List<CardWrapper> unfiltered = null;

        public int Mana
        {
            get;
            set;
        }

        public int MaxMana
        {
            get;
            set;
        }

        public GameState State
        {
            get;
            set;
        }

        public GameCards Cards
        {
            get;
            set;
        }

        public PlayTracker()
        {
            this.State = GameState.NotInitialized;
            this.Mana = 0;
            this.MaxMana = 0;
        }

        public void Update()
        {
            List<CardWrapper> cards = searcher.GetCardList();

            this.unfiltered = cards;
            this.Cards = new GameCards();
            this.Cards.Init(cards);

            // No cards, must be idle
            if(this.Cards.PlayerHero == null)
            {
                this.State = GameState.Idle;
                return;
            }
            else if(this.State == GameState.Idle)
            {
                this.State = GameState.Mulliganing;
            }

            // We make assummptions that we are in the idle start when the app starts
            //if(this.State == GameState.NotInitialized)
            //{
                //throw new Exception("Start Hearthstone-Bot before entering a game!");
            //}

            // Game has started, we go from idle state into mulligan state
            // Leave mulligan state once the number of cards in our hand has increased

            if(this.State == GameState.Mulliganing)
            {
                // We drew a card so we aren't in mulligan phase anymore
                if (this.lastTurnHand != null && this.lastTurnHand.Count < this.Cards.PlayerHand.CardsInList.Count)
                {
                    this.State = GameState.MyTurn;
                    this.MaxMana = 1;
                    this.Mana = 1;
                }

                this.lastTurnHand = this.Cards.PlayerHand.CardsInList;
            }

            if(this.State == GameState.OpponentTurn)
            {
                bool sameCards = true;
                for (int i = 0; i < this.lastTurnHand.Count && i < this.Cards.PlayerHand.CardsInList.Count; ++i)
                {
                    CardWrapper handCard = this.Cards.PlayerHand.CardsInList[i];
                    if(this.lastTurnHand.FirstOrDefault(c => c.Id == handCard.Id) == null)
                    {
                        sameCards = false;
                        break;
                    }
                }

                if (this.lastTurnHand.Count < this.Cards.PlayerHand.CardsInList.Count || sameCards == false)
                {
                    // Explain WHY it became my turn because I am very confused
                    FileLogger.Global.LogLine("Switching turns");
                    FileLogger.Global.LogLine("------------");
                    FileLogger.Global.LogLine("LastTurnHand");
                    FileLogger.Global.LogLine("------------");
                    foreach(CardWrapper card in this.lastTurnHand)
                    {
                        FileLogger.Global.LogLine(String.Format("{0} | {1}", card.Name, card.Id));
                    }
                    FileLogger.Global.LogLine("------------");
                    FileLogger.Global.LogLine("PlayerHand");
                    FileLogger.Global.LogLine("------------");
                    foreach(CardWrapper card in this.Cards.PlayerHand.CardsInList)
                    {
                        FileLogger.Global.LogLine(String.Format("{0} | {1}", card.Name, card.Id));
                    }
                    FileLogger.Global.LogLine(String.Empty);
                    FileLogger.Global.LogLine(String.Empty);
                    FileLogger.Global.LogLine(String.Empty);

                    this.State = GameState.MyTurn;
                    this.MaxMana++;
                    this.Mana = this.MaxMana;
                }
            }
        }

        public void PassedTurn()
        {
            if(this.State != GameState.MyTurn)
            {
                throw new Exception("Passing turn when it isn't mine!");
            }
            else
            {
                this.State = GameState.OpponentTurn;
                this.lastTurnHand = this.Cards.PlayerHand.CardsInList;
            }
        }
    }
}
