using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using F = FixPointCS.Fixed32;

namespace SearchAlgorithm;

public class MainAlgorithm
{
    private IStorageProvider _cache;
    private Cyclo8.ItemStruct endflag;
    private ThreadLocal<Pico8> p8;
    private List<ActionsStruct> Solutions;
    private Cyclo8.ItemStruct startflag;

    private byte[] _pathImageData;
    private int _pathImageStride;
    private PixelFormat _pathImagePixelFormat;
    private int _pathImageWidth;
    private int _pathImageHeight;

    public MainAlgorithm(int level)
    {
        ThreadPool.SetMinThreads(
        workerThreads: Environment.ProcessorCount,
        completionPortThreads: Environment.ProcessorCount
        );
        _cache = new(
            minKey: 0,
            maxKey: 10000000,
            concurrencyLevel: Environment.ProcessorCount);
        p8 = new ThreadLocal<Pico8>(() =>
        {
            var instance = new Pico8();
            instance._cart.StartLevel(level);
            return instance;
        }, trackAllValues: true);
        InitPathImage();
        Solutions = [];
    }

    private void InitPathImage()
    {
        using var pathImage = new Bitmap("Paths/lvl3route5.bmp");
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

    private GameState InitState()
    {
        startflag = p8.Value._cart.Items.Where(item => item.Type == 196608).FirstOrDefault();
        endflag = p8.Value._cart.Items.Where(item => item.Type == 262144).FirstOrDefault();
        return new GameState(p8.Value._cart.Wheel0, p8.Value._cart.Wheel1, p8.Value._cart.Link1, p8.Value._cart.Items, p8.Value._cart.Isdead, p8.Value._cart.Isfinish);
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
            var e = PathFromImage(state.Wheel0, state.Wheel1, 3, Color.FromArgb(0, 0, 255, 0), 2);
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

        var playerpercentage = F.Mul(F.DivPrecise(p8.Value._cart.Wheel0.Y - startflag.Y, lvlrange), 100);
        var depthpercentage = F.Mul(F.DivPrecise(F.Max(depth - 65, 0), maxdepth), 100);
        return playerpercentage + 10 >= depthpercentage;
    }

    private bool IsGoal(GameState state)
    {
        return state.IsFinish;
    }

    private int[] GetActions(GameState state)
    {
        return AllowableActions();
    }

    private GameState Transition(GameState state, int action)
    {
        p8.Value._cart.Wheel0 = state.Wheel0;
        p8.Value._cart.Wheel1 = state.Wheel1;
        p8.Value._cart.Link1 = state.Link;
        p8.Value._cart.Items = state.Items;
        p8.Value._cart.Isdead = state.IsDead;
        p8.Value._cart.Isfinish = state.IsFinish;
        p8.Value.SetBtnState(action);
        p8.Value.Step();
        return new GameState(p8.Value._cart.Wheel0, p8.Value._cart.Wheel1, p8.Value._cart.Link1, p8.Value._cart.Items, p8.Value._cart.Isdead, p8.Value._cart.Isfinish);
    }

    private bool DepthCheck(int depth, int maxdepth)
    {
        var snapshot = _cache.GetOrderedSnapshotAsList();

        Parallel.ForEach(snapshot, item =>
        {
            if (item.actions.Path.Length == depth)
            {
                foreach (int action in GetActions(item.state))
                {
                    ActionsStruct newActions = new(item.actions.Path.Append(action));
                    GameState newState = Transition(item.state, action);
                    
                    if (Hcost(newState, depth, maxdepth))
                    {
                        int newDist = F.Abs(newState.Wheel0.X - endflag.X) + F.Abs(newState.Wheel0.Y - endflag.Y);
                        _cache.Add(newDist, newActions, newState);
                    }

                    if (IsGoal(newState))
                    {
                        Solutions.Add(newActions);
                        Console.WriteLine($"  inputs: {InputsToEnglish(newActions.Path)}\n  frames: {newActions.Path.Length - 1}");
                    }
                }

                _cache.Remove(item.dist, item.actions);
            }
        });

        snapshot = _cache.GetOrderedSnapshotAsList(); // can fail, need to fix

        Console.WriteLine($"{snapshot.Count} states in cache");
        Console.WriteLine(snapshot[0].state.Wheel0.X >> 16);
        Console.WriteLine(snapshot[0].state.Wheel0.Y >> 16);
        Console.WriteLine(snapshot[0].state.Wheel0.Isflying);

        long keep = 3000000;

        if (_cache.Count() > keep)
        {
            _cache.CullTo(keep);
        }

        return true;
    }

    public List<ActionsStruct> Search(int max_depth, bool complete = false)
    {
        Solutions = [];
        DateTime timer = DateTime.Now;
        ActionsStruct key = new(ImmutableArray<int>.Empty);
        GameState state = InitState();
        _cache.Add(-1, new ActionsStruct(key.Path.Append(0)), state); 
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

    public string InputsToEnglish(ImmutableArray<int> inputs)
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

        if (inputs.Length <= 0)
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

    private bool PathFromImage(Cyclo8.EntityStruct wheel0, Cyclo8.EntityStruct wheel1, int lvl, Color color, int extralayers = 0)
    {
        int iX = F.FloorToInt(F.Min(wheel0.X, wheel1.X) + F.DivPrecise(F.Abs(wheel0.X - wheel1.X), 131072));
        int iY = F.FloorToInt(F.Min(wheel0.Y, wheel1.Y) + F.DivPrecise(F.Abs(wheel0.Y - wheel1.Y), 131072));
        var iLevel = p8.Value._cart.Levels[lvl - 1];
        int startx = iLevel.Zones[0].Startx >> 16;
        int starty = iLevel.Zones[0].Starty >> 16;
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
        foreach (var zone in p8.Value._cart.Levels[lvl - 1].Zones)
        {
            if (zone.Sizex + zone.Sizey > 0) // replaced null check
            {
                if (zone.Startx < minx) { minx = zone.Startx >> 16; }
                if (zone.Startx + zone.Sizex > maxx) { maxx = zone.Startx + zone.Sizex >> 16; }
                if (zone.Starty < miny) { miny = zone.Starty >> 16; }
                if (zone.Starty + zone.Sizey > maxy) { maxy = zone.Starty + zone.Sizey >> 16; }
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
                    snum = p8.Value.Mget((minx + i) << 16, (miny + j - extralayers) << 16) >> 16;
                }
                for (int k = 0; k < 8; k++)
                {
                    for (int l = 0; l < 8; l++)
                    {
                        int sx = (snum * 8) % 128;
                        int sy = snum / 16;
                        var col = p8.Value.Sget((sx + k) << 16, (sy * 8 + l) << 16) >> 16;
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
        (int, int) checkpointcenter = (F.Abs(checks[curcheck].Value.Item1.Item1 - checks[curcheck].Value.Item1.Item2), F.Abs(checks[curcheck].Value.Item2.Item1 - checks[curcheck].Value.Item2.Item2));
        if (depth < checks[curcheck].Key.Item1)
        {
            return true;
        }
        else if (depth >= checks[curcheck].Key.Item1 && depth <= checks[curcheck].Key.Item2)
        {
            bool wall_contact = false;
            if (!(state.Wheel0.Isflying && state.Wheel1.Isflying))
            {
                wall_contact = true;
            }
            if (wall_contact)
            {
                if (F.Abs(state.Wheel0.Y - state.Wheel1.Y) < 655360)
                {
                    return true;
                }
            }
        }
        return false;
    }

}
