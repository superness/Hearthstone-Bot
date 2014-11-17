using HearthstoneBot;
using HearthstoneLogReader.Heros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    public static class BasicPlayTracker
    {
        public static List<int> OpponentHand = new List<int>();
        public static List<InPlayCard> FriendlyHand = new List<InPlayCard>();

        public static List<InPlayCard> OpponentPlay = new List<InPlayCard>();
        public static List<InPlayCard> FriendlyPlay = new List<InPlayCard>();
        public static List<int> OpponentSecret = new List<int>();
        public static List<InPlayCard> FriendlySecret = new List<InPlayCard>();

        public static InPlayCard FriendlyWeapon;
        public static InPlayCard OpponentWeapon;

        public static InPlayCard FriendlyHero;
        public static InPlayCard OpponentHero;

        public static string FriendlyStatus;
        public static string OpponentStatus;

        public static string FriendlyHandStatus;

        public static int TotalTurns = 0;
        public static int CurrentMaxMana = 0;
        public static int CurrentMana = 0;
        public static bool IsFriendlyTurn = false;
        public static GameState CurrentGameState = GameState.WaitingForGame;
        public static int BestGuessDamageToOpponent = 0;

        public static IHeroLogic CurrentHero = null;
       
        public enum GameState
        {
            Lobby,
            WaitingForGame,
            Mulliganing,
            Playing,
            Idle,
            EndGameScreen,
        }

        public static int CurrentTurn
        {
            get
            {
                return (int)(TotalTurns / 2.0 + 0.5);
            }
        }

        public static InPlayCard FindInPlayCard(int id)
        {
            foreach (InPlayCard c in OpponentPlay)
            {
                if (c.Id == id)
                    return c;
            }
            foreach (InPlayCard c in FriendlyPlay)
            {
                if (c.Id == id)
                    return c;
            }

            if (OpponentWeapon != null && OpponentWeapon.Id == id)
                return OpponentWeapon;
            if (FriendlyWeapon != null && FriendlyWeapon.Id == id)
                return FriendlyWeapon;

            if (OpponentHero != null && OpponentHero.Id == id)
                return OpponentHero;
            if (FriendlyHero != null && FriendlyHero.Id == id)
                return FriendlyHero;

            return null;
        }

        public static int GetIndexInHandFromCardId(int id)
        {
            InPlayCard card = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.Id == id);

            int i = 0;
            foreach (InPlayCard c in BasicPlayTracker.FriendlyHand)
            {
                if (c == card)
                {
                    return i;
                }
                ++i;
            }

            return -1;
        }

        public static int GetIndexInPlayFromCardId(int id)
        {
            InPlayCard card = BasicPlayTracker.FindInPlayCard(id);

            int i = 0;
            foreach (InPlayCard c in BasicPlayTracker.FriendlyPlay)
            {
                if (c == card)
                {
                    return i;
                }
                ++i;
            }
            i = 0;
            foreach (InPlayCard c in BasicPlayTracker.OpponentPlay)
            {
                if (c == card)
                {
                    return i;
                }
                ++i;
            }

            return -1;
        }

        public static void AdvanceTurn()
        {
            if(TotalTurns == 0 && FriendlyHand.FirstOrDefault(c => c.CardData.name == "The Coin") != null)
            {
                IsFriendlyTurn = false;
            }

            // Switch priorty
            IsFriendlyTurn = !IsFriendlyTurn;
            TotalTurns++;

            // Reset can attack on minions
            foreach(InPlayCard c in FriendlyPlay)
            {
                c.CanAttack = true;
            }
            if(FriendlyWeapon != null)
                FriendlyWeapon.CanAttack = true;

            if(IsFriendlyTurn)
            {
                CurrentMaxMana++;
                CurrentMana = CurrentMaxMana;

                CurrentGameState = GameState.Playing;
            }
            else
            {
                CurrentGameState = GameState.Idle;
            }
        }
        
        public static void AddFriendlyHero(string s, int id)
        {
            if (FriendlyHero == null)
            {
                // A game started
                CurrentGameState = GameState.Mulliganing;
            }

            FriendlyHero = new InPlayCard(Program.Cards.GetCardFromCardId(s), id);

            SetHeroFromInPlayCard(FriendlyHero);
        }

        private static void SetHeroFromInPlayCard(InPlayCard hero)
        {
            switch(hero.CardData.name)
            {
                case "Rexxar":
                    CurrentHero = new HunterHero();
                    break;
                default:
                    break;
            }
        }

        public static void AddOpponentHero(string s, int id)
        {
            OpponentHero = new InPlayCard(Program.Cards.GetCardFromCardId(s), id);
        }

        public static void AddOpponentHand(int id)
        {
            OpponentHand.Add(id);
            Update();
        }

        public static void RemoveOpponentHand(int id)
        {
            OpponentHand.Remove(id);
            Update();
        }

        public static void AddFriendlyHand(string s, int id)
        {
            FriendlyHand.Add(new InPlayCard(Program.Cards.GetCardFromCardId(s), id));
            Update();
        }
        public static void RemoveFriendlyHand(int id)
        {
            foreach (InPlayCard c in FriendlyHand)
            {
                if (c.Id == id)
                {
                    FriendlyHand.Remove(c);
                    break;
                }
            }
            Update();
        }

        public static void AddFriendlySecret(string s, int id)
        {
            FriendlySecret.Add(new InPlayCard(Program.Cards.GetCardFromCardId(s), id));
            Update();
        }

        public static void AddOpponentSecret(int id)
        {
            OpponentSecret.Add(id);
            Update();
        }

        public static void RemoveFriendlySecret(int id)
        {
            foreach (InPlayCard c in FriendlySecret)
            {
                if (c.Id == id)
                {
                    FriendlySecret.Remove(c);
                    break;
                }
            }
            Update();
        }

        public static void RemoveOpponentSecret(int id)
        {
            OpponentSecret.Remove(id);
        }

        public static void AddOpponentPlay(string s, int id)
        {
            JsonCard card = Program.Cards.GetCardFromCardId(s);
            if (card.type == "Weapon")
            {
                OpponentWeapon = new InPlayCard(card, id);
            }
            else
            {
                OpponentPlay.Add(new InPlayCard(card, id));
            }
            if (card.text != null && card.text.Contains("Deathrattle"))
            {
                // If there are any undertakers on the board update their stats
                List<InPlayCard> undertakers = BasicPlayTracker.OpponentPlay.FindAll(c => c.CardData.name == "Undertaker");
                foreach (InPlayCard c in undertakers)
                {
                    GlobalLogs.AILogs.Add("Buffing opponent undertakers");
                    c.Attack += 1;
                    c.Health += 1;
                }
            }
            Update();
        }
        public static void RemoveOpponentPlay(string s, int id)
        {
            foreach (InPlayCard c in OpponentPlay)
            {
                if (c.Id == id)
                {
                    OpponentPlay.Remove(c);
                    break;
                }
            }
            if (OpponentWeapon != null && OpponentWeapon.Id == id)
            {
                OpponentWeapon = null;
            }
            Update();
        }

        public static void AddFriendlyPlay(string s, int id)
        {
            JsonCard card = Program.Cards.GetCardFromCardId(s);
            if (card.type == "Weapon")
            {
                FriendlyWeapon = new InPlayCard(card, id);
            }
            else
            {
                FriendlyPlay.Add(new InPlayCard(card, id));
            }
            Update();
        }
        public static void RemoveFriendlyPlay(string s, int id)
        {
            foreach(InPlayCard c in FriendlyPlay)
            {
                if(c.Id == id)
                {
                    FriendlyPlay.Remove(c);
                    break;
                }
            }
            if (FriendlyWeapon != null && FriendlyWeapon.Id == id)
            {
                FriendlyWeapon = null;
            }
            Update();
        }

        public static void OnAIPlayedCard(InPlayCard card)
        {
            CurrentMana -= card.CardData.cost;
            if(card.CardData.name == "The Coin")
            {
                BasicPlayTracker.CurrentMana += 1;
            }
            if(card.CardData.text.Contains("Deathrattle:"))
            {
                // If there are any undertakers on the board update their stats
                List<InPlayCard> undertakers = BasicPlayTracker.FriendlyPlay.FindAll(c => c.CardData.name == "Undertaker");
                foreach (InPlayCard c in undertakers)
                {
                    GlobalLogs.AILogs.Add("Buffing friendly undertakers");
                    c.Attack += 1;
                    c.Health += 1;
                }
            }
        }

        public static bool CanPlayCard(InPlayCard card)
        {
            if(FriendlySecret.FirstOrDefault(c => c.CardData.name == card.CardData.name) != null)
            {
                return false;
            }
            if(FriendlyWeapon != null && card.CardData.type == "Weapon")
            {
                return false;
            }

            return CurrentMana >= card.CardData.cost && FriendlyPlay.Count < 7;
        }

        public static void UpdateCardZonePos(int id, int zonePos)
        {
            foreach (InPlayCard c in FriendlyPlay)
            {
                if (c.Id == id)
                {
                    if (c.ZonePos != zonePos)
                    {
                        c.ZonePos = zonePos;
                        GlobalLogs.EntityUpdates.Add(string.Format("[{0}-{1}] moved to {2}", c.CardData.name, c.Id, c.ZonePos));
                    }
                    break;
                }
            }
            foreach (InPlayCard c in OpponentPlay)
            {
                if (c.Id == id)
                {
                    if (c.ZonePos != zonePos)
                    {
                        c.ZonePos = zonePos;
                        GlobalLogs.EntityUpdates.Add(string.Format("[{0}-{1}] moved to {2}", c.CardData.name, c.Id, c.ZonePos));
                    }
                    break;
                }
            }
            foreach (InPlayCard c in FriendlyHand)
            {
                if (c.Id == id)
                {
                    if (c.ZonePos != zonePos)
                    {
                        c.ZonePos = zonePos;
                        GlobalLogs.EntityUpdates.Add(string.Format("[{0}-{1}] moved to {2}", c.CardData.name, c.Id, c.ZonePos));

                        //if(c.CardData.name == "Bloodfen Raptor")
                        //{
                        //    Console.WriteLine("PPPPPPPP");
                        //}

                        // If there is another card colliding with this pos then.. move it left?
                        //InPlayCard cardAtPos = FriendlyHand.FirstOrDefault(cap => cap.ZonePos == zonePos && cap != c);
                        //if(cardAtPos != null)
                        //{
                        //    cardAtPos.ZonePos--;
                        //    GlobalLogs.EntityUpdates.Add(string.Format("[{0}-{1}] moved to {2}", cardAtPos.CardData.name, cardAtPos.Id, cardAtPos.ZonePos));
                        //}
                    }
                    break;
                }
            }
            Update();
        }

        public static void Update()
        {
            // Sort in play by zone pos
            FriendlyPlay.Sort((a, b) => a.ZonePos == b.ZonePos ? a.CardData.name.CompareTo(b.CardData.name) : a.ZonePos.CompareTo(b.ZonePos));
            OpponentPlay.Sort((a, b) => a.ZonePos == b.ZonePos ? a.CardData.name.CompareTo(b.CardData.name) : a.ZonePos.CompareTo(b.ZonePos));
            FriendlyHand.Sort((a, b) => a.ZonePos == b.ZonePos ? a.CreationId.CompareTo(b.CreationId) : a.ZonePos.CompareTo(b.ZonePos));

            FriendlyStatus = string.Empty;
            OpponentStatus = string.Empty;

            if (FriendlyHero != null)
                FriendlyStatus += string.Format("{0}-{1} ", FriendlyHero.CardData.name, FriendlyHero.Id);
            if (OpponentHero != null)
                OpponentStatus += string.Format("{0}-{1} ", OpponentHero.CardData.name, OpponentHero.Id);

            if (FriendlyWeapon != null)
                FriendlyStatus += string.Format("+ {0} ", FriendlyWeapon.CardData.name);
            if (OpponentWeapon != null)
                OpponentStatus += string.Format("+ {0} ", OpponentWeapon.CardData.name);

            FriendlyStatus += "| ";
            OpponentStatus += "| ";

            foreach (InPlayCard c in OpponentPlay)
            {
                OpponentStatus += string.Format("{0}-{4} [{1}/{2} @ {3}] ", c.CardData.name, c.Attack, c.Health, c.ZonePos, c.Id);
            }

            foreach (InPlayCard c in FriendlyPlay)
            {
                FriendlyStatus += string.Format("{0}-{4} [{1}/{2} @ {3}] ", c.CardData.name, c.Attack, c.Health, c.ZonePos, c.Id);
            }

            FriendlyHandStatus = string.Empty;
            foreach(InPlayCard c in FriendlyHand)
            {
                FriendlyHandStatus += c.CardData.name + " ";
            }
            //Console.WriteLine(status);
        }

        public static void Reset()
        {
            FriendlyWeapon = null;
            FriendlyHero = null;
            FriendlyHand.Clear();
            FriendlyPlay.Clear();
            FriendlySecret.Clear();

            OpponentWeapon = null;
            OpponentHero = null;
            OpponentHand.Clear();
            OpponentPlay.Clear();
            OpponentSecret.Clear();

            TotalTurns = 0;
            CurrentMaxMana = 0;
            CurrentMana = 0;
            IsFriendlyTurn = false;
            BestGuessDamageToOpponent = 0;

            Update();
        }
    }
}
