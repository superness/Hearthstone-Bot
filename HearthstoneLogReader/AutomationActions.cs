using Super;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HearthstoneLogReader
{
    public static class AutomationActions
    {
        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static Rect GameRect = new Rect();

        public static void PassMulligan()
        {
            MoveClickWrapper(650, 660);

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] passing mulligan"));
        }

        public static void PassTurn()
        {
            MoveClickWrapper(1092, 386);

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] passing turn"));
        }

        public static void UseHeroPower()
        {
            MoveClickWrapper(780, 640, ClickFlags.LongLeftClick);
            Thread.Sleep(500);
            MoveClickWrapper(780, 640, ClickFlags.LongLeftClick);
            BasicPlayTracker.CurrentMana -= 2;
            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] using hero power"));

            if(BasicPlayTracker.FriendlyHero.CardData.name == "Rexxar")
            {
                BasicPlayTracker.BestGuessDamageToOpponent += 2;
            }
        }

        public static void AttackMinion(int idx, int enemyIdx)
        {
            List<int> cardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int cardXAt = cardStarts[BasicPlayTracker.FriendlyPlay.Count - 1] + cardOffset * idx;
            int cardYAt = 475;

            int enemyXAt = cardStarts[BasicPlayTracker.OpponentPlay.Count - 1] + cardOffset * enemyIdx;
            int enemyYAt = 330;

            // 645, 190
            MoveDragWrapper((uint)cardXAt, (uint)cardYAt, (uint)enemyXAt, (uint)enemyYAt);
            MoveClickWrapper((uint)enemyXAt, (uint)enemyYAt);

            Thread.Sleep(500);

            // Clear can attack falg
            BasicPlayTracker.FriendlyPlay[idx].CanAttack = false;

            // Deal damage
            BasicPlayTracker.FriendlyPlay[idx].Health -= BasicPlayTracker.OpponentPlay[enemyIdx].Attack;
            BasicPlayTracker.OpponentPlay[enemyIdx].Health -= BasicPlayTracker.FriendlyPlay[idx].Attack;

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] {0} attacking {1}", BasicPlayTracker.FriendlyPlay[idx].CardData.name, BasicPlayTracker.OpponentPlay[enemyIdx].CardData.name));

            // Wait for play tracker to update
            int numAttacks = GlobalLogs.DeclaredAttackers.Count;
            while(numAttacks == GlobalLogs.DeclaredAttackers.Count)
            {
                Thread.Sleep(1000);
                GameStateTracker.Global.Update();
            }

            // Sleep a long time to let the dust settle from cascading effects
            Thread.Sleep(6000);

            // Clear the attack arrow if it got stuck
            //this.MoveClickWrapper((uint)enemyXAt, (uint)enemyYAt, ClickFlags.RightClick);
        }

        public static void AttackWithHero(int targetIdx)
        {
            List<int> cardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int enemyXAt = cardStarts[BasicPlayTracker.OpponentPlay.Count - 1] + cardOffset * targetIdx;
            int enemyYAt = 330;

            MoveDragWrapper((uint)650, (uint)650, (uint)enemyXAt, (uint)enemyYAt);
            MoveClickWrapper((uint)enemyXAt, (uint)enemyYAt);

            Thread.Sleep(500);
        }

        public static void AttackHeroWithHero()
        {
            MoveDragWrapper((uint)650, (uint)650, 645, 190);
            MoveClickWrapper(645, 190);

            Thread.Sleep(500);
        }

        public static void AttackHero(int idx)
        {
            List<int> cardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int cardXAt = cardStarts[BasicPlayTracker.FriendlyPlay.Count - 1] + cardOffset * idx;
            int cardYAt = 475;

            // 645, 190
            MoveDragWrapper((uint)cardXAt, (uint)cardYAt, 645, 190);
            MoveClickWrapper(645, 190);

            Thread.Sleep(500);

            BasicPlayTracker.FriendlyPlay[idx].CanAttack = false;

            BasicPlayTracker.BestGuessDamageToOpponent += BasicPlayTracker.FriendlyPlay[idx].Attack;
            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] {0} attacking face", BasicPlayTracker.FriendlyPlay[idx].CardData.name));

            // Clear the attack arrow if it got stuck
            //this.MoveClickWrapper(645, 190, ClickFlags.RightClick);
        }

        public static void MulliganCard(int idx)
        {
            List<int> threeMulliganCardLocations = new List<int>(new int[] { 400, 650, 900 });
            List<int> fourMulliganCardLocations = new List<int>(new int[] { 370, 560, 750, 940 });

            int cardYAt = 400;
            InPlayCard coin = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "The Coin");
            int cardXAt = coin != null ? fourMulliganCardLocations[idx] : threeMulliganCardLocations[idx];

            MoveClickWrapper((uint)cardXAt, (uint)cardYAt);

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] mulliganing {0}", idx));
        }

        public static void PlaySpellAtFace(int idx)
        {
            List<int> cardStarts = new List<int>(new int[] { 620, 570, 530, 470, 460, 440, 435, 420, 410, 408 });
            List<int> cardOffsets = new List<int>(new int[] { 0, 100, 100, 100, 80, 67, 57, 51, 46, 41 });

            int cardXAt = cardStarts[BasicPlayTracker.FriendlyHand.Count - 1] + cardOffsets[BasicPlayTracker.FriendlyHand.Count - 1] * idx;
            int cardYAt = 800;

            List<int> enemyCardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int enemyXAt = 650;
            int enemyYAt = 175;

            MoveClickWrapper((uint)cardXAt, (uint)cardYAt);
            Thread.Sleep(500);
            MoveClickWrapper((uint)enemyXAt, (uint)enemyYAt);

            InPlayCard card = BasicPlayTracker.FriendlyHand[idx];
            BasicPlayTracker.OnAIPlayedCard(card);

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] playing spell {0} at face", BasicPlayTracker.FriendlyHand[idx].CardData.name));

            // Wait for play tracker to update
            int numInHand = BasicPlayTracker.FriendlyHand.Count;
            while (numInHand == BasicPlayTracker.FriendlyHand.Count)
            {
                GameStateTracker.Global.Update();
                Thread.Sleep(1000);
            }
        }

        public static void PlaySpellAt(int idx, int playIdx)
        {
            List<int> cardStarts = new List<int>(new int[] { 620, 570, 530, 470, 460, 440, 435, 420, 410, 408 });
            List<int> cardOffsets = new List<int>(new int[] { 0, 100, 100, 100, 80, 67, 57, 51, 46, 41 });

            int cardXAt = cardStarts[BasicPlayTracker.FriendlyHand.Count - 1] + cardOffsets[BasicPlayTracker.FriendlyHand.Count - 1] * idx;
            int cardYAt = 800;

            List<int> enemyCardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int enemyXAt = cardStarts[BasicPlayTracker.OpponentPlay.Count - 1] + cardOffset * playIdx;
            int enemyYAt = 330;

            MoveClickWrapper((uint)cardXAt, (uint)cardYAt);
            Thread.Sleep(500);
            MoveClickWrapper((uint)enemyXAt, (uint)enemyYAt);

            InPlayCard card = BasicPlayTracker.FriendlyHand[idx];
            BasicPlayTracker.OnAIPlayedCard(card);

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] playing spell {0} at {1}", BasicPlayTracker.FriendlyHand[idx].CardData.name, BasicPlayTracker.OpponentPlay[playIdx].CardData.name));

            // Wait for play tracker to update
            int numInHand = BasicPlayTracker.FriendlyHand.Count;
            while (numInHand == BasicPlayTracker.FriendlyHand.Count)
            {
                GameStateTracker.Global.Update();
                Thread.Sleep(1000);
            }
        }

        public static void PlayCardById(int id, int playIdx = -1)
        {
            try
            {
                PlayCard(BasicPlayTracker.GetIndexInHandFromCardId(id));
            }
            catch(Exception ex)
            {
                //try again?
                try
                {
                    PlayCard(BasicPlayTracker.GetIndexInHandFromCardId(id));
                }
                catch(Exception ex2)
                {
                    // Maybe something is wrong and we don't have as much mana as we think
                    BasicPlayTracker.CurrentMaxMana -= 1;
                    BasicPlayTracker.CurrentMana -= 1;
                    BasicPlayTracker.TotalTurns -= 1;
                }
            }
        }

        public static void PlayCard(int idx, int playIdx = -1)
        {
            if (idx == -1)
                throw new Exception("Can't play a card with idx -1");
            List<int> cardStarts = new List<int>(new int[] { 620, 570, 530, 470, 460, 440, 435, 420, 410, 408 });
            List<int> cardOffsets = new List<int>(new int[] { 0, 100, 100, 100, 80, 67, 57, 51, 46, 41 });

            int cardXAt = cardStarts[BasicPlayTracker.FriendlyHand.Count - 1] + cardOffsets[BasicPlayTracker.FriendlyHand.Count - 1] * idx;
            int cardYAt = 800;

            // 680, 460
            MoveDragWrapper((uint)cardXAt, (uint)cardYAt, 1025, 460);

            InPlayCard card = BasicPlayTracker.FriendlyHand[idx];

            GlobalLogs.AILogs.Add(string.Format("[AutomationActions] playing card {0}", BasicPlayTracker.FriendlyHand[idx].CardData.name));

            // Wait for play tracker to update
            DateTime start = DateTime.Now;
            const int TimeOut = 10000;
            if(card.CardData.type == "Minion")
            {
                int numInPlay = BasicPlayTracker.FriendlyPlay.Count;
                while(numInPlay == BasicPlayTracker.FriendlyPlay.Count)
                {
                    GameStateTracker.Global.Update();
                    Thread.Sleep(1000);

                    if((DateTime.Now - start).TotalMilliseconds > TimeOut)
                    {
                        throw new Exception("Timed out playing " + card.CardData.name);
                    }
                }
            }
            else if(card.CardData.type == "Weapon")
            {
                while (BasicPlayTracker.FriendlyWeapon == null || (BasicPlayTracker.FriendlyWeapon != null && BasicPlayTracker.FriendlyWeapon.CardData.name != card.CardData.name))
                {
                    GameStateTracker.Global.Update();
                    Thread.Sleep(1000);

                    if ((DateTime.Now - start).TotalMilliseconds > TimeOut)
                    {
                        throw new Exception("Timed out playing " + card.CardData.name);
                    }
                }
            }
            else if(card.CardData.name.Contains("Trap"))
            {
                Thread.Sleep(1000); // Just sleep, might not work, oh well
            }
            else
            {
                int numInHand = BasicPlayTracker.FriendlyHand.Count;
                while(numInHand == BasicPlayTracker.FriendlyHand.Count)
                {
                    GameStateTracker.Global.Update();
                    Thread.Sleep(1000);

                    if ((DateTime.Now - start).TotalMilliseconds > TimeOut)
                    {
                        throw new Exception("Timed out playing " + card.CardData.name);
                    }
                }
            }
            BasicPlayTracker.OnAIPlayedCard(card);
        }

        public static void MoveDragWrapper(uint x1, uint y1, uint x2, uint y2)
        {
            uint actualX1 = (uint)(x1 + GameRect.Left);
            uint actualY1 = (uint)(y1 + GameRect.Top);
            uint actualX2 = (uint)(x2 + GameRect.Left);
            uint actualY2 = (uint)(y2 + GameRect.Top);

            SmoothMove(x1, y1);

            SuperInputSim.MoveMouseTo(actualX1, actualY1);
            Thread.Sleep(500);
            SuperInputSim.SendClick(actualX1, actualY1, ClickFlags.LeftClick);
            Thread.Sleep(500);
            SmoothMove(x2, y2);
            Thread.Sleep(500);
            SuperInputSim.SendClick(actualX2, actualY2, ClickFlags.LeftClick);
            //SuperInputSim.SendDrag(actualX1, actualY1, actualX2, actualY2, ClickFlags.LeftClick);
        }

        public static void MoveClickWrapper(uint x, uint y, ClickFlags flag = ClickFlags.LeftClick)
        {
            SmoothMove(x, y);

            SuperInputSim.MoveMouseTo((uint)(x + GameRect.Left), (uint)(y + GameRect.Top));
            Thread.Sleep(500);
            SuperInputSim.SendClick((uint)(x + GameRect.Left), (uint)(y + GameRect.Top), flag);
        }

        private static void SmoothMove(uint x, uint y)
        {
            // Smooth move exalx
            float numMoves = 3.0f;
            float curX = Cursor.Position.X;
            float curY = Cursor.Position.Y;
            float tarX = x + GameRect.Left;
            float tarY = y + GameRect.Top;
            float dX = (tarX - curX) / numMoves;
            float dY = (tarY - curY) / numMoves;

            for (int i = 0; i <= numMoves; ++i)
            {
                SuperInputSim.MoveMouseTo((uint)(curX), (uint)(curY));
                curX += dX;
                curY += dY;
                Thread.Sleep(25);
            }
        }
    }
}
