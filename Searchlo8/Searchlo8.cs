using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FixMath;
using Force.DeepCloner;

namespace Searchlo8;

public class Searchlo8
{
    private ConcurrentDictionary<List<int>, GameState> _cache;
    private Cyclo8.ItemClass endflag;
    private Pico8 p8;
    private List<List<int>> Solutions;
    private Cyclo8.ItemClass startflag;

    private byte[] _pathImageData;
    private int _pathImageStride;
    private PixelFormat _pathImagePixelFormat;
    private int _pathImageWidth;
    private int _pathImageHeight;

    public Searchlo8()
    {
        _cache = new();
        p8 = new();
        InitPathImage();
        Solutions = [];
    }

    private void InitPathImage()
    {
        using (var pathImage = new Bitmap("Paths/lvl3route4.bmp"))
        {
            _pathImageWidth = pathImage.Width;
            _pathImageHeight = pathImage.Height;
            var rect = new Rectangle(0, 0, pathImage.Width, pathImage.Height);
            var bitmapData = pathImage.LockBits(rect, ImageLockMode.ReadOnly, pathImage.PixelFormat);

            _pathImageStride = bitmapData.Stride;
            _pathImagePixelFormat = pathImage.PixelFormat;
            _pathImageData = new byte[bitmapData.Stride * pathImage.Height];

            Marshal.Copy(bitmapData.Scan0, _pathImageData, 0, _pathImageData.Length);
            pathImage.UnlockBits(bitmapData);
        }
    }

    private GameState InitState()
    {
        p8.game.LoadLevel(3);
        startflag = p8.game.Items.Find(item => item.Type == 3);
        endflag = p8.game.Items.Find(item => item.Type == 4);
        var state = new GameState(p8.game.Entities[0], p8.game.Entities[1], p8.game.Link1, p8.game.Items, p8.game.Isdead, p8.game.Isfinish);
        return state;
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

    private bool Hcost(GameState state, int depth, int maxdepth)
    {
        if (IsRip(state))
        {
            return false;
        }
        else
        {
            //return PathFromImage(state.Wheel0, state.Wheel1, 3, Color.FromArgb(0, 0, 255, 0), 2);
            return true;
        }
    }

    private bool IsRip(GameState state)
    {
        return state.IsDead;
    }

    private bool ExitHeuristic(GameState state, int depth, int maxdepth)
    {
        var lvlrange = endflag.Y - startflag.Y;

        var playerpercentage = (F32.FromRaw(state.Wheel0.Y) - startflag.Y) / lvlrange * 100;
        var depthpercentage = Math.Max(depth - 65, 0) / maxdepth * 100;
        return playerpercentage + 10 >= depthpercentage;
    }

    private bool IsGoal(GameState state)
    {
        return state.IsFinish;
    }

    private int[] GetActions()
    {
        return AllowableActions();
    }

    unsafe private GameState Transition(GameState s, int action)
    {
        var link = new Cyclo8.LinkClass(1, 2) { Length = F32.FromRaw(s.Link.Length), Dirx = F32.FromRaw(s.Link.Dirx), Diry = F32.FromRaw(s.Link.Diry) };
        var entities = new List<Cyclo8.EntityClass>
        {
            new(F32.FromRaw(s.Wheel0.X), F32.FromRaw(s.Wheel0.Y)) { Vx = F32.FromRaw(s.Wheel0.Vx), Vy = F32.FromRaw(s.Wheel0.Vy), Rot = F32.FromRaw(s.Wheel0.Rot), Vrot = F32.FromRaw(s.Wheel0.Vrot), Isflying = s.Wheel0.IsFlying, Link = link, Linkside = s.Wheel0.Linkside },
            new(F32.FromRaw(s.Wheel1.X), F32.FromRaw(s.Wheel1.Y)) { Vx = F32.FromRaw(s.Wheel1.Vx), Vy = F32.FromRaw(s.Wheel1.Vy), Rot = F32.FromRaw(s.Wheel1.Rot), Vrot = F32.FromRaw(s.Wheel1.Vrot), Isflying = s.Wheel1.IsFlying, Link = link, Linkside = s.Wheel1.Linkside }
        };
        
        var items = new List<Cyclo8.ItemClass>();
        for (int i = 0; i < 30; i++)
        {
            items.Add(new Cyclo8.ItemClass(F32.FromRaw(s.ItemsX[i]), F32.FromRaw(s.ItemsY[i]), s.ItemsType[i]) { Active = s.ItemsActive[i] });
        }

        p8.game.Entities = entities;
        p8.game.Link1 = link;
        p8.game.Items = items;
        p8.game.Isdead = s.IsDead;
        p8.game.Isfinish = s.IsFinish;

        p8.SetBtnState(action);
        p8.Step();

        var newState = new GameState(p8.game.Entities[0], p8.game.Entities[1], p8.game.Link1, p8.game.Items, p8.game.Isdead, p8.game.Isfinish);
        return newState;
    }

    private bool DepthCheck(int depth, int maxdepth)
    {
        List<KeyValuePair<List<int>, GameState>> kvpairs = [];
        foreach (var kvp in _cache)
        {
            if (kvp.Key.Count == depth)
            {
                kvpairs.Add(kvp);
            }
        }
        ConcurrentDictionary<List<int>, GameState> states = [];
        Parallel.ForEach(kvpairs, kvp =>
        {
            foreach (int action in GetActions())
            {
                List<int> new_kvp = new(kvp.Key) { action };
                states.TryAdd(new_kvp, kvp.Value);
            }
        });
        _cache = new();
        Parallel.ForEach(states, state =>
        {
            var (newKey, newValue) = state;
            GameState curstate = Transition(newValue, newKey[^1]);
            if (Hcost(curstate, depth, maxdepth))
            {
                _cache.TryAdd(newKey, curstate);
            }
            if (IsGoal(curstate))
            {
                Solutions.Add(newKey);
                Console.WriteLine($"  inputs: {newKey}\n  frames: {newKey.Count - 1}");
            }
        });
        Console.WriteLine(_cache.Count);
        if (_cache.Count > 1000000)
        {
            var sorted = _cache
            .OrderBy(kvp =>
            {
                var point = kvp.Value.Wheel0;
                return F32.Abs(point.X - endflag.X) + F32.Abs(point.Y - endflag.Y);
            })
            .ToList();
            Console.WriteLine(F32.FromRaw(sorted[0].Value.Wheel0.X));
            Console.WriteLine(F32.FromRaw(sorted[0].Value.Wheel0.Y));
            Console.WriteLine(sorted[0].Value.Wheel0.IsFlying);
            var entries = _cache.Count;
            var keep = 1000000; //(int)Math.Ceiling(entries * 0.5);

            _cache = new();
            foreach (var entry in sorted.Take(keep))
            {
                _cache.TryAdd(entry.Key, entry.Value);
            }
        }

        return true;
    }

    public List<List<int>> Search(int max_depth, bool complete = false)
    {
        Solutions = [];
        DateTime timer = DateTime.Now;
        GameState state = InitState();
        _cache.TryAdd([0], state);
        Console.WriteLine("searching...");
        for (int i = 1; i <= max_depth; i++)
        {
            Console.WriteLine($"depth {i}...");
            //bool done = Iddfs(state, i, []) && !complete;
            bool done = DepthCheck(i, max_depth) && !complete;
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

        if (inputs is null)
        {
            return string.Empty;
        }

        return string.Join(", ", inputs.Select(i => action_dict.GetValueOrDefault(i, "unknown")));
    }

    private Color[] colors =
    [
        ColorTranslator.FromHtml("#000000"), // 00 black
        ColorTranslator.FromHtml("#1D2B53"), // 01 dark-blue
        ColorTranslator.FromHtml("#7E2553"), // 02 dark-purple
        ColorTranslator.FromHtml("#008751"), // 03 dark-green
        ColorTranslator.FromHtml("#AB5236"), // 04 brown
        ColorTranslator.FromHtml("#5F574F"), // 05 dark-grey
        ColorTranslator.FromHtml("#C2C3C7"), // 06 light-grey
        ColorTranslator.FromHtml("#FFF1E8"), // 07 white
        ColorTranslator.FromHtml("#FF004D"), // 08 red
        ColorTranslator.FromHtml("#FFA300"), // 09 orange
        ColorTranslator.FromHtml("#FFEC27"), // 10 yellow
        ColorTranslator.FromHtml("#00E436"), // 11 green
        ColorTranslator.FromHtml("#29ADFF"), // 12 blue
        ColorTranslator.FromHtml("#83769C"), // 13 lavender
        ColorTranslator.FromHtml("#FF77A8"), // 14 pink
        ColorTranslator.FromHtml("#FFCCAA"), // 15 light-peach
        
        /*
        ColorTranslator.FromHtml("#291814"), // 16 brownish-black
        ColorTranslator.FromHtml("#111D35"), // 17 darker-blue
        ColorTranslator.FromHtml("#422136"), // 18 darker-purple
        ColorTranslator.FromHtml("#125359"), // 19 blue-green
        ColorTranslator.FromHtml("#742F29"), // 20 dark-brown
        ColorTranslator.FromHtml("#49333B"), // 21 darker-grey
        ColorTranslator.FromHtml("#A28879"), // 22 medium-grey
        ColorTranslator.FromHtml("#F3EF7D"), // 23 light-yellow
        ColorTranslator.FromHtml("#BE1250"), // 24 dark-red
        ColorTranslator.FromHtml("#FF6C24"), // 25 dark-orange
        ColorTranslator.FromHtml("#A8E72E"), // 26 lime-green
        ColorTranslator.FromHtml("#00B543"), // 27 medium-green
        ColorTranslator.FromHtml("#065AB5"), // 28 true-blue
        ColorTranslator.FromHtml("#754665"), // 29 mauve
        ColorTranslator.FromHtml("#FF6E59"), // 30 dark-peach
        ColorTranslator.FromHtml("#FF9D81"), // 31 peach
        */
    ];

    private bool PathFromImage(EntityState wheel0, EntityState wheel1, int lvl, Color color, int extralayers = 0)
    {
        (F32 X, F32 Y) w0 = (F32.FromRaw(wheel0.X), F32.FromRaw(wheel0.Y));
        (F32 X, F32 Y) w1 = (F32.FromRaw(wheel1.X), F32.FromRaw(wheel1.Y));
        int iX = F32.FloorToInt(F32.Min(w0.X, w1.X) + (F32.Max(w0.X, w1.X) - F32.Min(w0.X, w1.X)) / 2);
        int iY = F32.FloorToInt(F32.Min(w0.Y, w1.Y) + (F32.Max(w0.Y, w1.Y) - F32.Min(w0.Y, w1.Y)) / 2);
        var iLevel = p8.game.Levels[lvl - 1];
        int startx = iLevel.Zones[0].Startx;
        int starty = iLevel.Zones[0].Starty;
        iX -= startx * 8;
        iY -= starty * 8;
        iY += extralayers * 8;

        if (iX < 0 || iX >= _pathImageWidth || iY < 0 || iY >= _pathImageHeight)
        {
            return false;
        }

        // Get color from the byte array (assuming 32bppArgb format)
        int index = iY * _pathImageStride + iX * 4; // 4 bytes per pixel (BGRA)
        byte b = _pathImageData[index];
        byte g = _pathImageData[index + 1];
        byte r = _pathImageData[index + 2];
        byte a = _pathImageData[index + 3];

        Color pixelColor = Color.FromArgb(a, r, g, b);
        Color targetColor = color;

        return pixelColor == targetColor;
    }

    public void CreateLevelImage(int lvl, int extralayers = 0)
    {
        int minx = int.MaxValue;
        int maxx = 0;
        int miny = int.MaxValue;
        int maxy = 0;
        foreach (var zone in p8.game.Levels[lvl - 1].Zones)
        {
            if (zone != null)
            {
                if (zone.Startx < minx) { minx = zone.Startx; }
                if (zone.Startx + zone.Sizex > maxx) { maxx = zone.Startx + zone.Sizex; }
                if (zone.Starty < miny) { miny = zone.Starty; }
                if (zone.Starty + zone.Sizey > maxy) { maxy = zone.Starty + zone.Sizey; }
            }
        }

        Bitmap new_image = new((maxx - minx) * 8, (maxy - miny + extralayers) * 8);
        for (int i = 0; i < maxx - minx; i++)
        {
            for (int j = 0; j < maxy - miny + extralayers; j++)
            {
                int snum = 0;
                if (j > extralayers - 1)
                {
                    snum = p8.Mget(F32.FromInt(minx + i), F32.FromInt(miny + j - extralayers));
                }
                for (int k = 0; k < 8; k++)
                {
                    for (int l = 0; l < 8; l++)
                    {
                        int sx = (snum * 8) % 128;
                        int sy = snum / 16;
                        var col = p8.Sget(F32.FromInt(sx) + k, F32.FromInt(sy * 8) + l);
                        new_image.SetPixel(i * 8 + k, j * 8 + l, colors[col]);
                    }
                }
            }
        }
        new_image.Save($"lvl{lvl}.bmp");
    }

    private Dictionary<(int, int), ((int, int), (int, int))> Checkpoints = new()
    {
        { (15, 78), ((145, 25), (154, 48)) },
        { (80, 110), ((122, 88), (139, 110)) },
        { (120, 160), ((207, 133), (207, 133)) },
    };

    private bool ArchiveOrCull(int depth, GameState state)
    {
        int curcheck = 0;
        var checks = Checkpoints.ToList();
        (int, int) checkpointcenter = (Math.Abs(checks[curcheck].Value.Item1.Item1 - checks[curcheck].Value.Item1.Item2), Math.Abs(checks[curcheck].Value.Item2.Item1 - checks[curcheck].Value.Item2.Item2));
        if (depth < checks[curcheck].Key.Item1)
        {
            return true;
        }
        else if (depth >= checks[curcheck].Key.Item1 && depth <= checks[curcheck].Key.Item2)
        {
            bool wall_contact = false;

            if (!(state.Wheel0.IsFlying && state.Wheel1.IsFlying))
            {
                wall_contact = true;
            }

            if (wall_contact)
            {
                if (F32.Abs(F32.FromRaw(state.Wheel0.Y) - F32.FromRaw(state.Wheel1.Y)) < 10)
                {
                    return true;
                }
            }
        }
        return false;
    }

}
