using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    public static class HearthstoneEventCallbacks
    {
        public static void OnNextTurn()
        {
            BasicPlayTracker.AdvanceTurn();
            LogEvent("[Next turn]", BasicPlayTracker.IsFriendlyTurn ? "Friendly" : "Opponent", 0);
        }

        public static void OnFriendlyHero(ZoneChange zc)
        {
            LogEvent("[Friendly hero]", zc.name, zc.zonePos);
            BasicPlayTracker.AddFriendlyHero(zc.cardId, zc.id);
        }

        public static void OnOpponentHero(ZoneChange zc)
        {
            LogEvent("[Opponent hero]", zc.name, zc.zonePos);
            BasicPlayTracker.AddOpponentHero(zc.cardId, zc.id);
        }

        public static void OnFriendlyDraw(ZoneChange zc)
        {
            LogEvent("[Friendly drew]", zc.name, zc.zonePos);
            BasicPlayTracker.AddFriendlyHand(zc.cardId, zc.id);
        }

        public static void OnOpponentDraw(ZoneChange zc)
        {
            LogEvent("[Opponent drew]", zc.id.ToString(), zc.zonePos);
            BasicPlayTracker.AddOpponentHand(zc.id);
        }

        public static void OnFriendlyMulligan(ZoneChange zc)
        {
            LogEvent("[Friendly mulliganed]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveFriendlyHand(zc.id);
        }

        public static void OnOpponentMulligan(ZoneChange zc)
        {
            LogEvent("[Opponent mulliganed]", zc.id.ToString(), zc.zonePos);
            BasicPlayTracker.RemoveOpponentHand(zc.id);
        }

        public static void OnFriendlySecretPlayed(ZoneChange zc)
        {
            LogEvent("[Friendly played secret]", zc.name, zc.zonePos);
            BasicPlayTracker.AddFriendlySecret(zc.cardId, zc.id);
        }

        public static void OnOpponentSecretPlayed(ZoneChange zc)
        {
            LogEvent("[Opponent played secret]", zc.name, zc.zonePos);
            BasicPlayTracker.AddOpponentSecret(zc.id);
        }

        public static void OnFriendlySecretTriggered(ZoneChange zc)
        {
            LogEvent("[Friendly triggered secret]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveFriendlySecret(zc.id);
        }

        public static void OnOpponentSecretTriggered(ZoneChange zc)
        {
            LogEvent("[Opponent triggered secret]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveOpponentSecret(zc.id);
        }

        public static void OnFriendlyPlayedMinion(ZoneChange zc)
        {
            LogEvent("[Friendly played minion]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveFriendlyHand(zc.id);
            BasicPlayTracker.AddFriendlyPlay(zc.cardId, zc.id);
        }

        public static void OnOpponentPlayedMinion(ZoneChange zc)
        {
            LogEvent("[Opponent played minion]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveOpponentHand(zc.id);
            BasicPlayTracker.AddOpponentPlay(zc.cardId, zc.id);
        }

        public static void OnEffectGaveFriendlyMinion(ZoneChange zc)
        {
            LogEvent("[Effect gave friendly minion]", zc.name, zc.zonePos);
            BasicPlayTracker.AddFriendlyPlay(zc.cardId, zc.id);
        }

        public static void OnEffectGaveOpponentMinion(ZoneChange zc)
        {
            LogEvent("[Effect gave opponent minion]", zc.name, zc.zonePos);
            BasicPlayTracker.AddOpponentPlay(zc.cardId, zc.id);
        }

        public static void OnFriendlyPlayedSpell(ZoneChange zc)
        {
            LogEvent("[Friendly played spell]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveFriendlyHand(zc.id);
        }

        public static void OnOpponentPlayedSpell(ZoneChange zc)
        {
            LogEvent("[Opponent played spell]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveOpponentHand(zc.id);
        }

        public static void OnFriendlyMinionDied(ZoneChange zc)
        {
            LogEvent("[Friendly minion died]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveFriendlyPlay(zc.name, zc.id);
        }

        public static void OnOpponentMinionDied(ZoneChange zc)
        {
            LogEvent("[Opposing minon died]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveOpponentPlay(zc.name, zc.id);
        }

        public static void OnFriendlyMinionExiled(ZoneChange zc)
        {
            LogEvent("[Friendly minon exiled]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveFriendlyPlay(zc.name, zc.id);
        }

        public static void OnOpponentMinionExiled(ZoneChange zc)
        {
            LogEvent("[Opposing minon exiled]", zc.name, zc.zonePos);
            BasicPlayTracker.RemoveOpponentPlay(zc.name, zc.id);
        }

        public static void OnFriendlyWeaponDestroyed(ZoneChange zc)
        {
            LogEvent("[Friendly weapon destroyed]", zc.name, zc.zonePos);
            BasicPlayTracker.FriendlyWeapon = null;
        }

        public static void OnOpponentWeaponDestroyed(ZoneChange zc)
        {
            LogEvent("[Opponent weapon destroyed]", zc.name, zc.zonePos);
            BasicPlayTracker.OpponentWeapon = null;
        }

        public static void OnGameEnd()
        {
            BasicPlayTracker.Reset();
            BasicPlayTracker.CurrentGameState = BasicPlayTracker.GameState.EndGameScreen;
            LogEvent("[GameEnd]", string.Empty, 0);
        }

        public static void OnWin()
        {
            BasicPlayTracker.Reset();
            LogEvent("[GameWin]", string.Empty, 0);
        }

        public static void OnLoss()
        {
            BasicPlayTracker.Reset();
            LogEvent("[GameLoss]", string.Empty, 0);
        }

        private static void LogEvent(string eventType, string value, int zonePos)
        {
            GlobalLogs.ZoneChanges.Add(string.Format("{0,-50}: {1} @ {2}", eventType, value, zonePos));
        }
    }
}
