using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Force.DeepCloner;

namespace Searchlo8
{
    public class Searchlo8
    {
        private List<List<int>> Solutions;
        private Pico8 p8;
        private ConcurrentDictionary<List<int>, (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish)> _cache;

        public Searchlo8()
        {
            Solutions = [] ;
            p8 = new();
            _cache = [];
        }

        private (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) InitState()
        {
            p8.game.LoadLevel(3);
            return (p8.game.Entities, p8.game.Link1, p8.game.Isdead, p8.game.Isfinish);
        }

        public virtual int[] AllowableActions()
        {
            /*  button states
                0b000000 - 0 - no input
                0b000001 - 1 - l
                0b000010 - 2 - r
                0b000100 - 4 - u
                0b001000 - 8 - d
                0b010000 - 16 - z
                0b100000 - 32 - x
                0b000101 - 5 - l + u
                0b000110 - 6 - r + u
                0b001001 - 9 - l + d
                0b001010 - 10 - r + d
                0b010001 - 17 - l + z
                0b010010 - 18 - r + z
                0b010100 - 20 - u + z
                0b011000 - 24 - d + z
                0b100001 - 33 - l + x
                0b100010 - 34 - r + x
                0b100100 - 36 - u + x
                0b101000 - 40 - d + x
                0b010101 - 21 - l + u + z
                0b010110 - 22 - r + u + z
                0b011001 - 25 - l + d + z
                0b011010 - 26 - r + d + z
                0b100101 - 37 - l + u + x
                0b100110 - 38 - r + u + x
                0b101001 - 41 - l + d + x
                0b101010 - 42 - r + d + x
            */
            int[] actions = [0b000000, 0b000001,0b000010,0b000100,0b001000,0b010000,0b100000,
                            0b000101,0b000110,0b001001,0b001010,
                            0b010001,0b010010,0b010100,0b011000,
                            0b100001,0b100010,0b100100,0b101000,
                            0b010101,0b010110,0b011001,0b011010,
                            0b100101,0b100110,0b101001,0b101010];
            return actions;
        }

        private int Hcost((List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) state)
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

        private bool IsRip((List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) state)
        {
            return state.Isdead;
        }

        private bool IsGoal((List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) state)
        {
            return state.Isfinish;
        }

        private int[] GetActions()
        {
            return AllowableActions();
        }

        private (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) Transition((List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) state, int action)
        {
            p8.game.Entities = state.Item1.DeepClone();
            p8.game.Link1 = state.Item2.DeepClone();
            p8.game.Isdead = state.Item3.DeepClone();
            p8.game.Isfinish = state.Item4.DeepClone();
            p8.SetBtnState(action);
            p8.Step();
            return (p8.game.Entities, p8.game.Link1, p8.game.Isdead, p8.game.Isfinish);
        }

        private bool Iddfs((List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) state, int depth, List<int> inputs)
        {
            if (depth == 0 && IsGoal(state))
            {
                Solutions.Add(inputs);
                Console.WriteLine($"  inputs: {inputs}\n  frames: {inputs.Count - 1}");
                return true;
            }
            else
            {
                bool optimal_depth = false;
                if (depth > 0 && Hcost(state) <= depth)
                {
                    foreach (int action in GetActions())
                    {
                        (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) new_state = Transition(state, action);
                        inputs.Add(action);
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

        private bool DepthCheck(int depth)
        {
            List<List<int>> keys = [];
            foreach (List<int> key in _cache.Keys)
            {
                if (key.Count == depth)
                {
                    keys.Add(key);
                }
            }
            Dictionary<List<int>, (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish)> states = [];
            foreach (List<int> key in keys)
            {
                foreach (int action in GetActions())
                {
                    List<int> new_key = new(key);
                    new_key.Add(action);
                    states.Add(new_key, _cache[key]);
                }
            }
            _cache = [];
            Parallel.ForEach(states.Keys, index =>
            {
                (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) curstate = Transition(states[index], index[^1]);
                if (!curstate.Isdead)
                {
                    _cache.TryAdd(index, curstate);
                }
                if (curstate.Isfinish)
                {
                    Solutions.Append(index);
                    Console.WriteLine($"  inputs: {index}\n  frames: {index.Count - 1}");
                }
            });
            return true;
        }

        public List<List<int>> Search(int max_depth, bool complete = false)
        {
            Solutions = [];
            DateTime timer = DateTime.Now;
            (List<Cyclo8.EntityClass>, Cyclo8.LinkClass, bool Isdead, bool Isfinish) state = InitState();
            _cache.TryAdd([0], state);
            Console.WriteLine("searching...");
            for (int i = 1; i <= max_depth; i++)
            {
                Console.WriteLine($"depth {i}...");
                //bool done = Iddfs(state, i, []) && !complete;
                bool done = DepthCheck(i) && !complete;
                Console.WriteLine($" elapsed time: {DateTime.Now - timer} [s]");
                if (done)
                {
                    break;
                }
            }
            return Solutions;
        }

        public string InputsToEnglish(List<int> inputs)
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
