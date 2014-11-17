using HearthstoneBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    public class GameStateTracker : Singleton<GameStateTracker>
    {
        private static List<ZoneChange> zoneChanges = new List<ZoneChange>();
        private static List<EntityUpdate> entityUpdates = new List<EntityUpdate>();

        private static int powerCount = 0;
        private static int powerTurnLimit = 14; // WOoooooo magic

        private static long currentIndex = 0;
        private static bool first = true;

        private static string logFile = @"C:\Program Files (x86)\Hearthstone\Hearthstone_Data\output_log.txt";

        public void Update()
        {
            // Reset data
            zoneChanges.Clear();
            entityUpdates.Clear();

            DoLogging();

            if (first)
            {
                // Find the last game end
                using (FileStream fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    currentIndex = FindLastGameEnd(fs);
                    first = false;
                }
            }

            // Load log 
            using (FileStream fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(currentIndex, SeekOrigin.Begin);

                if (fs.Length == currentIndex)
                {
                    Thread.Sleep(1000);
                    return;
                }

                // Parse lines
                using (StreamReader sr = new StreamReader(fs))
                {
                    string newLines = sr.ReadToEnd();
                    if (!newLines.EndsWith("\n"))
                    {
                        //hearthstone log apparently does not append full lines
                        Thread.Sleep(1000);
                        return;
                    }

                    string[] lines = newLines.Split('\n');

                    // Output what happened
                    foreach (string s in lines)
                    {
                        ProcessLogLine(s);
                    }

                    currentIndex = fs.Length;

                    DoLogging();
                }

                foreach (EntityUpdate eu in entityUpdates)
                {
                    BasicPlayTracker.UpdateCardZonePos(eu.id, eu.zonePos);
                    //Console.WriteLine(string.Format("{0} {1} in {2} at {3}", eu.id, eu.name, eu.zone, eu.zonePos));
                }
            }
        }

        private static void DoLogging()
        {

            // const logging locations
            const int zoneChangedLogAt = 0;
            const int entityUpdatesLogAt = 21;
            const int cardTargetsLogAt = 32;
            const int sendOptionsLogAt = 43;
            const int declaredAttackersLogAt = 54;
            const int aiLogsAt = 65;
            const int cardPositionsStatusLogAt = 76;

            // Log zone changes
            Console.Clear();
            int j = 0;
            for (int i = Math.Max(0, GlobalLogs.ZoneChanges.Count - 20); i < GlobalLogs.ZoneChanges.Count; i++, j++)
            {
                if (GlobalLogs.ZoneChanges[i].ToLower().Contains("friendly"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.SetCursorPosition(0, zoneChangedLogAt + j);
                Console.WriteLine(GlobalLogs.ZoneChanges[i]);
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            PrintLogList(GlobalLogs.EntityUpdates, entityUpdatesLogAt, 10);

            Console.ForegroundColor = ConsoleColor.Yellow;
            PrintLogList(GlobalLogs.CardTargets, cardTargetsLogAt, 10);

            Console.ForegroundColor = ConsoleColor.Cyan;
            PrintLogList(GlobalLogs.SendOptions, sendOptionsLogAt, 10);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.BackgroundColor = ConsoleColor.White;
            PrintLogList(GlobalLogs.DeclaredAttackers, declaredAttackersLogAt, 10);
            Console.BackgroundColor = ConsoleColor.Black;

            Console.ForegroundColor = ConsoleColor.Red;
            PrintLogList(GlobalLogs.AILogs, aiLogsAt, 10);

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, cardPositionsStatusLogAt);
            Console.WriteLine(BasicPlayTracker.OpponentStatus);
            Console.WriteLine(BasicPlayTracker.FriendlyStatus);
            Console.WriteLine(BasicPlayTracker.FriendlyHandStatus);
            Console.WriteLine(BasicPlayTracker.CurrentGameState);
            Console.WriteLine(string.Format("{0}/{1} @ turn {2}({3})", BasicPlayTracker.CurrentMana, BasicPlayTracker.CurrentMaxMana, BasicPlayTracker.CurrentTurn, BasicPlayTracker.TotalTurns));
        }

        private static void PrintLogList(List<string> logs, int startAtY, int num)
        {
            int j = 0;
            for (int i = Math.Max(0, logs.Count - num); i < logs.Count; i++, j++)
            {
                Console.SetCursorPosition(0, startAtY + j);
                Console.WriteLine(logs[i]);
            }
        }

        private static long FindLastGameEnd(FileStream fs)
        {
            using (var sr = new StreamReader(fs))
            {
                long offset = 0, tempOffset = 0;
                var lines = sr.ReadToEnd().Split('\n');

                foreach (var line in lines)
                {
                    tempOffset += line.Length + 1;
                    if (line.StartsWith("[Bob] legend rank"))
                        offset = tempOffset;
                }

                return offset;
            }
        }

        private static void ProcessZoneChange(ZoneChange zc)
        {

            if ((zc.from.Contains("DECK") || zc.from.Contains("GRAVEYARD") || zc.from == string.Empty) && zc.to.Contains("HAND"))
            {
                if (zc.from.Contains("DECK") && powerCount >= powerTurnLimit)
                {
                    if(BasicPlayTracker.IsFriendlyTurn != zc.to.Contains("FRIENDLY"))
                    {
                        HearthstoneEventCallbacks.OnNextTurn();
                    }
                }
                if (zc.to.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlyDraw(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentDraw(zc);
                }
                //BGlobalLogs.ZoneChanges.Add("DREW with power " + powerCount + " turn = " + BasicPlayTracker.IsFriendlyTurn);
            }
            else if (zc.from.Contains("HAND") && zc.to.Contains("DECK"))
            {
                if (zc.from.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlyMulligan(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentMulligan(zc);
                }
            }
            else if (zc.to.Contains("FRIENDLY PLAY (Hero)"))
            {
                HearthstoneEventCallbacks.OnFriendlyHero(zc);
            }
            else if (zc.to.Contains("FRIENDLY PLAY (Hero Power)"))
            {
                LogEvent("[Friendly hero power]", zc.name);
            }
            else if (zc.to.Contains("OPPOSING PLAY (Hero)"))
            {
                HearthstoneEventCallbacks.OnOpponentHero(zc);
            }
            else if (zc.to.Contains("OPPOSING PLAY (Hero Power)"))
            {
                LogEvent("[Opossing hero power]", zc.name);
            }
            else if (zc.to.Contains("DECK"))
            {

            }
            else if(zc.to.Contains("SECRET"))
            {
                if(zc.to.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlySecretPlayed(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentSecretPlayed(zc);
                }
            }
            else if(zc.from.Contains("SECRET"))
            {
                if(zc.from.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlySecretTriggered(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentSecretTriggered(zc);
                }
            }
            else if (zc.to.Contains("PLAY"))
            {
                // Got into play from hand then I am a minion
                if (zc.from.Contains("HAND"))
                {
                    if (zc.to.Contains("FRIENDLY"))
                    {
                        HearthstoneEventCallbacks.OnFriendlyPlayedMinion(zc);
                    }
                    else
                    {
                        HearthstoneEventCallbacks.OnOpponentPlayedMinion(zc);
                    }
                }
                // Otherwise I came from an effect
                else
                {
                    if (zc.to.Contains("FRIENDLY"))
                    {
                        HearthstoneEventCallbacks.OnEffectGaveFriendlyMinion(zc);
                    }
                    else
                    {
                        HearthstoneEventCallbacks.OnEffectGaveOpponentMinion(zc);
                    }
                }
            }
            else if (zc.from.Contains("HAND"))
            {
                // From hand but not to play is a spell
                if (zc.from.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlyPlayedSpell(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentPlayedSpell(zc);
                }
            }
            else if (zc.from.Contains("PLAY") && zc.to.Contains("GRAVEYARD"))
            {
                if (zc.from.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlyMinionDied(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentMinionDied(zc);
                }
            }
            else if (zc.to.Contains("GRAVEYARD"))
            {
                JsonCard card = Program.Cards.GetCardFromCardId(zc.cardId);

                if(card.type == "Weapon")
                {
                    if(zc.to.Contains("FRIENDLY"))
                    {
                        HearthstoneEventCallbacks.OnFriendlyWeaponDestroyed(zc);
                    }
                    else
                    {
                        HearthstoneEventCallbacks.OnOpponentWeaponDestroyed(zc);
                    }
                }

            }
            else if (zc.from.Contains("PLAY"))
            {
                if (zc.from.Contains("FRIENDLY"))
                {
                    HearthstoneEventCallbacks.OnFriendlyMinionExiled(zc);
                }
                else
                {
                    HearthstoneEventCallbacks.OnOpponentMinionExiled(zc);
                }
            }
            else
            {
                GlobalLogs.ZoneChanges.Add(string.Format("[UNDOCUMENTED] {0} {1} from {2} to {3}", zc.id, zc.name, zc.from, zc.to));
            }
        }

        private static void LogEvent(string eventType, string value)
        {
            GlobalLogs.ZoneChanges.Add(string.Format("{0,-50}: {1}", eventType, value));
        }

        private static string FindEqualsValue(string line, string param)
        {
            int idx1 = line.IndexOf(param) + param.Length;
            int idx2 = line.IndexOf('=', idx1) != -1 ? line.IndexOf('=', idx1) : line.Length;
            string p1 = line.Substring(idx1, idx2 - idx1);
            string p2 = p1.Substring(0, p1.LastIndexOf(' ') != -1 ? p1.LastIndexOf(' ') : p1.Length);

            if (p2.EndsWith("]"))
            {
                p2 = p2.Substring(0, p2.Length - 1);
            }
            return p2;
        }

        private static void ProcessLogLine(string line)
        {
            if (line.StartsWith("[Power]"))
            {
                powerCount++;

                //[Power] GameState.SendOption() - selectedOption=8 selectedSubOption=-1 selectedTarget=38 selectedPosition=0
                if (line.StartsWith("[Power] GameState.SendOption()"))
                {
                    string selectedOption = FindEqualsValue(line, "selectedOption=");
                    string selectedSubOption = FindEqualsValue(line, "selectedSubOption=");
                    string selectedTarget = FindEqualsValue(line, "selectedTarget=");
                    string selectedPosition = FindEqualsValue(line, "selectedPosition=");

                    InPlayCard target = BasicPlayTracker.FindInPlayCard(int.Parse(selectedTarget));

                    if (target != null)
                    {
                        GlobalLogs.SendOptions.Add(string.Format("Selected Option={0}, Selected SubOption={1}, Selected Target={2}-{3}, SelectedPosition={4}", selectedOption, selectedSubOption, target.CardData.name, target.Id, selectedPosition));
                    }
                    else
                    {
                        GlobalLogs.SendOptions.Add(string.Format("Selected Option={0}, Selected SubOption={1}, Selected Target={2}, SelectedPosition={3}", selectedOption, selectedSubOption, selectedTarget, selectedPosition));
                    }
                }
            }
            else if (line.Contains("tag=NEXT_STEP value=MAIN_ACTION"))
            {
                //HearthstoneEventCallbacks.OnNextTurn();
            }
            //[FaceDownCard] Card.SetDoNotSort() - card=[name=Harvest Golem id=47 zone=PLAY zonePos=1 cardId=EX1_556 player=2] bOn=False
            else if (line.StartsWith("[FaceDownCard]"))
            {
                if (line.StartsWith("[FaceDownCard] Card.SetDoNotSort()"))
                {

                    string id = FindEqualsValue(line, "id=");

                    InPlayCard card = BasicPlayTracker.FindInPlayCard(int.Parse(id));

                    GlobalLogs.DeclaredAttackers.Add(string.Format("{0}-{1} made an attack", card != null ? card.CardData.name : "NA", id));
                }
            }
            else if (line.StartsWith("[Zone] ZoneChangeList.ProcessChanges()"))
            {
                line = line.Substring("[Zone] ZoneChangeList.ProcessChanges()".Length);
                //id=132 local=False [name=Faceless Manipulator id=10 zone=HAND zonePos=0 cardId=EX1_564 player=1] zone from FRIENDLY DECK -> FRIENDLY HAND
                if (line.Contains("zone from"))
                {
                    string bracketedInfo = line.Substring(line.IndexOf("[") + 1, line.IndexOf("]") - line.IndexOf("[") - 1);
                    string zoneInfo = line.Substring(line.IndexOf("zone from") + "zone from ".Length);

                    string from = zoneInfo.Substring(0, zoneInfo.IndexOf("->") - 1);
                    string to = zoneInfo.Substring(zoneInfo.IndexOf("->") + "->".Length + 1);
                    string name = FindEqualsValue(bracketedInfo, "name=");
                    string id = FindEqualsValue(bracketedInfo, "id=");
                    string cardId = FindEqualsValue(bracketedInfo, "cardId=");
                    string zonePos = FindEqualsValue(bracketedInfo, "zonePos=");

                    ZoneChange change = new ZoneChange();
                    change.from = from;
                    change.to = to;
                    change.name = name;
                    change.cardId = cardId;
                    change.id = int.Parse(id);
                    change.zonePos = int.Parse(zonePos);

                    ProcessZoneChange(change);

                    powerCount = 0;

                    zoneChanges.Add(change);
                }
                // Card targets ability
                //[Zone] ZoneChangeList.ProcessChanges() - processing index=7 change=powerTask=[power=[type=TAG_CHANGE entity=[id=55 cardId=CS2_189 name=Elven Archer] tag=CARD_TARGET value=85] complete=False] entity=[name=Elven Archer id=55 zone=PLAY zonePos=1 cardId=CS2_189 player=2] srcZoneTag=INVALID srcPos= dstZoneTag=INVALID dstPos=
                else if (line.Contains("tag=CARD_TARGET"))
                {
                    // Snip inner []
                    int idx1 = line.IndexOf('[');
                    idx1 = line.IndexOf('[', idx1 + 1);
                    int idx2 = line.IndexOf(']');
                    idx2 = line.IndexOf(']', idx2 + 1);

                    line = line.Substring(idx1, idx2);

                    string id = FindEqualsValue(line, "id=");
                    string cardId = FindEqualsValue(line, "cardId=");
                    string name = FindEqualsValue(line, "name=");
                    string value = FindEqualsValue(line, "value=");

                    // What is it? Does it target an entity in play? Player?
                    JsonCard card = Program.Cards.GetCardFromCardId(cardId);
                    InPlayCard target = BasicPlayTracker.FindInPlayCard(int.Parse(value));

                    GlobalLogs.CardTargets.Add(string.Format("{0} targeted {1}-{2}", card.name, target.CardData.name, target.Id));
                }
            }
            else if (line.StartsWith("[Bob] legend rank"))
            {
                HearthstoneEventCallbacks.OnGameEnd();
            }
            else if (line.StartsWith("[Asset]"))
            {
                if (line.ToLower().Contains("victory_screen_start"))
                    HearthstoneEventCallbacks.OnWin();
                else if (line.ToLower().Contains("defeat_screen_start"))
                    HearthstoneEventCallbacks.OnLoss();
            }
            if (line.Contains("zonePos="))
            {
                // Expand out to [] and parse info

                int idx1 = line.IndexOf("zonePos=");
                if (idx1 == -1) return;
                int idx2 = line.Substring(0, idx1).LastIndexOf('[');
                if (idx2 == -1) return;
                int idx3 = line.Substring(idx1).IndexOf(']') + idx1 - idx2;
                if (idx3 == -1) return;
                line = line.Substring(idx2, idx3);

                // [name=Wrath of Air Totem id=103 zone=PLAY zonePos=4 cardId=CS2_052 player=1

                string name = FindEqualsValue(line, "name=");
                string id = FindEqualsValue(line, "id=");
                string zone = FindEqualsValue(line, "zone=");
                string zonePos = FindEqualsValue(line, "zonePos=");
                string cardId = FindEqualsValue(line, "cardId=");
                string player = FindEqualsValue(line, "player=");

                EntityUpdate update = new EntityUpdate();
                update.name = name;
                update.id = int.Parse(id);
                update.zone = zone;
                update.zonePos = int.Parse(zonePos);
                update.cardId = cardId;
                if (int.TryParse(player, out update.player))
                {
                    entityUpdates.Add(update);
                }
            }
        }
    }
}
