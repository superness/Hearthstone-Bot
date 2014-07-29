using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Super;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

        public void Update()
        {
            // Click some stuff

            if(PlayTracker.Global.State == PlayTracker.GameState.MyTurn)
            {
                Thread.Sleep(5000);
                
                Process[] procs = Process.GetProcessesByName("Hearthstone");
                Process hsProc = procs[0];

                WindowFocusing.ActivateWindow(hsProc.MainWindowHandle);

                GetWindowRect(hsProc.MainWindowHandle, ref this.GameRect);
            }
        }

        private void PassTurn()
        {
            this.MoveClickWrapper(1092, 386);
            PlayTracker.Global.PassedTurn();
        }

        private void MoveClickWrapper(uint x, uint y)
        {
            SuperInputSim.MoveMouseTo((uint)(x + this.GameRect.Left), (uint)(y + this.GameRect.Top));
            SuperInputSim.SendClick((uint)(x + this.GameRect.Left), (uint)(y + this.GameRect.Top), ClickFlags.LeftClick);
        }
    }
}
