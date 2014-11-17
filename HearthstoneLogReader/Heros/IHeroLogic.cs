using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneLogReader
{
    public interface IHeroLogic
    {
        void PerformMulligans();
        void ProcessTurn();
    }
}
