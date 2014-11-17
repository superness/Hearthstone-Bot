using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    public static class GlobalLogs
    {
        public static List<string> ZoneChanges = new List<string>();
        public static List<string> EntityUpdates = new List<string>();
        public static List<string> CardTargets = new List<string>();
        public static List<string> SendOptions = new List<string>();
        public static List<string> DeclaredAttackers = new List<string>();
        public static List<string> AILogs = new List<string>();
    }
}
