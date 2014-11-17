using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HearthstoneLogReader.Heros
{
    public class HunterHero : IHeroLogic
    {
        public void PerformMulligans()
        {
            List<int> toMulligan = new List<int>();
            int i = 0;
            foreach(InPlayCard c in BasicPlayTracker.FriendlyHand)
            {
                if(c.CardData.name == "The Coin")
                {
                    continue;
                }

                // Keep deathrattles and undertaker
                if (!(c.CardData.name == "Undertaker" || (c.HasMechanic("Deathrattle") && c.CardData.name != "Savannah Highmane")))
                {
                    toMulligan.Add(i);
                }
                ++i;
            }

            // Mulligan these
            foreach(int j in toMulligan)
            {
                // NotIMPL
                AutomationActions.MulliganCard(j);
                Thread.Sleep(1000);
            }

            // Pass mulligan
            AutomationActions.PassMulligan();
        }

        public void ProcessTurn()
        {
            Legal();
            GameStateTracker.Global.Update();
            FillBoard();
            GameStateTracker.Global.Update();
            CleanupBoard();
            GameStateTracker.Global.Update();
            AttackFace();
            GameStateTracker.Global.Update();
            FillBoard();
            GameStateTracker.Global.Update();

            // Use hero power
            if(BasicPlayTracker.CurrentMana >= 2)
            {
                AutomationActions.UseHeroPower();
            }

            AutomationActions.PassTurn();
        }

        private void Legal()
        {
            // Check for kill commands
            List<InPlayCard> killCommands = BasicPlayTracker.FriendlyHand.FindAll(c => c.CardData.name == "Kill Command");
            int killShotDmg = BasicPlayTracker.FriendlyPlay.FirstOrDefault(c => (c.CardData.text != null && c.CardData.text.Contains("Beast"))) != null ? 5 : 3;

            int totalDmg = killCommands.Count * killShotDmg + 2;
            int totalCost = killCommands.Count * 3 + 2;

            // If no taunts, check for everything to face
            if(BasicPlayTracker.OpponentPlay.FirstOrDefault(c=>c.HasMechanic("Taunt")) == null)
            {
                totalDmg += (BasicPlayTracker.FriendlyWeapon != null ? BasicPlayTracker.FriendlyWeapon.Attack : 0);
                
                foreach(InPlayCard c in BasicPlayTracker.FriendlyPlay)
                {
                    totalDmg += c.Attack;
                }

                // Check weapon in hand and not in play
                if(BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.type == "Weapon") != null && BasicPlayTracker.FriendlyWeapon == null)
                {
                    totalDmg += 3;
                    totalCost += BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.type == "Weapon").CardData.cost;
                }
            }

            GlobalLogs.AILogs.Add(string.Format("[Legal Check] damage {0}/{1} cost {2}", totalDmg, 30 - BasicPlayTracker.BestGuessDamageToOpponent, totalCost));

            if(BasicPlayTracker.BestGuessDamageToOpponent + totalDmg >= 30 )
            {
                GlobalLogs.AILogs.Add("Going for legal! " + totalDmg + " > " + (30 - BasicPlayTracker.BestGuessDamageToOpponent) + " costs " + totalCost);
                GameStateTracker.Global.Update();

                // Unleash!
                foreach(InPlayCard c in killCommands)
                {
                    AutomationActions.PlaySpellAtFace(BasicPlayTracker.GetIndexInHandFromCardId(c.Id));
                }

                AutomationActions.UseHeroPower();

                if (BasicPlayTracker.OpponentPlay.FirstOrDefault(c => c.HasMechanic("Taunt")) == null)
                {
                    for (int i = 0; i < BasicPlayTracker.FriendlyPlay.Count; ++i)
                    {
                        AutomationActions.AttackHero(i);
                    }

                    if (BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.type == "Weapon") != null && BasicPlayTracker.FriendlyWeapon == null)
                    {
                        AutomationActions.PlayCardById(BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.type == "Weapon").Id);
                        AutomationActions.AttackHeroWithHero();
                    }
                }
            }
        }

        private void CleanupBoard()
        {
            // Break taunts
            while (BasicPlayTracker.OpponentPlay.FirstOrDefault(c => c.HasMechanic("Taunt")) != null)
            {
                bool tryAgain = false;
                foreach (InPlayCard enemy in BasicPlayTracker.OpponentPlay)
                {
                    if (enemy.HasMechanic("Taunt"))
                    {
                        // Find the best minion for this destruction
                        List<InPlayCard> canKillWith = BasicPlayTracker.FriendlyPlay.FindAll(c => c.Attack >= enemy.Health && c.CanAttack);
                        canKillWith.Sort((x, y) => x.Health.CompareTo(y.Health));

                        if (canKillWith.Count > 0)
                        {
                            // Attack
                            int idxFriendly = BasicPlayTracker.GetIndexInPlayFromCardId(canKillWith.First().Id);
                            int idxEnemy = BasicPlayTracker.GetIndexInPlayFromCardId(enemy.Id);

                            AutomationActions.AttackMinion(idxFriendly, idxEnemy);
                            tryAgain = true;
                        }

                        break;
                    }
                }
                if (!tryAgain) break;
            }

            // More taunts? Can we kill shot them or hunter's mark them?
            {
                while (BasicPlayTracker.OpponentPlay.FirstOrDefault(c => c.HasMechanic("Taunt")) != null)
                {
                    // If we have one attack guys on the board and a Hunter's Mark in hand then kill the taunt
                    InPlayCard oneAttackCard = BasicPlayTracker.FriendlyPlay.FirstOrDefault(c => c.Attack == 1);
                    if (oneAttackCard != null && oneAttackCard.CanAttack == false) oneAttackCard = null;
                    InPlayCard huntersMark = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "Hunter's Mark");
                    InPlayCard tauntEnemy = BasicPlayTracker.OpponentPlay.First(c => c.HasMechanic("Taunt"));

                    if (oneAttackCard != null && huntersMark != null && tauntEnemy != null && !tauntEnemy.HasMechanic("Stealth"))
                    {
                        int huntermarkIdx = BasicPlayTracker.GetIndexInHandFromCardId(huntersMark.Id);
                        int oneAttackIdx = BasicPlayTracker.GetIndexInPlayFromCardId(oneAttackCard.Id);
                        int targetPlayIdx = BasicPlayTracker.GetIndexInPlayFromCardId(tauntEnemy.Id);

                        // Hunter's mark the big enemy
                        AutomationActions.PlaySpellAt(huntermarkIdx, targetPlayIdx);

                        // Attack it
                        AutomationActions.AttackMinion(oneAttackIdx, targetPlayIdx);

                        continue;
                    }

                    // Look for a kill command
                    int killShotDmg = BasicPlayTracker.FriendlyPlay.FirstOrDefault(c => (c.CardData.text != null && c.CardData.text.Contains("Beast"))) != null ? 5 : 3;
                    InPlayCard killshot = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "Kill Command");
                    InPlayCard tauntEnemyCanKill = BasicPlayTracker.OpponentPlay.FirstOrDefault(c => c.HasMechanic("Taunt") && c.Health <= killShotDmg);
                    if (killshot != null && tauntEnemyCanKill != null && !tauntEnemyCanKill.HasMechanic("Stealth") && BasicPlayTracker.CanPlayCard(killshot))
                    {
                        AutomationActions.PlaySpellAt(BasicPlayTracker.GetIndexInHandFromCardId(killshot.Id), BasicPlayTracker.GetIndexInPlayFromCardId(tauntEnemy.Id));

                        continue;
                    }

                    // Didn't work? Throw guys into it until it dies..
                    foreach(InPlayCard friendly in BasicPlayTracker.FriendlyPlay.FindAll(c => c.CanAttack))
                    {
                        if (BasicPlayTracker.GetIndexInPlayFromCardId(tauntEnemy.Id) != -1)
                        {
                            AutomationActions.AttackMinion(BasicPlayTracker.GetIndexInPlayFromCardId(friendly.Id), BasicPlayTracker.GetIndexInPlayFromCardId(tauntEnemy.Id));
                        }

                        continue;
                    }

                    break;
                }
            }

            {
                // If we have one attack guys on the board and a Hunter's Mark in hand then kill something juicy
                InPlayCard oneAttackCard = BasicPlayTracker.FriendlyPlay.FirstOrDefault(c => c.Attack == 1);
                if (oneAttackCard != null && oneAttackCard.CanAttack == false) oneAttackCard = null;
                InPlayCard huntersMark = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "Hunter's Mark");
                InPlayCard bigEnemy = BasicPlayTracker.OpponentPlay.FirstOrDefault(c => c.CardData.cost >= 4);

                if (oneAttackCard != null && huntersMark != null && bigEnemy != null && !bigEnemy.HasMechanic("Stealth"))
                {
                    int huntermarkIdx = BasicPlayTracker.GetIndexInHandFromCardId(huntersMark.Id);
                    int oneAttackIdx = BasicPlayTracker.GetIndexInPlayFromCardId(oneAttackCard.Id);
                    int targetPlayIdx = BasicPlayTracker.GetIndexInPlayFromCardId(bigEnemy.Id);

                    // Hunter's mark the big enemy
                    AutomationActions.PlaySpellAt(huntermarkIdx, targetPlayIdx);

                    // Attack it
                    AutomationActions.AttackMinion(oneAttackIdx, targetPlayIdx);
                }
            }

            // Kill stuff with our weapon
            if (BasicPlayTracker.FriendlyWeapon != null && BasicPlayTracker.FriendlyWeapon.CanAttack == true)
            {
                // Have to attack taunts first
                InPlayCard tauntMinion = BasicPlayTracker.OpponentPlay.FirstOrDefault(c => c.HasMechanic("Taunt"));
                if(tauntMinion != null)
                {
                    AutomationActions.AttackWithHero(BasicPlayTracker.GetIndexInPlayFromCardId(tauntMinion.Id));
                }
                else
                {
                    foreach (InPlayCard enemy in BasicPlayTracker.OpponentPlay)
                    {
                        if (enemy.Health <= BasicPlayTracker.FriendlyWeapon.Attack)
                        {
                            AutomationActions.AttackWithHero(BasicPlayTracker.GetIndexInPlayFromCardId(enemy.Id));
                            BasicPlayTracker.FriendlyWeapon.CanAttack = false;
                            break;
                        }
                    }
                }
                if (BasicPlayTracker.FriendlyWeapon.CanAttack == true)
                {
                    AutomationActions.AttackHeroWithHero();
                }
            }
            GameStateTracker.Global.Update();

            // Kill guys that won't kill us
            bool keepTrying = true;
            while (keepTrying)
            {
                keepTrying = false;
                foreach (InPlayCard friendly in BasicPlayTracker.FriendlyPlay)
                {
                    bool doBreak = false;
                    foreach (InPlayCard enemy in BasicPlayTracker.OpponentPlay)
                    {
                        if (friendly.CanAttack && friendly.Attack >= enemy.Health && friendly.Health > enemy.Health && !enemy.HasMechanic("Stealth"))
                        {
                            int friendlyIdx = BasicPlayTracker.GetIndexInPlayFromCardId(friendly.Id);
                            int enemyIdx = BasicPlayTracker.GetIndexInPlayFromCardId(enemy.Id);

                            AutomationActions.AttackMinion(friendlyIdx, enemyIdx);
                            doBreak = true;
                            keepTrying = true;
                            break;
                        }
                    }

                    if (doBreak) break;
                }
                break;
            }
        }

        private void FillBoard()
        {
            while (true)
            {
                // Play a highmane over anything else
                InPlayCard highmane = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "Savannah Highmane");
                if (highmane != null && BasicPlayTracker.CanPlayCard(highmane))
                {
                    // Play him
                    AutomationActions.PlayCardById(highmane.Id);
                    continue;
                }

                // Try to play an undertaker first and trigger deathrattle proc if possible
                InPlayCard undertaker = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "Undertaker");
                if (undertaker != null && BasicPlayTracker.CanPlayCard(undertaker))
                {
                    AutomationActions.PlayCardById(undertaker.Id);

                    continue;
                }

                // Play all our deathrattles
                List<InPlayCard> deathrattles = BasicPlayTracker.FriendlyHand.FindAll(c => c.HasMechanic("Deathrattle"));
                deathrattles.Sort((a, b) => b.CardData.cost.CompareTo(a.CardData.cost));
                bool didPlay = false;
                foreach (InPlayCard card in deathrattles)
                {
                    if (BasicPlayTracker.CanPlayCard(card))
                    {
                        GlobalLogs.AILogs.Add("Playing DEATHRATTLE minion");
                        AutomationActions.PlayCardById(card.Id);
                        didPlay = true;
                    }
                }
                if (didPlay) continue;

                // Play whatever else we can, expensive guys first
                BasicPlayTracker.FriendlyHand.Sort((a, b) => a.CardData.name == "The Coin" ? -100 : a.CardData.cost.CompareTo(b.CardData.cost));
                List<InPlayCard> cardToPlay = new List<InPlayCard>();
                BasicPlayTracker.FriendlyHand.Sort((a, b) => a.ZonePos == b.ZonePos ? a.CreationId.CompareTo(b.CreationId) : a.ZonePos.CompareTo(b.ZonePos));
                foreach (InPlayCard c in BasicPlayTracker.FriendlyHand)
                {
                    if (c.CardData.name == "The Coin") continue;
                    cardToPlay.Add(c);
                }
                foreach (InPlayCard card in cardToPlay)
                {
                    if (BasicPlayTracker.CanPlayCard(card) && card.CardData.name != "Hunter's Mark" && card.CardData.name != "Kill Command" && card.CardData.name != "The Coin")
                    {
                        AutomationActions.PlayCardById(card.Id);
                        Thread.Sleep(1000);
                    }
                }

                GameStateTracker.Global.Update();

                // Only play the coin if it will enable another play
                InPlayCard theCoin = BasicPlayTracker.FriendlyHand.FirstOrDefault(c => c.CardData.name == "The Coin");
                if(theCoin != null)
                {
                    BasicPlayTracker.CurrentMana += 1;
                    foreach(InPlayCard c in BasicPlayTracker.FriendlyHand)
                    {
                        if(c.CardData.cost == BasicPlayTracker.CurrentMana && BasicPlayTracker.CanPlayCard(c))
                        {
                            // Play it
                            GlobalLogs.AILogs.Add("Playing the coin to enable " + c.CardData.name);
                            AutomationActions.PlayCardById(theCoin.Id);
                        }
                    }
                    BasicPlayTracker.CurrentMana -= 1;
                }

                // Play whatever else we can, expensive guys first
                BasicPlayTracker.FriendlyHand.Sort((a, b) => a.CardData.name == "The Coin" ? -100 : a.CardData.cost.CompareTo(b.CardData.cost));
                cardToPlay = new List<InPlayCard>();
                BasicPlayTracker.FriendlyHand.Sort((a, b) => a.ZonePos == b.ZonePos ? a.CreationId.CompareTo(b.CreationId) : a.ZonePos.CompareTo(b.ZonePos));
                foreach (InPlayCard c in BasicPlayTracker.FriendlyHand)
                {
                    if (c.CardData.name == "The Coin") continue;
                    cardToPlay.Add(c);
                }
                foreach (InPlayCard card in cardToPlay)
                {
                    if (BasicPlayTracker.CanPlayCard(card) && card.CardData.name != "Hunter's Mark" && card.CardData.name != "Kill Command")
                    {
                        AutomationActions.PlayCardById(card.Id);
                        Thread.Sleep(1000);
                    }
                }

                break;
            }
        }

        private void AttackFace()
        {
            foreach(InPlayCard c in BasicPlayTracker.FriendlyPlay.FindAll(c => c.CanAttack))
            {
                AutomationActions.AttackHero(BasicPlayTracker.GetIndexInPlayFromCardId(c.Id));
            }
        }
    }
}
