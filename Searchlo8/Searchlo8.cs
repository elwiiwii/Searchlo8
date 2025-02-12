using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Searchlo8
{
    public class Searchlo8
    {
        private int[] Solutions;
        private Pico8 p8;

        public Searchlo8(Pico8 pico8)
        {
            Solutions = [];
            p8 = pico8;

        }

        private int[] AllowableActions()
        {
            /*  button states
                0x000000 - 0 - no input
                0x000001 - 1 - l
                0x000010 - 2 - r
                0x000100 - 4 - u
                0x001000 - 8 - d
                0x010000 - 16 - z
                0x100000 - 32 - x
                0x000101 - 5 - l + u
                0x000110 - 6 - r + u
                0x001001 - 9 - l + d
                0x001010 - 10 - r + d
                0x010001 - 17 - l + z
                0x010010 - 18 - r + z
                0x010100 - 20 - u + z
                0x011000 - 24 - d + z
                0x100001 - 33 - l + x
                0x100010 - 34 - r + x
                0x100100 - 36 - u + x
                0x101000 - 40 - d + x
                0x010101 - 21 - l + u + z
                0x010110 - 22 - r + u + z
                0x011001 - 25 - l + d + z
                0x011010 - 26 - r + d + z
                0x100101 - 37 - l + u + x
                0x100110 - 38 - r + u + x
                0x101001 - 41 - l + d + x
                0x101010 - 42 - r + d + x
            */
            int[] actions = [0x000000, 0x000001,0x000010,0x000100,0x001000,0x010000,0x100000,
                    0x000101,0x000110,0x001001,0x001010,
                    0x010001,0x010010,0x010100,0x011000,
                    0x100001,0x100010,0x100100,0x101000,
                    0x010101,0x010110,0x011001,0x011010,
                    0x100101,0x100110,0x101001,0x101010];
            return actions;
        }

        private bool IsRip()
        {
            return p8._cart.Isdead;
        }

        private bool IsGoal()
        {
            return p8._cart.Isfinish;
        }

        private void Transition()
        {

        }
    }
}
