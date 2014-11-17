using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Super;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HearthstoneLogReader;

namespace HearthstoneBot
{
    public class PlayAI : Singleton<PlayAI>
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref AutomationActions.Rect rectangle);

        private int internalTurn = -1;

        public void Update()
        {
            Process[] procs = Process.GetProcessesByName("Hearthstone");

            if (procs.Count() > 0)
            {
                Process hsProc = procs[0];

                WindowFocusing.ActivateWindow(hsProc.MainWindowHandle);

                GetWindowRect(hsProc.MainWindowHandle, ref AutomationActions.GameRect);
            }

            GameStateTracker.Global.Update();

            if(internalTurn == -1)
            {
                internalTurn = BasicPlayTracker.CurrentTurn;
            }

            // Handle mulligans
            if(BasicPlayTracker.CurrentGameState == BasicPlayTracker.GameState.Mulliganing)
            {
                // Wait for some cards in hand
                while(BasicPlayTracker.FriendlyHand.Count == 0)
                {
                    GameStateTracker.Global.Update();
                    Thread.Sleep(1000);
                }
                // Sleep for animation to popup
                Thread.Sleep(15000);
                GameStateTracker.Global.Update();
                BasicPlayTracker.CurrentHero.PerformMulligans();
                
                // Wait until mulligans are over
                while(BasicPlayTracker.CurrentGameState == BasicPlayTracker.GameState.Mulliganing)
                {
                    Thread.Sleep(1000);
                    GameStateTracker.Global.Update();

                    internalTurn = 1;
                }
            }
            else if(BasicPlayTracker.CurrentGameState == BasicPlayTracker.GameState.Playing && BasicPlayTracker.CurrentTurn >= internalTurn)
            {
                // Sleep for animations to settle
                Thread.Sleep(5000);
                BasicPlayTracker.CurrentHero.ProcessTurn();

                // Wait for turn to end
                while(BasicPlayTracker.CurrentGameState == BasicPlayTracker.GameState.Playing)
                {
                    Thread.Sleep(1000);
                    GameStateTracker.Global.Update();
                }

                internalTurn++;
            }
        }
    }
}
