using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore2.PoEMemory.Elements.AtlasElements;
using static System.Windows.Forms.AxHost;

namespace cOverlay
{
    public static class Utility
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private static bool[] _previousKeyStates = new bool[256];

        private static bool IsKeyPressed(Keys key)
        {
            return (GetAsyncKeyState((int)key) & 0x8000) != 0;
        }

        public static bool IsInScreen(AtlasNodeDescription node, State state)
        {
            var nodeElement = node.Element.Center;
            return nodeElement.X > 0 && nodeElement.X < state.borderX && nodeElement.Y > 0 && nodeElement.Y < state.borderY;
        }

        public static bool IsKeyPressedOnce(Keys key)
        {
            int vKey = (int)key;
            bool isCurrentlyPressed = (GetAsyncKeyState(vKey) & 0x8000) != 0;

            // Key was just pressed (current=true, previous=false)
            bool triggered = isCurrentlyPressed && !_previousKeyStates[vKey];

            _previousKeyStates[vKey] = isCurrentlyPressed; // Update state
            return triggered;
        }
    }
}