using System.Runtime.CompilerServices;
using Force.DeepCloner;

namespace Searchlo8
{
    public class Searchlo8
    {
        private int[][] Solutions;
        private Pico8 p8;
        int count;

        public Searchlo8()
        {
            Solutions = [] ;
            p8 = new();
            count = 0;
        }

        private Cyclo8 InitState()
        {
            p8.game.LoadLevel(3);
            return p8.game;
        }

        public virtual int[] AllowableActions()
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

        private int Hcost(Cyclo8 state)
        {
            if (IsRip(state))
            {
                return int.MaxValue;
            }
            else
            {
                return 0;
            }
        }

        private bool IsRip(Cyclo8 state)
        {
            return state.Isdead;
        }

        private bool IsGoal(Cyclo8 state)
        {
            return state.Isfinish;
        }

        private int[] GetActions()
        {
            return AllowableActions();
        }

        private Cyclo8 Transition(Cyclo8 state, int action)
        {
            p8.game.Entities = state.Entities.DeepClone();
            p8.game.Items = state.Items.DeepClone();
            p8.game.Link1 = state.Link1.DeepClone();
            p8.SetBtnState(action);
            p8.Step();
            count += 1;
            //Console.WriteLine($"{count}");
            return p8.game;
        }

        private bool Iddfs(Cyclo8 state, int depth, int[] inputs)
        {
            if (depth == 0 && IsGoal(state))
            {
                Solutions.Append(inputs);
                Console.WriteLine($"  inputs: {inputs}\n  frames: {inputs.Length - 1}");
                return true;
            }
            else
            {
                bool optimal_depth = false;
                if (depth > 0 && Hcost(state) <= depth)
                {
                    foreach (int action in GetActions())
                    {
                        Cyclo8 new_state = Transition(state, action);
                        inputs.Append(action);
                        bool done = Iddfs(new_state, depth - 1, inputs);
                        if (done)
                        {
                            optimal_depth = true;
                        }
                    }
                }
                return optimal_depth;
            }
        }

        public int[][] Search(int max_depth, bool complete = false)
        {
            Solutions = [];
            DateTime timer = DateTime.Now;
            Cyclo8 state = InitState();
            Console.WriteLine("searching...");
            for (int i = 1; i <= max_depth; i++)
            {
                Console.WriteLine($"depth {i}...");
                bool done = Iddfs(state, i, []) && !complete;
                Console.WriteLine($" elapsed time: {DateTime.Now - timer} [s]");
                if (done)
                {
                    break;
                }
            }
            return Solutions;
        }

        public string InputsToEnglish(int[] inputs)
        {
            Dictionary<int, string> action_dict = new()
            {
                { 0, "no input" },
                { 1, "left" },
                { 2, "right" },
                { 4, "up" },
                { 8, "down" },
                { 16, "z" },
                { 32, "x" },
                { 5, "left + up" },
                { 6, "right + up" },
                { 9, "left + down" },
                { 10, "right + down" },
                { 17, "left + z" },
                { 18, "right + z" },
                { 20, "up + z" },
                { 24, "down + z" },
                { 33, "left + x" },
                { 34, "right + x" },
                { 36, "up + x" },
                { 40, "down + x" },
                { 21, "left + up + z" },
                { 22, "right + up + z" },
                { 25, "left + down + z" },
                { 26, "right + down + z" },
                { 37, "left + up + x" },
                { 38, "right + up + x" },
                { 41, "left + down + x" },
                { 42, "right + down + x" }
            };

            string s = "";
            foreach (int i in inputs)
            {
                s.Concat($", {action_dict[i]}");
            }
            return s;
        }

    }
}
