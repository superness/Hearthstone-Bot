using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Super;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HearthstoneMemorySearchCLR;

namespace HearthstoneBot
{
    public class PlayAI : Singleton<PlayAI>
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        private Rect GameRect = new Rect();

        private bool PassedMulligan = false;

        public void Update()
        {
            Process[] procs = Process.GetProcessesByName("Hearthstone");

            if (procs.Count() > 0)
            {
                Process hsProc = procs[0];

                WindowFocusing.ActivateWindow(hsProc.MainWindowHandle);

                GetWindowRect(hsProc.MainWindowHandle, ref this.GameRect);
            }

            // Click some stuff
            if (PlayTracker.Global.State == PlayTracker.GameState.Mulliganing && PassedMulligan == false)
            {
                Thread.Sleep(15000);

                this.PassMulligan();
                this.PassedMulligan = true;
            }
            else if(PlayTracker.Global.State == PlayTracker.GameState.MyTurn)
            {
                Thread.Sleep(7000);

                int manaToSpend = PlayTracker.Global.Mana;

                // Coin first
                CardWrapper coinCard = PlayTracker.Global.Cards.PlayerHand.CardsInList.FirstOrDefault(c => c.Name == "The Coin");
                if (coinCard != null)
                {
                    this.PlayCard(PlayTracker.Global.Cards.PlayerHand.CardsInList.IndexOf(coinCard));
                    PlayTracker.Global.Update();
                    Program.UpdateDisplay();

                    manaToSpend++;
                    PlayTracker.Global.Mana = manaToSpend;

                    // Attempt to mark the memory so we don't find this 'dead' card again
                    //HearthstoneMemorySearchWrapper.MarkMemory(coinCard);
                }

                // Try to play everything? lolz
                Program.PlayedThisTurn = new List<int>();
                List<CardWrapper> triedToPlay = new List<CardWrapper>();
                while (true)
                {
                    int mostExpensiveToCast = 0;
                    int idMostExpensive = -1;
                    JsonCard jCardMostExpensive = null;
                    CardWrapper cardMostExpensive = null;

                    for (int i = 0; i < PlayTracker.Global.Cards.PlayerHand.CardsInList.Count; ++i)
                    {
                        CardWrapper card = PlayTracker.Global.Cards.PlayerHand.CardsInList[i];
                        JsonCard jCard = Program.Cards.GetCardFromCardId(card.CardId);
                        if (jCard.cost <= manaToSpend)
                        {
                            if (jCard.cost > mostExpensiveToCast && triedToPlay.FirstOrDefault(c => c.Id == card.Id) == null)
                            {
                                mostExpensiveToCast = jCard.cost;
                                idMostExpensive = i;
                                jCardMostExpensive = jCard;
                                cardMostExpensive = card;
                            }
                        }
                    }

                    // Play the most expensive card we can first
                    if (idMostExpensive != -1)
                    {
                        this.PlayCard(idMostExpensive);

                        Thread.Sleep(1000);

                        PlayTracker.Global.Update();
                        Program.UpdateDisplay();
                        CardWrapper toPlay = PlayTracker.Global.Cards.PlayerHand.CardsInList.FirstOrDefault(c => c.Id == cardMostExpensive.Id);
                        triedToPlay.Add(cardMostExpensive);
                        if (toPlay == null)
                        {
                            manaToSpend -= jCardMostExpensive.cost;
                            PlayTracker.Global.Mana = manaToSpend;
                            Program.PlayedThisTurn.Add(cardMostExpensive.Id);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                PlayTracker.Global.Update();
                Program.UpdateDisplay();

                // Attack!
                for (int i = 0; i < PlayTracker.Global.Cards.PlayerPlay.CardsInList.Count; ++i)
                {
                    CardWrapper card = PlayTracker.Global.Cards.PlayerPlay.CardsInList[i];
                    JsonCard jCard = Program.Cards.GetCardFromCardId(card.CardId);

                    // If this minion was just played and doesn't have charge then skip it
                    if (Program.PlayedThisTurn.Contains(card.Id))
                    {
                        if(jCard.mechanics == null || jCard.mechanics.Contains("Charge") == false)
                        {
                            continue;
                        }
                    }

                    // Find a target
                    List<JsonCard> enemyCards = new List<JsonCard>();
                    for (int j = 0; j < PlayTracker.Global.Cards.OpponentPlay.CardsInList.Count; ++j)
                    {
                        CardWrapper enemyCard = PlayTracker.Global.Cards.OpponentPlay.CardsInList[j];
                        JsonCard jCardEnemy = Program.Cards.GetCardFromCardId(enemyCard.CardId);
                        if(jCardEnemy != null)
                            enemyCards.Add(jCardEnemy);
                    }

                    // Look for anyone with taunt that we have to attack
                    bool didAttack = false;
                    JsonCard tauntEnemy = enemyCards.FirstOrDefault(c => c != null && c.mechanics != null && c.mechanics.Contains("Taunt"));
                    if(tauntEnemy != null)
                    {
                        this.AttackMinion(i, enemyCards.IndexOf(tauntEnemy));
                        didAttack = true;
                    }
                    else if(enemyCards.Count > 0)
                    {
                        // Look for a value attack
                        // Attack a minion if I can kill it and not die
                        int j = 0;
                        foreach (JsonCard jCardEnemy in enemyCards)
                        {
                            if(jCard.attack >= jCardEnemy.health && jCard.health > jCardEnemy.attack)
                            {
                                this.AttackMinion(i, j);
                                didAttack = true;
                                break;
                            }
                            ++j;
                        }
                    }
                    if(didAttack == false)
                    {
                        // Attack hero
                        this.AttackHero(i);
                        didAttack = true;
                    }
                    PlayTracker.Global.Update();
                    Program.UpdateDisplay();
                }

                // Use hero power
                this.MoveClickWrapper(780, 640, ClickFlags.LongLeftClick);

                Thread.Sleep(1000);

                this.PassTurn();
            }
        }

        private void PassMulligan()
        {
            this.MoveClickWrapper(650, 660);
        }

        private void PassTurn()
        {
            this.MoveClickWrapper(1092, 386);
            PlayTracker.Global.PassedTurn();
        }

        private void AttackMinion(int idx, int enemyIdx)
        {
            List<int> cardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int cardXAt = cardStarts[PlayTracker.Global.Cards.PlayerPlay.CardsInList.Count - 1] + cardOffset * idx;
            int cardYAt = 475;

            int enemyXAt = cardStarts[PlayTracker.Global.Cards.OpponentPlay.CardsInList.Count - 1] + cardOffset * enemyIdx;
            int enemyYAt = 330;

            // 645, 190
            this.MoveDragWrapper((uint)cardXAt, (uint)cardYAt, (uint)enemyXAt, (uint)enemyYAt);

            Thread.Sleep(500);

            // Clear the attack arrow if it got stuck
            this.MoveClickWrapper((uint)enemyXAt, (uint)enemyYAt, ClickFlags.RightClick);
        }


        private void AttackHero(int idx)
        {
            List<int> cardStarts = new List<int>(new int[] { 640, 590, 540, 490, 440, 390, 340 });
            int cardOffset = 102;

            int cardXAt = cardStarts[PlayTracker.Global.Cards.PlayerPlay.CardsInList.Count - 1] + cardOffset * idx;
            int cardYAt = 475;

            // 645, 190
            this.MoveDragWrapper((uint)cardXAt, (uint)cardYAt, 645, 190);

            Thread.Sleep(500);

            // Clear the attack arrow if it got stuck
            this.MoveClickWrapper(645, 190, ClickFlags.RightClick);
        }

        private void PlayCard(int idx)
        {
            List<int> cardStarts = new List<int>(new int[] {620, 570, 530, 470, 460, 440, 435, 420, 410, 408});
            List<int> cardOffsets = new List<int>(new int[] {0, 100, 100, 100, 80, 67, 57, 51, 46, 41 });

            int cardXAt = cardStarts[PlayTracker.Global.Cards.PlayerHand.CardsInList.Count - 1] + cardOffsets[PlayTracker.Global.Cards.PlayerHand.CardsInList.Count - 1] * idx;
            int cardYAt = 800;

            // 680, 460
            this.MoveDragWrapper((uint)cardXAt, (uint)cardYAt, 680, 460);
        }

        private void MoveDragWrapper(uint x1, uint y1, uint x2, uint y2)
        {
            uint actualX1 = (uint)(x1 + this.GameRect.Left);
            uint actualY1 = (uint)(y1 + this.GameRect.Top);
            uint actualX2 = (uint)(x2 + this.GameRect.Left);
            uint actualY2 = (uint)(y2 + this.GameRect.Top);

            SuperInputSim.MoveMouseTo(actualX1, actualY1);
            Thread.Sleep(500);
            SuperInputSim.SendClick(actualX1, actualY1, ClickFlags.LeftClick);
            Thread.Sleep(500);
            SuperInputSim.SendDrag(actualX1, actualY1, actualX2, actualY2, ClickFlags.LeftClick);
        }

        private void MoveClickWrapper(uint x, uint y, ClickFlags flag = ClickFlags.LeftClick)
        {
            SuperInputSim.MoveMouseTo((uint)(x + this.GameRect.Left), (uint)(y + this.GameRect.Top));
            SuperInputSim.SendClick((uint)(x + this.GameRect.Left), (uint)(y + this.GameRect.Top), flag);
        }
    }
}
