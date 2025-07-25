using FixMath;
using F = FixPointCS.Fixed32;

namespace Searchlo8
{
    public class Cyclo8F32Raw
    {
        #region globals
        private static Pico8Graphics p8;

        private readonly int Base_frameadvback;
        private readonly int Base_frameadvfront;
        private readonly int Base_speedback;
        private readonly int Base_speedfront;
        private readonly int Base_speedlerp;
        private bool Bikefaceright;
        private int Bikeframe;
        private int Bodyrot;
        private int Camadvanx;
        private int Camoffx;
        private int Camoffy;
        private bool Chardown;
        private int Charx;
        private int Charx2;
        private int Chary;
        private int Chary2;
        private readonly List<int> Cloudss;
        private readonly List<int> Cloudsx;
        private readonly List<int> Cloudsy;
        private int Currentlevel;
        private bool Dbg_checkfound;
        private int Dbg_curcheckcount;
        private int Dbg_lastcheckidx;
        private int Flaganim;
        private int Goalcamx;
        private int Goalcamy;
        public bool Isdead;
        public bool Isfinish;
        private bool Isstarted;
        private readonly int Item_apple;
        private readonly int Item_checkpoint;
        private readonly int Item_finish;
        private readonly int Item_start;
        private readonly int Item_teleport;
        private int Itemnb;
        public List<ItemStruct> Items;
        public int Last_check_x;
        public int Last_check_y;
        private readonly int Levelnb;
        public readonly List<LevelClass> Levels;
        private readonly int Limit_col;
        private readonly int Limit_wheel;
        public LinkStruct Link1;
        private int[] Pal;
        private bool Restartafterfinish;
        private int Retries;
        private int Score;
        private readonly List<int> Sdflink;
        private readonly int Stepnb;
        private readonly int Str_air;
        private readonly int Str_bodyrot;
        private readonly int Str_gravity;
        private readonly int Str_link;
        private readonly int Str_reflect;
        private readonly int Str_wheel;
        private readonly int Str_wheel_size;
        private int Timer;
        private int Timerlasteleport;
        private int Timernextlevel;
        private readonly int Timernextlevel_dur;
        private int Totalleveldone;
        private int Totalretries;
        private int Totalscore;
        private int Totaltimer;
        public EntityStruct Wheel0;
        public EntityStruct Wheel1;
        #endregion

        public Cyclo8F32Raw(Pico8Graphics pico8)
        {
            p8 = pico8;

            Camoffx = 0;
            Camoffy = -4194304;
            Goalcamx = 0;
            Goalcamy = -4194304;
            Camadvanx = 0;

            Bikeframe = 0;

            Flaganim = 0;
            Score = 0;
            Retries = 0;
            Timer = 0;

            Totalscore = 0;
            Totalretries = 0;
            Totaltimer = 0;
            Totalleveldone = 0;

            Bikefaceright = true;
            Isdead = false;
            Isfinish = false;
            Restartafterfinish = false;
            Isstarted = false;

            Charx = 0;
            Chary = 0;
            Charx2 = 0;
            Chary2 = 0;
            Chardown = false;

            // position of the last checkpoint
            Last_check_x = 0;
            Last_check_y = 0;

            Dbg_curcheckcount = 65536;
            Dbg_checkfound = false;
            Dbg_lastcheckidx = 0;

            // physics settings :
            // nb of physics substeps
            Stepnb = 655360;
            // strengh of rebound
            Str_reflect = 72089;
            Str_gravity = 3932;
            Str_air = 64880;
            Str_wheel = 16384;
            Str_wheel_size = 65536;
            Str_link = 32768;
            // rotation of the bike
            // according to arrow keys
            Str_bodyrot = 2621;
            // acceleration factor
            Base_speedlerp = 32768;
            // max speed front
            Base_speedfront = 11796;
            // max speed back
            Base_speedback = 1966;
            Base_frameadvfront = 19660;
            Base_frameadvback = 9830;

            Limit_col = 131072;
            Limit_wheel = 98304;

            Bodyrot = 0;

            Currentlevel = 1;
            Levelnb = 7;

            Timernextlevel = 0;
            Timernextlevel_dur = 13762560;
            Timerlasteleport = 65536000;

            Cloudsx = new(new int[60]);
            Cloudsy = new(new int[60]);
            Cloudss = new(new int[60]);
            Levels = new(new LevelClass[7]);
            Pal = [];

            Itemnb = 0;
            Item_apple = 65536;
            Item_checkpoint = 131072;
            Item_start = 196608;
            Item_finish = 262144;
            Item_teleport = 327680;

            Items = new(new ItemStruct[30]);
            Link1 = new();

            // array to link sprite to colision
            Sdflink = new(new int[59])
            {
                [0] = 65536,
                [1] = 131072,
                [2] = 196608,
                [11] = 196608,
                [5] = 262144,
                [12] = 262144,
                [7] = 327680,
                [8] = 393216,
                [9] = 458752,
                [26] = 524288,
                [47] = 589824,
                [48] = 655360,
                [49] = 720896,
                [50] = 786432,
                [51] = 851968,
                [57] = 917504,
                [58] = 983040
            };
        }

        // map zone structure.
        // level is made of several zones
        public class ZoneClass(int inStartx, int inStarty, int inSizex, int inSizey)
        {
            public int Startx = inStartx;
            public int Starty = inStarty;
            public int Sizex = inSizex;
            public int Sizey = inSizey;
        }

        private static ZoneClass ZoneNew(int inStartX, int inStartY, int inSizeX, int inSizeY)
        {
            return new ZoneClass(inStartX, inStartY, inSizeX, inSizeY);
        }

        // level structure
        public class LevelClass(string inName, int inZkill, int inBacky, int inCamminx, int inCammaxx, int inCamminy, int inCammaxy)
        {
            public string Name = inName;
            public List<ZoneClass> Zones = new(new ZoneClass[2]);
            public int Zonenb = 0;
            public int Zkill = inZkill;
            public int Backy = inBacky;
            public int Camminx = inCamminx;
            public int Cammaxx = inCammaxx;
            public int Camminy = inCamminy;
            public int Cammaxy = inCammaxy;
            public bool Startright = true;
        }

        private static LevelClass LevelNew(string inName, int inZkill, int inBacky, int inCamminx, int inCammaxx, int inCamminy, int inCammaxy)
        {
            return new LevelClass(inName, inZkill, inBacky, inCamminx, inCammaxx, inCamminy, inCammaxy);
        }

        // entity = the 2 wheels

        public struct EntityStruct(int inx, int iny)
        {
            public int X = inx;
            public int Y = iny;
            public int Vx = 0;
            public int Vy = 0;
            public int Rot = 0;
            public int Vrot = 0;
            public bool Isflying = true;
            public int Linkside = 65536;
        }

        private static EntityStruct EntityNew(int inx, int iny)
        {
            return new EntityStruct(inx, iny);
        }

        public struct ItemStruct(int inx, int iny, int inType)
        {
            public int X = inx;
            public int Y = iny;
            public int Type = inType;
            public bool Active = true;
            public int Size = 524288;
        }

        private static ItemStruct ItemNew(int inX, int inY, int inType)
        {
            return new ItemStruct(inX, inY, inType);
        }

        // a physic link between wheels
        public struct LinkStruct()
        {
            public int Baselen = 524288;
            public int Length = 524288;
            public int Dirx = 0;
            public int Diry = 0;
        }

        private static int Lerp(int a, int b, int alpha)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"Lerp()" + Environment.NewLine);
            return F.Mul(a, 65536 - alpha) + F.Mul(b, alpha);
        }

        private static int Saturate(int a)
        {
            return F.Max(0, F.Min(65536, a));
        }

        private void CreateLevels()
        {
            Levels[0] = LevelNew("long road", 16777216, 9437184, 33554432, 58720256, -65536000, 8388608);
            Levels[0].Zones[0] = ZoneNew(4194304, 1048576, 4194304, 1048576);
            Levels[0].Zonenb = 1;

            Levels[1] = LevelNew("easy wheely", 8192000, 1048576, 0, 26214400, -65536000, 3801088);
            Levels[1].Zones[0] = ZoneNew(0, 0, 4194304, 1048576);
            Levels[1].Zones[1] = ZoneNew(2097152, 1048576, 2097152, 262144);
            Levels[1].Zonenb = 2;

            Levels[2] = LevelNew("central pit", 16777216, 9437184, 0, 8388608, -65536000, 8388608);
            Levels[2].Zones[0] = ZoneNew(0, 1048576, 2097152, 1048576);
            Levels[2].Zonenb = 1;

            Levels[3] = LevelNew("spiral", 8192000, 1048576, 54657024, 58720256, 131072, 131072);
            Levels[3].Zones[0] = ZoneNew(6815744, 0, 1572864, 1048576);
            Levels[3].Zonenb = 1;

            Levels[4] = LevelNew("sky fall", 8192000, 1048576, 33554432, 45875200, -65536000, 0);
            Levels[4].Zones[0] = ZoneNew(4194304, 0, 2621440, 1048576);
            Levels[4].Zonenb = 1;

            Levels[5] = LevelNew("here and there", 25296896, 17825792, 25165824, 58720256, -65536000, 16777216);
            Levels[5].Zones[0] = ZoneNew(3145728, 2097152, 4194304, 1048576);
            Levels[5].Zones[1] = ZoneNew(7340032, 2097152, 1048576, 524288);
            Levels[5].Zonenb = 2;
            Levels[5].Startright = false;

            Levels[6] = LevelNew("ninja rise", 25296896, 16777216, 0, 25231360, -65536000, 16777216);
            Levels[6].Zones[0] = ZoneNew(0, 2097152, 3145728, 1048576);
            Levels[6].Zones[1] = ZoneNew(2097152, 1310720, 2097152, 786432);
            Levels[6].Zonenb = 2;
        }

        public void Init()
        {
            // uncomment the next line
            // to regenerate sdf sprite
            // GenAllSdf();

            Pal = [327680,
                851968,
                983040,
                720896,
                589824,
                393216,
                458752,
                458752,
                917504,
                655360,
                458752,
                458752,
                458752,
                393216,
                983040,
                458752
            ];

            CreateEntities();
            CreateLevels();
        }

        private void CreateClouds()
        {
            for (int i = 0; i < 60; i++)
            {
                Cloudsx[i] = p8.Rnd(0, 655360);
                Cloudsy[i] = p8.Rnd(0, 327680);
                Cloudss[i] = p8.Rnd(0, 655360);
            }
        }

        private void StartLevel(int level)
        {
            Currentlevel = level;

            Items = new(new ItemStruct[30]);
            Itemnb = 0;
            Bikefaceright = Levels[Currentlevel - 1].Startright;

            FindReplaceItems();
            ResetCamera();
            ResetPlayer();
            Score = 0;
            Retries = 0;
            Timer = 0;
            Restartafterfinish = false;
        }

        private void FindReplaceItemsZone(int startx, int starty, int sizex, int sizey)
        {
            for (int i = startx; i < startx + sizex; i += 65536)
            {
                for (int j = starty; j < starty + sizey; j += 65536)
                {
                    int col = p8.Mget(i, j);
                    int flags = p8.Fget(col);
                    int itemtype = 0;

                    if (((flags >> 16) & 4) > 0)
                    {
                        itemtype = Item_teleport;
                        if (col == 3670016)
                        {
                            itemtype = Item_apple;
                        }
                    }
                    if (((flags >> 16) & 8) > 0)
                    {
                        itemtype = Item_checkpoint;
                        if (col == 4390912)
                        {
                            itemtype = Item_start;
                            Last_check_x = F.Mul(524288, i) + 262144;
                            Last_check_y = F.Mul(524288, j) + 262144;
                        }
                        if (col == 4456448)
                        {
                            itemtype = Item_finish;
                        }
                    }
                    //if we found an item
                    if (itemtype != 0)
                    {
                        Itemnb += 1;
                        Items[Itemnb - 1] = ItemNew(F.Mul(i, 524288) + 229376, F.Mul(j, 524288) + 229376, itemtype);

                        // remove from the map
                        p8.Mset(i, j, 0);
                    }
                }
            }
        }

        // find all items
        // insert in the array
        // remove them from the map
        private void FindReplaceItems()
        {
            // here is the list of zones
            // that make the level
            for (int i = 0; i < Levels[Currentlevel - 1].Zonenb; i++)
            {
                ZoneClass curzone = Levels[Currentlevel - 1].Zones[i];
                FindReplaceItemsZone(curzone.Startx, curzone.Starty, curzone.Sizex, curzone.Sizey);
            }
        }

        // display the level
        private void DrawMap(int flags)
        {
            // here is the list of zones
            // that make the level
            for (int i = 0; i < Levels[Currentlevel - 1].Zonenb; i++)
            {
                ZoneClass curzone = Levels[Currentlevel - 1].Zones[i];
                p8.Map(curzone.Startx, curzone.Starty, F.Mul(curzone.Startx, 524288), F.Mul(curzone.Starty, 524288), curzone.Sizex, curzone.Sizey, flags);
            }
        }

        // reset player state
        // after a retry
        private void ResetPlayer()
        {
            Isdead = true;
            Wheel0.X = Last_check_x;
            Wheel0.Y = Last_check_y;
            Wheel0.Vx = 0;
            Wheel0.Vy = 0;
            Wheel0.Vrot = 0;

            Wheel1.X = Last_check_x + 524288;
            Wheel1.Y = Last_check_y;
            Wheel1.Vx = 0;
            Wheel1.Vy = 0;
            Wheel1.Vrot = 0;

            Isdead = false;

            if (Isfinish)
            {
                Restartafterfinish = true;
            }
            if (!Isfinish)
            {
                Retries += 65536;
            }
        }

        private void ResetCamera()
        {
            Camoffx = Last_check_x - 1048576;
            Camoffy = Last_check_y - 4194304;
            Goalcamx = Camoffx;
            Goalcamy = Camoffy;
        }

        // create the 2 wheels
        // and init some variables
        private void CreateEntities()
        {
            Wheel0 = EntityNew(0, 0);
            Wheel1 = EntityNew(524288, 0);
            //Wheel0.Link = Link1;
            Wheel0.Linkside = 65536;
            //Wheel1.Link = Link1;
            Wheel1.Linkside = -65536;
        }

        // get the value of sdf
        // at location lx,ly
        // according to a sprite
        // chosen at an offset ox,oy
        private int GetSdf(int lx, int ly, int ox, int oy)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"GetSdf()" + Environment.NewLine);
            int sx = F.DivPrecise(lx + ox, 524288);
            int sy = F.DivPrecise(ly + oy, 524288);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"sx {F32.FromRaw(sx)} | lx {F32.FromRaw(lx)} | ox {F32.FromRaw(ox)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"sy {F32.FromRaw(sy)} | ly {F32.FromRaw(ly)} | oy {F32.FromRaw(oy)}" + Environment.NewLine);

            // get the sprite at the offset
            int col = p8.Mget(sx, sy);
            if (col != 0)
            {

            }
            int flags = p8.Fget(col);
            int isc = ((flags >> 16) & 1) << 16;
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"col {F32.FromRaw(col)} | lx {F32.FromRaw(lx)} | ox {F32.FromRaw(ox)} | ly {F32.FromRaw(ly)} | oy {F32.FromRaw(oy)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"flags {F32.FromRaw(flags)} | isc {F32.FromRaw(isc)}" + Environment.NewLine);

            // check if its a colision
            if (isc == 0)
            {
                return 0;
            }

            // check if its in the level zone
            bool inlevelzone = false;
            for (int i = 0; i < Levels[Currentlevel - 1].Zonenb; i++)
            {
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"for loop {i}" + Environment.NewLine);
                ZoneClass curzone = Levels[Currentlevel - 1].Zones[i];
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"sx {F32.FromRaw(sx)} | curzone.Startx {F32.FromRaw(curzone.Startx)} | curzone.Sizex {F32.FromRaw(curzone.Sizex)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"sy {F32.FromRaw(sy)} | curzone.Starty {F32.FromRaw(curzone.Starty)} | curzone.Sizey {F32.FromRaw(curzone.Sizey)}" + Environment.NewLine);
                if ((sx >= curzone.Startx) && (sx < (curzone.Startx + curzone.Sizex)))
                {
                    if ((sy >= curzone.Starty) && (sy < (curzone.Starty + curzone.Sizey)))
                    {
                        inlevelzone = true;
                        File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"inlevelzone {inlevelzone}" + Environment.NewLine);
                        break;
                    }
                }
            }

            if (!inlevelzone)
            {
                return 0;
            }

            // get the colision profile
            int sdfval = Sdflink[(col >> 16) - 1];
            // if none is found, use the full square
            //sdfval ??= 0;
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"sdfval {F32.FromRaw(sdfval)} | col {F32.FromRaw(col)}" + Environment.NewLine);

            // proper coordinates in sdf
            int wx = F.Mul(1048576, p8.Mod(sdfval, 524288)) + lx - F.Mul(sx, 524288) + 262144;
            int wy = F.Mul(1048576, F.DivPrecise(sdfval, 524288)) + 6553600 + ly - F.Mul(sy, 524288);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"wx {F32.FromRaw(wx)} | sdfval {F32.FromRaw(sdfval)} | lx {F32.FromRaw(lx)} | sx {F32.FromRaw(sx)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"wy {F32.FromRaw(wy)} | sdfval {F32.FromRaw(sdfval)} | ly {F32.FromRaw(ly)} | sy {F32.FromRaw(sy)}" + Environment.NewLine);
            // get distance
            int dist = p8.Sget(wx, wy);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"dist {F32.FromRaw(dist)} | wx {F32.FromRaw(wx)} | wy {F32.FromRaw(wy)}" + Environment.NewLine);

            return dist;
        }

        // get the combined sdf
        // of the 4 closest cells
        private int IsPointcol(int lx, int ly)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"IsPointCol()" + Environment.NewLine);
            int v0 = GetSdf(lx, ly, -196608, -196608);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v0 {F32.FromRaw(v0)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            int v1 = GetSdf(lx, ly, 262144, -196608);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v1 {F32.FromRaw(v1)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            int v2 = GetSdf(lx, ly, 262144, 262144);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v2 {F32.FromRaw(v2)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            int v3 = GetSdf(lx, ly, -196608, 262144);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v3 {F32.FromRaw(v3)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
                
            return F.Max(F.Max(v0, v1), F.Max(v2, v3));
        }

        // get the colision distance
        // and surface normal
        private (int final, int norx, int nory) IsColiding(int lx, int ly)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"IsColiding()" + Environment.NewLine);
            // we take the 4 points
            // at the center of the wheel
            int v0 = IsPointcol(lx - 32768, ly - 32768);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v0 {F32.FromRaw(v0)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            int v1 = IsPointcol(lx + 32768, ly - 32768);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v1 {F32.FromRaw(v1)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            int v2 = IsPointcol(lx + 32768, ly + 32768);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v2 {F32.FromRaw(v2)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            int v3 = IsPointcol(lx - 32768, ly + 32768);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"v3 {F32.FromRaw(v3)} | lx {F32.FromRaw(lx)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);

            // we iterpolate the distance
            // with bilinear
            int llx = lx - 32768 - F.Floor(lx - 32768);
            int lly = ly - 32768 - F.Floor(ly - 32768);
            int lerp1 = F.Mul(65536 - llx, v0) + F.Mul(llx, v1);
            int lerp2 = F.Mul(65536 - llx, v3) + F.Mul(llx, v2);
            int final = F.Mul(65536 - lly, lerp1) + F.Mul(lly, lerp2);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"llx {F32.FromRaw(llx)} | lx {F32.FromRaw(lx)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"lly {F32.FromRaw(lly)} | ly {F32.FromRaw(ly)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"lerp1 {F32.FromRaw(lerp1)} | llx {F32.FromRaw(llx)} | v0 {F32.FromRaw(v0)} | v1 {F32.FromRaw(v1)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"lerp2 {F32.FromRaw(lerp2)} | llx {F32.FromRaw(llx)} | v3 {F32.FromRaw(v3)} | v2 {F32.FromRaw(v2)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"final {F32.FromRaw(final)} | lly {F32.FromRaw(lly)} | lerp1 {F32.FromRaw(lerp1)} | lerp2 {F32.FromRaw(lerp2)}" + Environment.NewLine);

            // the normal is a gradient
            int norx = F.Mul(v0 - v1 + v3 - v2, 32768);
            int nory = F.Mul(v0 - v3 + v1 - v2, 32768);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"norx {F32.FromRaw(norx)} | v0 {F32.FromRaw(v0)} | v1 {F32.FromRaw(v1)} | v3 {F32.FromRaw(v3)} | v2 {F32.FromRaw(v2)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"nory {F32.FromRaw(nory)} | v0 {F32.FromRaw(v0)} | v3 {F32.FromRaw(v3)} | v1 {F32.FromRaw(v1)} | v2 {F32.FromRaw(v2)}" + Environment.NewLine);

            // we ensure normal is normalized
            int len = F.SqrtPrecise(F.Mul(norx, norx) + F.Mul(nory, nory) + 65);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"len {F32.FromRaw(len)} | norx {F32.FromRaw(norx)} | nory {F32.FromRaw(nory)}" + Environment.NewLine);
            norx = F.DivPrecise(norx, len);
            nory = F.DivPrecise(nory, len);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"norx {F32.FromRaw(norx)} | nory {F32.FromRaw(nory)}" + Environment.NewLine);

            return (final, norx, nory);
        }

        // this take a velocity vector
        // and reflect it by a normal
        // a damping is applyed of the reflection
        private (int, int) Reflect(int vx, int vy, int nx, int ny)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"Reflect()" + Environment.NewLine);
            int dot = F.Mul(vx, nx) + F.Mul(vy, ny);
            int bx = F.Mul(dot, nx);
            int by = F.Mul(dot, ny);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"dot {F32.FromRaw(dot)} | vx {F32.FromRaw(vx)} | nx {F32.FromRaw(nx)} | vy {F32.FromRaw(vy)} | ny {F32.FromRaw(ny)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"bx {F32.FromRaw(bx)} | dot {F32.FromRaw(dot)} | nx {F32.FromRaw(nx)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"by {F32.FromRaw(by)} | dot {F32.FromRaw(dot)} | nx {F32.FromRaw(ny)}" + Environment.NewLine);

            int rx = vx - F.Mul(Str_reflect, bx);
            int ry = vy - F.Mul(Str_reflect, by);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"rx {F32.FromRaw(rx)} | vx {F32.FromRaw(vx)} | Str_reflect {F32.FromRaw(Str_reflect)} | bx {F32.FromRaw(bx)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ry {F32.FromRaw(ry)} | vy {F32.FromRaw(vy)} | Str_reflect {F32.FromRaw(Str_reflect)} | by {F32.FromRaw(by)}" + Environment.NewLine);

            // we play some colision sounds
            // when both vector are opposite
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"dot {F32.FromRaw(dot)}" + Environment.NewLine);
            if (dot < -52428)
            {
                p8.Sfx(0, 3);
            }
            else
            {
                if (dot < -13107)
                {
                    p8.Sfx(6, 3);
                }
            }

            return (rx, ry);
        }

        // this update the state of a link
        // between 2 wheels
        private void UpLink(ref LinkStruct link)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"UpLink()" + Environment.NewLine);
            int dirx = Wheel1.X - Wheel0.X;
            int diry = Wheel1.Y - Wheel0.Y;
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"dirx {F32.FromRaw(dirx)} | Wheel1.x {F32.FromRaw(Wheel1.X)} | Wheel0.x {F32.FromRaw(Wheel0.X)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"diry {F32.FromRaw(diry)} | Wheel1.y {F32.FromRaw(Wheel1.Y)} | Wheel0.y {F32.FromRaw(Wheel0.Y)}" + Environment.NewLine);

            link.Length = F.SqrtPrecise(F.Mul(dirx, dirx) + F.Mul(diry, diry) + 655);
            link.Dirx = F.DivPrecise(dirx, link.Length);
            link.Diry = F.DivPrecise(diry, link.Length);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"link.Length {F32.FromRaw(link.Length)} | link.Dirx {F32.FromRaw(link.Dirx)} | link.Diry {F32.FromRaw(link.Diry)}" + Environment.NewLine);
        }

        // pre physic update of a wheel
        private void UpStartEntity(ref EntityStruct ent)
        {
            // apply gravity
            ent.Vy += Str_gravity;
            ent.Isflying = true;
        }

        // do one step of physic on a wheel
        private void UpStepEntity(ref EntityStruct ent)
        {
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"UpStepEntity()" + Environment.NewLine);
            // apply link force
            //if (Link1 is not null)
            //{
            // force according to base length
            int flink = F.Mul(Link1.Length - Link1.Baselen, Str_link);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"flink {F32.FromRaw(flink)} | Link1.Length {F32.FromRaw(Link1.Length)} | Link1.Baselen {F32.FromRaw(Link1.Baselen)} | Str_link {F32.FromRaw(Str_link)}" + Environment.NewLine);

            // add the force
            ent.Vx += F.Mul(F.Mul(Link1.Dirx, ent.Linkside), flink);
            ent.Vy += F.Mul(F.Mul(Link1.Diry, ent.Linkside), flink);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vx {F32.FromRaw(ent.Vx)} | Link1.Dirx {F32.FromRaw(Link1.Dirx)} | ent.Linkside {F32.FromRaw(ent.Linkside)} | flink {F32.FromRaw(flink)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vy {F32.FromRaw(ent.Vy)} | Link1.Diry {F32.FromRaw(Link1.Diry)} | ent.Linkside {F32.FromRaw(ent.Linkside)} | flink {F32.FromRaw(flink)}" + Environment.NewLine);

            // apply the rotation
            // due to the body
            // if not on the ground ?
            // if(ent.isflying) then
            if (true)
            {
                // force perpendicular
                // to the link axis
                int perpx = Link1.Diry;
                int perpy = -Link1.Dirx;
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"perpx {F32.FromRaw(perpx)} | Link1.Diry {F32.FromRaw(Link1.Diry)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"perpy {F32.FromRaw(perpy)} | -Link1.Dirx {F32.FromRaw(-Link1.Dirx)}" + Environment.NewLine);

                ent.Vx += F.Mul(F.DivPrecise(F.Mul(perpx, Bodyrot), Stepnb), ent.Linkside);
                ent.Vy += F.Mul(F.DivPrecise(F.Mul(perpy, Bodyrot), Stepnb), ent.Linkside);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vx {F32.FromRaw(ent.Vx)} | perpx {F32.FromRaw(perpx)} | Bodyrot {F32.FromRaw(Bodyrot)} | Stepnb {F32.FromRaw(Stepnb)} | ent.Linkside {F32.FromRaw(ent.Linkside)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vy {F32.FromRaw(ent.Vy)} | perpy {F32.FromRaw(perpy)} | Bodyrot {F32.FromRaw(Bodyrot)} | Stepnb {F32.FromRaw(Stepnb)} | ent.Linkside {F32.FromRaw(ent.Linkside)}" + Environment.NewLine);
            }
            //}

            // we test if the new location
            // is coliding
            int x2 = ent.X + F.DivPrecise(ent.Vx, Stepnb);
            int y2 = ent.Y + F.DivPrecise(ent.Vy, Stepnb);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"x2 {F32.FromRaw(x2)} | ent.X {F32.FromRaw(ent.X)} | ent.Vx {F32.FromRaw(ent.Vx)} | Stepnb {F32.FromRaw(Stepnb)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"y2 {F32.FromRaw(y2)} | ent.Y {F32.FromRaw(ent.Y)} | ent.Vy {F32.FromRaw(ent.Vy)} | Stepnb {F32.FromRaw(Stepnb)}" + Environment.NewLine);

            //if (ent.Y > F32.FromDouble(16.1)) //F32.FromDouble(16.9132))
            //{
            //
            //}

            (int iscol, int norx, int nory) = IsColiding(x2, y2); // should all be 0
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"x2 {F32.FromRaw(x2)} | y2 {F32.FromRaw(y2)} | iscol {F32.FromRaw(iscol)} | norx {F32.FromRaw(norx)} | nory {F32.FromRaw(nory)}" + Environment.NewLine);

            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"iscol {F32.FromRaw(iscol)} | Limit_col {F32.FromRaw(Limit_col)}" + Environment.NewLine);
            // if coliding
            if (iscol > Limit_col)
            {
                //Console.WriteLine("collision");
                // debug data
                // ent.Lastcolx = ent.X
                // ent.Lastcoly = ent.Y
                // ent.Lastcolnx = Norx
                // ent.Lastcolny = Nory

                // reflect the velocity by
                // the surface normal

                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vx {F32.FromRaw(ent.Vx)} | ent.Vy {F32.FromRaw(ent.Vy)}" + Environment.NewLine);
                (ent.Vx, ent.Vy) = Reflect(ent.Vx, ent.Vy, norx, nory);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vx {F32.FromRaw(ent.Vx)} | ent.Vy {F32.FromRaw(ent.Vy)} | norx {F32.FromRaw(norx)} | nory {F32.FromRaw(nory)}" + Environment.NewLine);
                //Console.WriteLine($"iscol {iscol}");
                //Console.WriteLine($"Limit_col {Limit_col}");
                //Console.WriteLine($"ent.Vx {ent.Vx}");
                //Console.WriteLine($"ent.Vy {ent.Vy}");

                // ensure we are not inside the colision
                ent.X += F.Mul(norx, iscol - Limit_col);
                ent.Y += F.Mul(nory, iscol - Limit_col);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.X {F32.FromRaw(ent.X)} | norx {F32.FromRaw(norx)} | iscol {F32.FromRaw(iscol)} | Limit_col {F32.FromRaw(Limit_col)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Y {F32.FromRaw(ent.Y)} | nory {F32.FromRaw(nory)} | iscol {F32.FromRaw(iscol)} | Limit_col {F32.FromRaw(Limit_col)}" + Environment.NewLine);
            }

            //if (ent.Vx < -1)
            //{
            //
            //}

            // apply the motion
            ent.X += F.DivPrecise(ent.Vx, Stepnb);
            ent.Y += F.DivPrecise(ent.Vy, Stepnb);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.X {F32.FromRaw(ent.X)} | ent.Vx {F32.FromRaw(ent.Vx)} | Stepnb {F32.FromRaw(Stepnb)}" + Environment.NewLine);
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Y {F32.FromRaw(ent.Y)} | ent.Vy {F32.FromRaw(ent.Vy)} | Stepnb {F32.FromRaw(Stepnb)}" + Environment.NewLine);
            //Console.WriteLine(ent.Y);

            // if wheel is near the ground
            // we apply the wheel force
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"iscol {F32.FromRaw(iscol)} | Limit_wheel {F32.FromRaw(Limit_wheel)}" + Environment.NewLine);
            if (iscol > Limit_wheel)
            {
                // force direction
                // perpendicular to the
                // surface normal
                int perpx = nory;
                int perpy = -norx;
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"perpx {F32.FromRaw(perpx)} | nory {F32.FromRaw(nory)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"perpy {F32.FromRaw(perpy)} | -norx {F32.FromRaw(-norx)}" + Environment.NewLine);

                int angfac = F.Mul(F.Mul(205881, 524288), Str_wheel_size); // F32.FromDouble(3.1415) * 8 * Str_wheel_size;
                // transform wheel speed to force
                int angrot = F.Mul(ent.Vrot, angfac);
                int wantx = F.Mul(angrot, perpx);
                int wanty = F.Mul(angrot, perpy);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"angfac {F32.FromRaw(angfac)} | Str_wheel_size {F32.FromRaw(Str_wheel_size)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"angrot {F32.FromRaw(angrot)} | ent.Vrot {F32.FromRaw(ent.Vrot)} | angfac {F32.FromRaw(angfac)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"wantx {F32.FromRaw(wantx)} | angrot {F32.FromRaw(angrot)} | perpx {F32.FromRaw(perpx)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"wanty {F32.FromRaw(wanty)} | angrot {F32.FromRaw(angrot)} | perpy {F32.FromRaw(perpy)}" + Environment.NewLine);

                //int distfactor = 65536; // F32.FromDouble(1.0);  // Saturate((iscol - Limit_wheel)*1.0)
                //File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"distfactor {distfactor}" + Environment.NewLine);

                // interpolate between
                // wheel motion
                // and entity motion
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vx {F32.FromRaw(ent.Vx)} | wantx {F32.FromRaw(wantx)} | Str_wheel {F32.FromRaw(Str_wheel)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vy {F32.FromRaw(ent.Vy)} | wanty {F32.FromRaw(wanty)} | Str_wheel {F32.FromRaw(Str_wheel)}" + Environment.NewLine);
                int lerpx = Lerp(ent.Vx, wantx, Str_wheel); // * distfactor);
                int lerpy = Lerp(ent.Vy, wanty, Str_wheel); // * distfactor);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"lerpx {F32.FromRaw(lerpx)} | ent.Vx {F32.FromRaw(ent.Vx)} | wantx {F32.FromRaw(wantx)} | Str_wheel {F32.FromRaw(Str_wheel)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"lerpy {F32.FromRaw(lerpy)} | ent.Vy {F32.FromRaw(ent.Vy)} | wanty {F32.FromRaw(wanty)} | Str_wheel {F32.FromRaw(Str_wheel)}" + Environment.NewLine);

                ent.Vx = lerpx;
                ent.Vy = lerpy;
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vx {F32.FromRaw(ent.Vx)} | lerpx {F32.FromRaw(lerpx)}" + Environment.NewLine);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vy {F32.FromRaw(ent.Vy)} | lerpy {F32.FromRaw(lerpy)}" + Environment.NewLine);

                // get the wheel speed along the surface
                int dotperp = F.Mul(ent.Vx, perpx) + F.Mul(ent.Vy, perpy);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"dotperp {F32.FromRaw(dotperp)} | ent.Vx {F32.FromRaw(ent.Vx)} | perpx {F32.FromRaw(perpx)} | ent.Vy {F32.FromRaw(ent.Vy)} | perpy {F32.FromRaw(perpy)}" + Environment.NewLine);

                // the new wheel rotation is
                // the speed along the surface
                ent.Vrot = F.DivPrecise(dotperp, angfac);
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Vrot {F32.FromRaw(ent.Vrot)} | dotperp {F32.FromRaw(dotperp)} | angfac {F32.FromRaw(angfac)}" + Environment.NewLine);

                // the wheel touch the ground
                ent.Isflying = false;
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"ent.Isflying {ent.Isflying}" + Environment.NewLine);
            }
        }

        // post physic update of a wheel
        private void UpEndEntity(ref EntityStruct ent)
        {
            // apply air friction
            if (!ent.Isflying)
            {
                ent.Vx = F.Mul(ent.Vx, Str_air);
                ent.Vy = F.Mul(ent.Vy, Str_air);
            }

            // make the wheel turn
            ent.Rot += ent.Vrot;
            // we could apply a wheel friction
            // ent.Vrot *= 0.94
        }

        // check if an item
        // is near the player
        private void CheckItem(ref ItemStruct it)
        {
            // need to be carefull
            // with squaring because of overflow
            // so we first divide by the item size
            // before squaring
            int madx = F.DivPrecise(it.X - Charx, it.Size);
            int mady = F.DivPrecise(it.Y - Chary, it.Size);
            int sqrlen = F.Mul(madx, madx) + F.Mul(mady, mady);

            // if colision with an item
            if ((!Isdead) && (sqrlen < 65536))
            {
                // apples
                if ((it.Type == Item_apple) && it.Active)
                {
                    if (!Restartafterfinish)
                    {
                        it.Active = false;
                        Score += 65536;
                        if (Isfinish)
                        {
                            Totalscore += 65536; // special case
                        }
                        p8.Sfx(3, 3);
                    }
                }
                // teleports
                if ((it.Type == Item_teleport) && it.Active)
                {
                    if ((!Isfinish) && (!Isdead))
                    {
                        p8.Sfx(7, 2);
                        Retries -= 65536; // free retry
                        Timerlasteleport = 0;
                        ResetPlayer();
                    }
                }
                // checkpoints
                if ((it.Type == Item_checkpoint) || (it.Type == Item_finish))
                {
                    if (it.Active)
                    {
                        it.Active = false;
                        Last_check_x = it.X;
                        Last_check_y = it.Y;

                        if (it.Type == Item_finish)
                        {
                            Isfinish = true;
                            // cumul total values
                            Totalscore += Score;
                            Totaltimer += Timer;
                            Totalretries += Retries;
                            Totalleveldone += 65536;
                            p8.Sfx(5, 2);
                        }
                        else
                        {
                            p8.Sfx(7, 2);
                        }
                    }
                }
            }
        }

        // debug function
        // find the next checkpoint in the list of item
        private void FindNextCheckpoint()
        {
            Dbg_curcheckcount = 65536;
            Dbg_checkfound = false;
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                LoopNextCheckpoint(ref item);
                Items[i] = item;
            }
            if (Dbg_checkfound)
            {
                Dbg_lastcheckidx += 65536;
                Retries -= 65536;
                ResetPlayer();
            }
            else
            {
                Dbg_lastcheckidx = 0;
            }
        }

        private void LoopNextCheckpoint(ref ItemStruct it)
        {
            if (it.Type == Item_checkpoint)
            {
                if (Dbg_curcheckcount == Dbg_lastcheckidx + 65536)
                {
                    it.Active = true;
                    Last_check_x = it.X;
                    Last_check_y = it.Y;
                    Dbg_checkfound = true;
                }
                Dbg_curcheckcount += 65536;
            }
        }

        // [CHANGE]
        public void LoadLevel(int level)
        {
            Currentlevel = level;
            Isstarted = true;
            StartLevel(Currentlevel);
        }

        // main update function
        public void Update()
        {
            // start menu
            if (!Isstarted)
            {
                // start the game
                if (p8.Btnp(4))
                {
                    LoadLevel(Currentlevel);
                }
                // change current level
                if (p8.Btnp(0) || p8.Btnp(3))
                {
                    Currentlevel -= 1;
                    if (Currentlevel <= 0)
                    {
                        Currentlevel = Levelnb;
                    }
                    p8.Sfx(0, 3);
                }
                if (p8.Btnp(1) || p8.Btnp(2))
                {
                    Currentlevel += 1;
                    if (Currentlevel > Levelnb)
                    {
                        Currentlevel = 1;
                    }
                    p8.Sfx(0, 3);
                }
                // debug
                // Isstarted = true;
                return;
            }

            // handle going to the next level
            if (Isfinish)
            {
                if (Timernextlevel > Timernextlevel_dur)
                {
                    if (Currentlevel != Levelnb)
                    {
                        Isfinish = false;
                        StartLevel(Currentlevel + 1);
                        Timernextlevel = 0;
                    }
                }
                Timernextlevel += 65536;
            }
            Bodyrot = 0;

            // player control
            if ((!Isdead) && (!Isfinish))
            {
                // flip button (c)
                if (p8.Btnp(4))
                {
                    Bikefaceright = !Bikefaceright;
                    p8.Sfx(8, 3);
                }
                EntityStruct controlwheel = Wheel0;
                EntityStruct otherwheel = Wheel1;
                int wheelside = 65536;
                                       // invert all values if bike face left
                if (!Bikefaceright)
                {
                    (controlwheel, otherwheel) = (otherwheel, controlwheel);
                    wheelside = -65536;
                }
                // button left
                if (p8.Btn(0))
                {
                    // make the body rotate
                    Bodyrot -= Str_bodyrot;
                }
                // button right
                if (p8.Btn(1))
                {
                    // make the body rotate
                    Bodyrot += Str_bodyrot;
                }
                // button up
                if (p8.Btn(2))
                {
                    // only the back wheel is set in motion
                    Wheel0.Vrot = Lerp(controlwheel.Vrot, F.Mul(-Base_speedfront, wheelside), Base_speedlerp);
                    Bikeframe -= Base_frameadvfront;
                }
                // button down
                if (p8.Btn(3))
                {
                    // both wheels are slowed
                    Wheel0.Vrot = Lerp(controlwheel.Vrot, F.Mul(Base_speedback, wheelside), Base_speedlerp);
                    Wheel0.Vrot = Lerp(otherwheel.Vrot, F.Mul(Base_speedback, wheelside), Base_speedlerp);
                    Bikeframe += Base_frameadvback;
                }
            }

            // update the physics
            // using several substep
            // to improve colision
            //foreach (EntityStruct i in Entities)
            //{
            UpStartEntity(ref Wheel0);
            UpStartEntity(ref Wheel1);
            //}
            //Console.WriteLine($"Timer {Timer}");
            File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"Timer {F32.FromRaw(Timer)}" + Environment.NewLine);
            for (int i = 0; i < Stepnb; i += 65536)
            {
                //Console.WriteLine($"physics loop {i}");
                File.AppendAllText(@"c:\Users\me\Desktop\output.txt", $"physics loop {F32.FromRaw(i)}" + Environment.NewLine);
                // update links
                UpLink(ref Link1);
                // update wheels
                //foreach (EntityStruct j in Entities)
                //{
                UpStepEntity(ref Wheel0);
                UpStepEntity(ref Wheel1);
                //}
            }
            //foreach (EntityStruct i in Entities)
            //{
            UpEndEntity(ref Wheel0);
            UpEndEntity(ref Wheel1);
            //}

            bool isdown = false;

            // compute the body location
            // according to the 2 wheels
            // this is the upper body
            (Charx, Chary, Chardown) = GetBikeRot(ref Wheel0, ref Wheel1, 262144);
                                                                           // this is the lower body
            (Charx2, Chary2, isdown) = GetBikeRot(ref Wheel0, ref Wheel1, 65536);

            // make upper body a bit closer
            // to the lower body
            Charx += F.Mul(Charx2 - Charx, 32768);

            // check the upper body colision
            (int coldist, int colnx, int colny) = IsColiding(Charx, Chary);
            if (coldist > 117964)
            {
                // if there is a colision
                // the player is dead
                if (!Isdead)
                {
                    Isdead = true;
                    if (!Isfinish)
                    {
                        p8.Sfx(4, 2);
                    }
                }
            }

            // check items colision
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item.Active) // replaced null check
                {
                    CheckItem(ref item);
                    Items[i] = item;
                }
            }

            bool needkillplayer = false;
            // check the killing floor
            if (Wheel0.Y > Levels[Currentlevel - 1].Zkill)
            {
                needkillplayer = true;
            }
            // check the killing camera limit
            if (Wheel0.X > Levels[Currentlevel - 1].Cammaxx + 8388608)
            {
                needkillplayer = true;
            }
            if (Wheel0.X < Levels[Currentlevel - 1].Camminx)
            {
                needkillplayer = true;
            }
            if (Wheel0.Y > Levels[Currentlevel - 1].Cammaxy + 8388608)
            {
                needkillplayer = true;
            }
            if (Wheel0.Y < Levels[Currentlevel - 1].Camminy)
            {
                needkillplayer = true;
            }

            if (needkillplayer)
            {
                if ((!Isfinish) && (!Isdead))
                {
                    p8.Sfx(4, 2);
                }
                ResetPlayer();
            }

            // check the retry button (v)
            if (p8.Btnp(5) && (!Isfinish))
            {
                p8.Sfx(7, 2);
                ResetPlayer();
            }

            // debug cheating :
            if (false)
            {
                if (p8.Btnp(5, 1))
                {
                    FindNextCheckpoint();
                }
            }

            // update the camera :

            // make the camer look back
            // if(entities[playeridx].vx<-0.8) then
            if (!Bikefaceright)
            {
                Camadvanx = -2097152;
            }
            // make the camer look front
            // if(entities[playeridx].vx>0.8) then
            if (Bikefaceright)
            {
                Camadvanx = 2097152;
            }

            // update the camera goal
            Goalcamx = F.Mul(Goalcamx, 58982) + F.Mul(Wheel0.X - 4194304 + Camadvanx, 6553);

            // in y there is a safe zone
            if (Camoffy > Wheel0.Y - 6291456)
            {
                Goalcamy = Wheel0.Y - 6291456;
            }
            if (Camoffy < Wheel0.Y - 2097152)
            {
                Goalcamy = Wheel0.Y - 2097152;
            }
            // or else the camera is only updated
            // when the wheel touch ground
            if (!Wheel0.Isflying)
            {
                Goalcamy = Wheel0.Y - 4194304;
            }

            // clamp the camera goal to the level limit
            Goalcamx = F.Max(Goalcamx, Levels[Currentlevel - 1].Camminx);
            Goalcamx = F.Min(Goalcamx, Levels[Currentlevel - 1].Cammaxx);

            Goalcamy = F.Max(Goalcamy, Levels[Currentlevel - 1].Camminy);
            Goalcamy = F.Min(Goalcamy, Levels[Currentlevel - 1].Cammaxy);

            // the camera location is lerped
            Camoffx = Lerp(Camoffx, Goalcamx, 13107);
            Camoffy = Lerp(Camoffy, Goalcamy, 19660);

            // increment the timer
            if (!Isfinish)
            {
                Timer += 65536;
            }
        }

        // draw a wheel entity
        private static void DrawEntity(ref EntityStruct ent)
        {
            int @base = 5242880;
            // the wheel sprite
            // depend on the wheel rotation
            int rfr = p8.Mod(F.Floor(F.Mul(F.Mul(-ent.Rot, 262144), 327680)), 327680);
            if (rfr < 0)
            {
                rfr += 327680;
            }
            int cspr = @base + rfr;

            // if (Math.Abs(ent.Vrot) > 0.14)
            if (false)
            {
                rfr = p8.Mod(F.Floor(F.Mul(-ent.Rot, 196608)), 196608);
                if (rfr < 0)
                {
                    rfr += 196608;
                }
                cspr = @base + 327680 + rfr;
            }

            // to avoid the wheel appearing
            // to rotate backward
            // if the speed is too strong
            // we rotate slower but skip
            // a frame each time
            if (F.Abs(ent.Vrot) > 9175)
            {
                rfr = p8.Mod(F.Floor(F.Mul(-ent.Rot, 196608)), 327680);
                if (rfr < 0)
                {
                    rfr += 327680;
                }
                rfr = F.Mul(rfr, 131072);
                if (rfr > 327680)
                {
                    rfr -= 327680;
                }
                cspr = @base + rfr;
            }
            p8.Spr(cspr, ent.X - 229376, ent.Y - 229376, 65536, 65536);
        }

        // take 2 wheel and give
        // a point between
        // with an perpendicular offset
        private static (int, int, bool) GetBikeRot(ref EntityStruct ent1, ref EntityStruct ent2, int offset)
        {
            int dirx = ent2.X - ent1.X;
            int diry = ent2.Y - ent1.Y;

            // average to get the center
            int centx = ent1.X + F.Mul(dirx, 32768);
            int centy = ent1.Y + F.Mul(diry, 32768);

            // normalize the direction
            int length = F.SqrtPrecise(F.Mul(dirx, dirx) + F.Mul(diry, diry) + 655);
            dirx = F.DivPrecise(dirx, length);
            diry = F.DivPrecise(diry, length);

            // get the perpendicular
            int perpx = diry;
            int perpy = -dirx;

            // offset the point
            // along the perpendicular
            centx += F.Mul(perpx, offset);
            centy += F.Mul(perpy, offset);

            // we want to know
            // is the point is below the bike
            bool isdown = false;
            if (perpy > 32768)
            {
                isdown = true;
            }
            return (centx, centy, isdown);
        }

        private static void CenterText(int posx, int posy, string text, int col)
        {
            int sposx = posx - F.Mul(text.Length << 16, 131072);
            int sposy = posy;
            p8.Print(text, sposx + 65536, sposy, 0);
            p8.Print(text, sposx - 65536, sposy, 0);
            p8.Print(text, sposx, sposy + 65536, 0);
            p8.Print(text, sposx, sposy - 65536, 0);
            p8.Print(text, sposx, sposy, col);
        }

        // draw an item icon (apple, checkpoint)
        private void DrawItem(ref ItemStruct it)
        {
            // only apples can be picked
            bool hide = false;
            if ((it.Type == Item_apple) && (!it.Active))
            {
                hide = true;
            }

            if (it.Type == Item_start)
            {
                hide = true;
            }

            if (!hide)
            {
                int sprite = 3670016;

                if (it.Type == Item_teleport)
                {
                    sprite = 6750208 + p8.Mod(Flaganim, 196608);
                }

                if ((it.Type == Item_checkpoint) || (it.Type == Item_finish))
                {
                    sprite = 4194304 + p8.Mod(Flaganim, 196608);

                    // change the flag pole color
                    int flagcolor = 786432;
                    if (it.Active)
                    {
                        if (it.Type == Item_finish)
                        {
                            flagcolor = 720896;
                        }
                        else
                        {
                            flagcolor = 524288;
                        }
                    }
                    p8.Line(it.X - 65536, it.Y - 196608, it.X - 65536, it.Y + 786432, flagcolor);
                }

                p8.Spr(sprite, it.X - 229376, it.Y - 229376, 65536, 65536);
            }
        }

        // draw the introduction and victory big flag
        private void DrawBigFlag(string text, int finishx, int finishy, int col)
        {
            p8.Rectfill(finishx, finishy, finishx + 4128768, finishy + 983040, 0);
            for (int i = 0; i < 2097152; i += 65536)
            {
                int tmpx = finishx + F.Mul(p8.Mod(i, 1048576), 262144);
                int tmpy = finishy + F.Mul(65536 - p8.Mod(i, 131072), 262144) + F.Mul(F.Floor(F.DivPrecise(i, 1048576)), 524288);
                p8.Rectfill(tmpx, tmpy, tmpx + 196608, tmpy + 196608, 393216);
            }
            CenterText(finishx + 2097152, finishy + 393216, text, col);

            int sprite = 4194304 + p8.Mod(F.Floor(F.Mul(Flaganim, 45875)), 196608);

            p8.Sspr(F.Mul(sprite - 4194304, 524288), F.Mul(524288, 524288), 524288, 524288, finishx - 1310720, finishy - 524288, 2097152, 2097152, true);
            p8.Sspr(F.Mul(sprite - 4194304, 524288), F.Mul(524288, 524288), 524288, 524288, finishx - 1048576 + 4194304, finishy - 524288, 2097152, 2097152, false);
        }

        private static string GetTimeStr(int val)
        {
            // transform timer to min:sec:dec
            int t_cent = p8.Mod(F.Floor(F.DivPrecise(F.Mul(val, 655360), 1966080)), 655360);
            int t_sec = p8.Mod(F.Floor(F.DivPrecise(val, 1966080)), 3932160);
            int t_min = F.Floor(F.DivPrecise(val, F.Mul(1966080, 3932160)));

            string fill_sec = "";
            if (t_sec < 655360)
            {
                fill_sec = "0";
            }

            return $"{t_min >> 16}:{fill_sec}{t_sec >> 16}:{t_cent >> 16}";
        }

        private static int Clampy(int v)
        {
            return v; // return max(0,min(128,v))
        }

        private static (int, int) Swap(int x1, int x2)
        {
            return (x2, x1);
        }

        private void Rectlight(int x, int y, int sx)
        {
            int mx = F.Min(x, sx);
            int ex = F.Max(x, sx);
            for (int i = F.Floor(mx); i <= ex; i += 65536)
            {
                p8.Pset(i, y, Pal[p8.Pget(i, y) + 65536 - 65536]);
            }
        }

        private void Otri(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            if (y2 < y1)
            {
                if (y3 < y2)
                {
                    (y1, y3) = Swap(y1, y3);
                    (x1, x3) = Swap(x1, x3);
                }
                else
                {
                    (y1, y2) = Swap(y1, y2);
                    (x1, x2) = Swap(x1, x2);
                }
            }
            else
            {
                if (y3 < y1)
                {
                    (y1, y3) = Swap(y1, y3);
                    (x1, x3) = Swap(x1, x3);
                }
            }

            y1 += 65;

            int miny = F.Min(y2, y3);
            int maxy = F.Max(y2, y3);

            int fx = x2;
            if (y2 < y3)
            {
                fx = x3;
            }

            int cl_y1 = Clampy(y1);
            int cl_miny = Clampy(miny);
            int cl_maxy = Clampy(maxy);

            int steps = F.DivPrecise(x3 - x1, y3 - y1);
            int stepe = F.DivPrecise(x2 - x1, y2 - y1);

            int sx = F.Mul(steps, cl_y1 - y1) + x1;
            int ex = F.Mul(stepe, cl_y1 - y1) + x1;

            for (int y = F.Floor(cl_y1); y <= cl_miny; y += 65536)
            {
                Rectlight(sx, y, ex);
                sx += steps;
                ex += stepe;
            }

            sx = F.Mul(steps, miny - y1) + x1;
            ex = F.Mul(stepe, miny - y1) + x1;

            int df = F.DivPrecise(65536, maxy - miny);

            int step2s = F.Mul(fx - sx, df);
            int step2e = F.Mul(fx - ex, df);

            int sx2 = sx + F.Mul(step2s, cl_miny - miny);
            int ex2 = ex + F.Mul(step2e, cl_miny - miny);

            for (int y = F.Floor(cl_miny); y <= cl_maxy; y += 65536)
            {
                Rectlight(sx2, y, ex2);
                sx2 += step2s;
                ex2 += step2e;
            }
        }

        private void Lamp(int lampx, int lampy, int lampdirx, int lampdiry, int lampperpx, int lampperpy, int sidefac, int lamplen, int lampwid)
        {
            int lampp1x = lampx + F.Mul(F.Mul(lampdirx, lamplen), sidefac) + F.Mul(lampperpx, lampwid);
            int lampp1y = lampy + F.Mul(F.Mul(lampdiry, lamplen), sidefac) + F.Mul(lampperpy, lampwid);
            int lampp2x = lampx + F.Mul(F.Mul(lampdirx, lamplen), sidefac) - F.Mul(lampperpx, lampwid);
            int lampp2y = lampy + F.Mul(F.Mul(lampdiry, lamplen), sidefac) - F.Mul(lampperpy, lampwid);
            Otri(F.Floor(lampx), F.Floor(lampy), F.Floor(lampp1x), F.Floor(lampp1y), F.Floor(lampp2x), F.Floor(lampp2y));
        }

        // main draw function
        public void Draw()
        {
            p8.Cls();

            p8.Camera(0, 0);

            // black will not be translucent
            // dark green will be
            p8.Palt(0, false);
            p8.Palt(196608, true);
            p8.Palt(262144, false);

            // start menu
            if (!Isstarted)
            {
                p8.Rectfill(0, 0, 8323072, 8323072, 65536);

                int c = 1048576;

                CenterText(4194304, c, "nusan present", 327680);
                DrawBigFlag("cyclo 8", 2097152, c + 786432, 655360);

                c = 3801088;
                CenterText(2097152, c, "up = gas", 458752);
                CenterText(2097152, c + 524288, "down = brake", 458752);
                CenterText(4194304, c + 1048576, "left-right = rotate the bike", 458752);
                CenterText(6291456, c, "c = flip bike", 458752);
                CenterText(6291456, c + 524288, "v = retry", 458752);

                int flipcol = 393216 + p8.Mod(F.Floor(F.Mul(Flaganim, 32768)), 131072);

                c = 6160384;
                CenterText(4194304, c, "starting level :", 458752);
                CenterText(4194304, c + 524288, $"< {Currentlevel} - {Levels[Currentlevel - 1].Name} >", 524288);
                CenterText(4194304, c + 1179648, "press c to start", flipcol);

                Flaganim += 13107;

                return;
            }

            // background color
            p8.Rectfill(0, 0, 8323072, 8323072, 262144);

            p8.Camera(Camoffx, Camoffy);

            int treeoff = Levels[Currentlevel - 1].Backy;

            // draw the cloud in background :
            int paral2x = F.Mul(Camoffx, 49152);
            int paral2y = F.Mul(Camoffy - treeoff, 49152) + treeoff;

            for (int i = 0; i < 30; i++)
            {
                p8.Circfill(paral2x + (i * 20) << 16 + Cloudsx[i], paral2y + 2949120 + Cloudsy[i], 655360 + Cloudss[i], 327680);
            }

            for (int j = 0; j < 30; j++)
            {
                p8.Circfill(paral2x + (j * 20) << 16 + Cloudsx[j], paral2y + 4063232 + Cloudsy[j], 655360 + Cloudss[j], 262144);
            }

            // draw the trees :
            int paralx = F.Mul(Camoffx, 32768);
            int paraly = F.Mul(Camoffy - treeoff, 32768) + treeoff;
            p8.Palt(196608, false);
            p8.Palt(262144, true);
            // draw the bottom of the trees
            p8.Rectfill(Camoffx, paraly + 4194304 + 524288, Camoffx + 8388608, Camoffy + 8388608, 131072);

            paralx = p8.Mod(paralx, 8388608) + F.Mul(F.Floor(F.DivPrecise(paralx, 8388608)), 16777216);
            // draw 2 series of trees
            // warping infinitly
            p8.Map(7340032, 2621440, paralx, paraly + 1048576, 1048576, 524288);
            p8.Map(7340032, 2621440, paralx + 8388608, paraly + 1048576, 1048576, 524288);
            p8.Palt(196608, true);
            p8.Palt(262144, false);

            // draw the bottom line
            // in black to mask bottom
            // of the level
            p8.Rectfill(Camoffx, 7077888 + treeoff, Camoffx + 8388608, 7208960 + treeoff, 786432);
            p8.Rectfill(Camoffx, 7143424 + treeoff, Camoffx + 8388608, Camoffy + treeoff + 8388608, 0);

            // draw_col()

            // draw the all level
            DrawMap(0);

            for (int j = 0; j < Items.Count; j++)
            {
                var item = Items[j];
                if (item.X + item.Y != 0) // replaced null check
                {
                    DrawItem(ref item);
                    Items[j] = item;
                }
            }

            // draw_entity(entities[1])
            // draw_entity(entities[2])
            //foreach (EntityStruct j in Entities)
            //{
            DrawEntity(ref Wheel0);
            DrawEntity(ref Wheel1);
            //}

            // draw the player :
            int cspr = p8.Mod(F.Floor(-Bikeframe), 262144);
            if (cspr < 0)
            {
                cspr += 262144;
            }
            if (Isdead)
            {
                cspr = 262144;
            }

            int bodyadv = 0;
            if (Bodyrot > 0)
            {
                bodyadv = 65536;
            }
            if (Bodyrot < 0)
            {
                bodyadv = -65536;
            }

            int cspr2 = cspr + 1048576;

            if (Chardown)
            {
                cspr = 327680;
                if (Isdead)
                {
                    cspr = 393216;
                }
            }

            // player lower body
            p8.Spr(6291456 + cspr2, Charx2 - 229376, Chary2 - 294912, 65536, 65536, !Bikefaceright);
            // player upper body
            p8.Spr(6291456 + cspr, Charx - 229376 + F.Mul(bodyadv, 131072), Chary - 393216, 65536, 65536, !Bikefaceright);

            EntityStruct wheel = Wheel0;
            int sidefac = -65536;
            if (Bikefaceright)
            {
                wheel = Wheel1;
                sidefac = 65536;
            }

            int lampdirx = Link1.Dirx;
            int lampdiry = Link1.Diry;
            int lampperpx = lampdiry;
            int lampperpy = -lampdirx;
            int lampx = wheel.X + F.Mul(lampperpx, 262144) + F.Mul(F.Mul(lampdirx, sidefac), 131072);
            int lampy = wheel.Y + F.Mul(lampperpy, 262144) + F.Mul(F.Mul(lampdiry, sidefac), 131072);

            Lamp(lampx, lampy, lampdirx, lampdiry, lampperpx, lampperpy, sidefac, 1310720, 655360);
            
            p8.Circ(F.Floor(lampx), F.Floor(lampy), 65536, 65536);
            p8.Pset(F.FloorToInt(lampx), F.Floor(lampy), 458752);

            // draw the foreground part of the level
            DrawMap(~0x2);

            p8.Camera(0, 0);

            // display hud
            if (true)
            {
                if (Timerlasteleport < 1966080)
                {
                    if (p8.Mod(Timerlasteleport, 262144) < 131072)
                    {
                        CenterText(4194304, 4194304, "teleport", 786432);
                    }
                    Timerlasteleport += 65536;
                }

                // handle going to the next level
                if (Isfinish)
                {
                    int progress = Saturate(F.DivPrecise(F.DivPrecise(Timernextlevel, Timernextlevel_dur) - 19660, 32768));
                    p8.Rectfill(-65536, 0, F.Mul(8388608, progress) - 65536, 8388608, 65536);

                    if (progress > 58982)
                    {
                        if (Currentlevel >= Levelnb)
                        {
                            int c = 2359296;
                            CenterText(4194304, c, "nusan present", 327680);
                            DrawBigFlag("cyclo 8", 2097152, c + 786432, 655360);
                            CenterText(4325376, 4718592, "thanks for playing", 458752);
                        }
                        else
                        {
                            CenterText(4194304, 4194304, "next level :", 458752);
                            CenterText(4194304, 4849664, $"{Currentlevel + 1} - {Levels[Currentlevel].Name}", 458752);
                        }
                    }
                    CenterText(4194304, 917504, $"total over {Totalleveldone >> 16} levels", 786432);
                    CenterText(1441792, 1441792, $"retries:{Totalretries >> 16}", 786432);
                    CenterText(4194304, 1572864, $"score:{Totalscore >> 16}", 786432);
                    CenterText(7340032, 1441792, $"{GetTimeStr(Totaltimer)}", 786432);
                }
                CenterText(1441792, 262144, $"retries:{Retries >> 16}", 524288);
                CenterText(4194304, 131072, $"score:{Score >> 16}", 524288);
                CenterText(7340032, 262144, $"{GetTimeStr(Timer)}", 524288);

                if (Isdead && (!Isfinish))
                {
                    CenterText(4194304, 1310720, "you are dead", 524288);
                    CenterText(4194304, 1835008, "press v to retry", 524288);
                }
                if (Isfinish)
                {
                    DrawBigFlag("victory", 2097152, 2097152, 524288);
                }
            }

            // debug draw values
            if (false)
            {
                /*p8.Print($"{(int)Math.Floor(Entities[Playeridx].X)}", 0, 112, 4);
                p8.Print($"{(int)Math.Floor(Entities[Playeridx].Y)}", 0, 120, 4);

                p8.Print($"{Entities[Playeridx].Vrot}", 24, 112, 4);
                p8.Print($"{Entities[Playeridx].Vx}", 24, 120, 4);

                p8.Print($"{(int)Math.Floor(Camoffx)}", 64, 112, 4);
                p8.Print($"{(int)Math.Floor(Camoffy)}", 64, 120, 4);*/
                p8.Print($"cpu {p8.Stat(1)}", 6291456, 6291456, 458752);
            }
            Flaganim += 13107;
        }

        public string SpriteData = @"
333333337733333333333333333333331110d100001d011133333333333333333333337773333333333333377777777733333333333333331000000000110001
33333333667733333333333333333333d61010d11d01016d33333333333333333333776667333333333333766666666633333333333333331100000111111011
33333333d66677333333333333333333dd11016dd61011dd33333333333333333377666dd67333333333376ddddddddd33333333333333333111111111111111
33333333ddd666773333333333333333dd6d01111110d6dd333333333333333377666ddddd673333333376dddddddddd33333333333333333311111111131113
33333333ddddd66677777777773333336dd1000110001dd63333337733333333666dddddddd6733333376ddddddddddd77333333333333773311111311311313
33333333ddddddd66666666666773333dddd1d1001d1dddd33337766333333336ddddddddddd67333376dddddddddddd66773333333377663313113311313311
3333333311111ddddddddddddd667733dd11d6d11d6d11dd337766dd33333333ddd11111ddddd673376ddddd11111111d66677333377666d3313313331113311
3333333300011111111111111111117711011d1001d110117711111133333333111110001ddddd6776ddddd100000000ddd6667777666ddd3313313331331313
01110100011110000111110000101000733333333333333773333337000000000101010101ddddd66ddddd107333333733333333333333333313133331333113
1dd61d101d76d1011ddd610001d161001733333333333371173333710000000010101010001dddddddddd1006733337633333333333333333313113331131313
01dd110001ddd11d111dd1000011dd1017333333333333711733337100000000010101010001dddddddd1000d673376d33333333333333333313131331111313
00110100001d10010001d10111101100063333333333336006333360000000001010101000001dddddd10000dd6776dd33333333333333333311131333113313
00001dd1d11101000110111ddd6100006333333333333336633333360000000001010101100001dddd100001ddd66ddd33333337733333333311131331111313
61001d6d1001dd1016dd10011dd100163333333333333333333333330000000010101010d101001dd100101ddddddddd33333336633333333331113331311113
7d11d11001dd6dd11dd10000011d11d73333333333333333333333330000000001010101161d10011001d16111dddd1133333336633333333311313331133113
1dd0100000110110011000000001001d333333333333333333333333000000001010101000010000000010000111111033333333333333333313131331313313
10001101100000000011000110110001133333333333333113333331000001000000000001110000001010007777777777777777777777773313131331313313
111111111100000111111011111110131333333333333331133333310111010010110011001d000001ddd10066666dddddddddddddd666663311331331311313
3111311311111111111111111111111313333333333333311333333101dd000111d1101d0011100000011000ddddddd1111111111ddddddd3313111331313113
3313333313111111111311133111113313333333333333311333333100110000011110110000000100001000ddddd11111111111111ddddd3311311311133113
3333333333313311113311333331133313333333333333311333333100000110000000001110011100000000ddd111111111111111111ddd3311311111131313
3333333333333331133331333333333313333333333333311333333110001dd1000111000dd10001d1000010d1111110101010100111111d3311111131111113
3333333333333333333333333333333313333333333333311333333110011d10001dd10101d10000011001d11101010101010101101010113113311311111113
33333333333333333333333333333333133333333333333113333331d00000000000000000100000000000110000000000000000000000001111111111111111
3333337777777777666ddd1111ddd66677333333000000011000000010001010333333331110d116000001100000000033131113331331133333333333333333
33337766666666666ddd11333311ddd66677333310ddd01dd10ddd010001610133388333d610011d011101333311001133111313331113133333333333333333
3377666ddddddddddd113333333311ddd66677330166dd0110dd66100166d10033899833dd11116d01dd33333333101d33131313313311333333333333333333
77666ddd111111111133333333333311ddd666771d661d1001d166d1061dddd03249a983dd6d1ddd001333333333301133131313331313333313313313133333
666ddd1133333333333333333333333311ddd6661d06dd1001dd60d10d6d1d1032499983ddd11dd6003333333333330033133133333111333313113313113333
6ddd11333333333333333333333333333311ddd601d6611001166d1001d1d1d0332448336dd1dddd103333333333330031113333333313333311113111111313
dd113333333333333333333333333333333311dd01161d0110d1611001100d6d33322333ddd111dd133333333333333133133333333333333111111111111113
11333333333333333333333333333333333333111d01d100001d10d1000001d13333333311001011133333333333333033333333333333331111111111111111
3333333333333333333333333333333333333333333c333344444444244444442200222222022022000000000000000000000000000000000000000000000000
333603363336063333363603333330b33333308333cc333344444442224444442220022222222222000000000000000000000000000000000000000000000000
333067503330605733305067333b0b03333808033ccccccc44444422222444442222220022222202000000000000000000000000000000000000000000000000
3336057633360675333676053330b0b333308083cccccccc44444222222244442222222222222220000000000000000000000000000000000000000000000000
333067503330605733305067333b0b0333380803cccccccc44442222222222442000222222202222000000000000000000000000000000000000000000000000
3333357333333375333373353330b333333083333ccccccc44022222222200040022222222222222000000000000000000000000000000000000000000000000
333333333333333333333333333333333333333333cc333344000222000000442202222222222222000000000000000000000000000000000000000000000000
3333333333333333333333333333333333333333333c333344420002222224442222222222222222000000000000000000000000000000000000000000000000
331dd1333311dd3333111d3333d1113333dd11330000000044222202222222242222222222222222000000000000000000000000000000000000000000000000
31d66d1331dd66133d6dd6d33d6dd6d33166dd130000000044022222222220042222220222222220000000000000000000000000000000000000000000000000
1dd66dd1d66d66d1d66dd661166dd66d1d66d66d0000000044400200000200442222200222222022000000000000000000000000000000000000000000000000
d667766dd6677dd11dd77dd11dd77dd11dd7766d0000000044440002220020440022222222222222000000000000000000000000000000000000000000000000
d667766d1dd7766d1dd77dd11dd77dd1d6677dd10000000044422200222200442002222222022222000000000000000000000000000000000000000000000000
1dd66dd11d66d66d166dd66dd66dd661d66d66d10000000044022222200002242200222222022222000000000000000000000000000000000000000000000000
31d66d133166dd133d6dd6d33d6dd6d331dd66130000000040002222202222242222222222222222000000000000000000000000000000000000000000000000
331dd13333dd113333d1113333111d333311dd330000000042200022222222202222002222220222000000000000000000000000000000000000000000000000
333333333333333333333333333333333333333333333333333333333333c333333c333333c33333000000000000000000000000000000000000000000000000
33333333333333333333333333333333333333333333333333333333333cc33333ccc33335cc3333000000000000000000000000000000000000000000000000
333aaa33333aaa33333aaa33333aaa33333aaa3333eeff33338e8f33335ccc3335ccc33333cccc33000000000000000000000000000000000000000000000000
333e9033333e9033333e9033333e9033333880333eeefff338e8f8f335cc7c3335c7cc3335cccc33000000000000000000000000000000000000000000000000
33ee993333ee993333ee993333ee9933338e8833eeeefff38e8e8f8335cccc3335cccc3335cc7c33000000000000000000000000000000000000000000000000
3eeeff333eeeff333eefff333eeeff3338e8f8339399fff39388f8f3335cc533335cc533335cc533000000000000000000000000000000000000000000000000
3eeeff933eeff9333eef9f933eeff9333e8e8f833309f39333088393333553333335533333355333000000000000000000000000000000000000000000000000
33eeff3333eeff3333eeff3333eeff3333e8f83333aaa33333aaa333333333333333333333333333000000000000000000000000000000000000000000000000
33333333333333333333333333333333333333330000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
33eeff3333eeff3333eeff3333eeff33338e8f330000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
3eeefff33eeefff33eeefff33eeefff338e8f8f30000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
3eeefff33eeefff33eeeeff33eeefff33e8e8f830000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
3ee33ff333eeff3333feeef333eeff3338e338f30000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
3ee11ff33311ff333fffee3333ee11333e811f830000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
311111133331ff333111111333ee1333311111130000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
33311333333311333333333333113333333113330000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
000000000000000000000000000000000000000000000000000000000060300000000000000000000083000000d0032300000000000000000000000000000000
00000000000000603000000060300000000000000083000000000000000000000000000000000000000000000000000000000000000000000000000000000060
0000000000000000000000000000000000000000000000000000006080407241000000000000000000d080b1b1a1420000000000000000000000000000000000
00000000603060406341005101114183000000006020203000000000000000000000000000000000000000000000000000000000000000000000000000005182
000000000000000000000000830000000000040000000000000051b321a3020000000000000000c180a19282720142000000000000000000000000000000e3f3
0000345101218292503000528250102020300051222202f041000000000000000000000000000000000000000000000083000000000000000000000000000092
3000000000000000000000000000000000d02020c000000000000052314283000000040000c180b29292533101824200300000000000000083000000c180b2d2
1020202040827192a20141528292a27222124100000000f10000a0b0900000000000000000000000e30000000000602020202030000000000000000000440021
31410000000000d020300000000000600323e0f03343300000000052824200006020202020804092720111721131420001410000000000602030000052821131
72922182a3e0e022f032005272a3023200000000000000e2a0b0a1929110c00000000000040000a0b24100000051f0122212e01241000000d080b010c000d040
a24200000000a0a172824100000051e00000f1e10000e04100830052634200512202e022e0f02212922172f082a242001100000000005112f0224100001282a2
2212223200d3e100f100005272428300000400d0d1c180b0a182a3e02212f04100a090e3f3d080a1a30000000400c3008300f1000000d080a192825391b0a192
9242830000a0a1a312320000000000c30000e1f10000f10000000052509000000000f100c3e10000021222d3123200006300000000000000f100000000000232
00000000e300f183c30000523142000000d00323000002121250c0e28300f2d080a191b0b0a1a3028300e3f3e3f3d0031341d300d080a17211a27282a2319292
824200600323020000000000000000000000f1e10000c360c0000000b39110c00000d30083d30000000000000000000050c0000000000000e100000000000000
0000d0031341f20000000052534200c1032300000000000000b391b0b0b0b0a19282722163824276005113131313230000c12080a12212122202321222021222
634251220000040000000000000000000000d3f100005131919000000002b39110203000000083000000000000000000a29110c000000000f100000000000000
60032300c180b010d10000528242000000000000008300000000e0021222f022e002122212f00000000000000000000000000000000000000064740000000000
11420000000000d03000000083000000000000e10000835392919000000000e0f01222410000000000000000000000008272019110c00000d300000000000051
7200000000e0f022000004525010202020202030000000000000e1000000f100d300000000f10000000000000000000000000000006474000065750000006474
501020202003132332410000000000a0900000f10000009282a29190000000d3f1000000000000a09000000000000000310131827291900000000000e3000052
2183000000e1f1000060208040a302e0f0e01222410000000000d3000000e1000000000000c30000000000000083000000647400006575006485750000006575
72f002e0220000000000e30000c180a19110c0f2f3d08040a302b39110c00000c300000000d080a1919000000004000073a302f0e0121241000000a0b2410052
3100000000e1c3005122f022320000e1f1d300000000000000a090000000c3000000000000000000000000602020300000657564746585746584857400648475
31f100d30000000000a0b2410000b372920191b0b0a131a3008300e0b391b2410000006080a12131a29110c000000000014283e1f18300000000a0a192428352
50c0000400d300000000f100000000d3e10000040000c180b0a19110c000000000000000000000d03000511282a2224164847565848484756585847564858575
11c33400000000d080a18242000052a28272211131a3f000000000f100e0f00000005101113172823101a33313131341214200f1c300000000a0a12172420052
729110202030000000e3f2f300000000c300d0202080b2927253a2729110c0000000040000d080a1224100527263420085848584858494848584858484858485
50202020202080a18272714200005271719292317342c3760000e3d300c3d3000000523182319272a282420000000000509000d3040000d080a1828271427652
92a292a22182416080b2c2d2b1b1b19000a0a192723111820121718221729110c0e3f3d080a12192420000528292420084949585849594858494958585849484
8292213101822172a271714200005271717182a2501020202080b01020202020d100527221927171727242000000000092911020202080a17211927171420052
7282717171725172119271a28221a291b1a172118271a271a29271717282829291b0b0a192a29282420000520182420000000094959400959400009595940000
00122222222221000012222100000000000000000000000000000000000000000000000000000000000000001222210000122210000000000000000001222100
02466666666664200246666421000000000000000000000000000000000000000000000000000000000000124666642002466642000000000000000024666420
147aaaaaaaaaa741147aaaa764210000000000000000000000000000000000000000000000000000000012467aaaa741147aaa74200000000000000247aaa741
26adeeeeeeeeda6226adeedaa76421000000000000000000000000000000000000000000000000000012467aadeeda6226adeda742000000000000247adeda62
26aeffffffffea6226aeffeedaa7642000122222222221000012222100000000000000001222210002467aadeeffea6226aefeda7420000000000247adefea62
26aeffffffffea6226aeffffeedaa741024666666666642002466664210000000000001246666420147aadeeffffea6226aeffeda74200000000247adeffea62
26aeffffffffea6226aeffffffeeda62147aaaaaaaaaa741147aaaa764210000000012467aaaa74126adeeffffffea6226aefffeda742000000247adefffea62
26aeffffffffea6226aeffffffffea6226adeeeeeeeeda6226adeedaa76421000012467aadeeda6226aeffffffffea6226aeffffeda7420000247adeffffea62
26aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffeedaa7642002467aadeeffea6226aeffffffffea6226aefffffeda74200247adefffffea62
26aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffeedaa741147aadeeffffea6226aeffffffffea6226aeffffffeda741147adeffffffea62
26aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffffeeda6226adeeffffffea6226aeffffffffea6226aefffffffeda6226adefffffffea62
26aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffffffffea62
26adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda62
147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741
02466666666664200246666666666420024666666666642002466666666664200246666666666420024666666666642002466666666664200246666666666420
00122222222221000012222222222100001222222222210000122222222221000012222222222100001222222222210000122222222221000012222222222100
00122210012221000000000012222100001222222222210000122222222221000012222222222100001222210000000000122222222221000012222222222100
02466642246664200000001246666420024666666666642002466666666664200246666666666420024666642100000002466666666664200246666666666420
147aaa7447aaa741000012467aaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaaaaaaaa741147aaaa764210000147aaaaaaaaaa741147aaaaaaaaaa741
26adeda77adeda620012467aadeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeeeeeeeeda6226adeedaa764210026adeeeeeeeeda6226adeeeeeeeeda62
26aefedaadefea6202467aadeeffea6226aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffeedaa7642026aeffffffffea6226aeffffffffea62
26aeffeddeffea62147aadeeffffea6226aeffffffffea6226aeffffffeeda6226adeeffffffea6226aeffffeedaa74126aeffffffeeda6226adeeffffffea62
26aefffeefffea6226adeeffffffea6226aeffffffffea6226aeffffeedaa741147aadeeffffea6226aeffffffeeda6226aeffffeedaa741147aadeeffffea62
26aeffffffffea6226aeffffffffea6226aeffffffffea6226aeffeedaa7642002467aadeeffea6226aeffffffffea6226aefffedaa7642002467aadefffea62
26aeffffffffea6226aeffffffffea6226adeeeeeeeeda6226adeedaa76421000012467aadeeda6226aeffffffffea6226aeffeda76421000012467adeffea62
26aeffffffffea6226aeffffffeeda62147aaaaaaaaaa741147aaaa764210000000012467aaaa74126adeeffffffea6226aeffea7421000000001247aeffea62
26aeffffffffea6226aeffffeedaa741024666666666642002466664210000000000001246666420147aadeeffffea6226aefeda6200000000000026adefea62
26aeffffffffea6226aeffeedaa7642000122222222221000012222100000000000000001222210002467aadeeffea6226aefea741000000000000147aefea62
26adeeeeeeeeda6226adeedaa76421000000000000000000000000000000000000000000000000000012467aadeeda6226adeda620000000000000026adeda62
147aaaaaaaaaa741147aaaa764210000000000000000000000000000000000000000000000000000000012467aaaa741147aaa74100000000000000147aaa741
02466666666664200246666421000000000000000000000000000000000000000000000000000000000000124666642002466642000000000000000024666420
00122222222221000012222100000000000000000000000000000000000000000000000000000000000000001222210000122210000000000000000001222100
".Replace("\n", "").Replace("\r", "");

        public string FlagData = @"
0001010101010100010101010101020201010101000000010101010100000202020202020000000101010101010102020101010101010101040101010202020208080808080200000000000000000000000000000000000000000000000000000000000000000004040400000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
".Replace("\n", "").Replace("\r", "");

        public string MapData = @"
030000000000380d1d060c0000003800000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000380000001510140000000000000000000000003800000000000000252a292736132827282a292813121110282917
10140000000a0b1a16271901081b1b09000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d0202030000251224000000000000000000000d082b2c140000430000252811123a2021220e0f202122233b13112717
12240006081a3a23252827282a3512190103000000000000000000000000000000000000000000000000000000000038000000000000000000000000000000000000430000000000000000000000001532383b283e3f25352400000000000000060230320e3b2900060203380025123a0e003800002e2f38004038003b121329
1324002021234300380e0f3b272a272a1129140000000000000000000000000000000000000000000000003800000d02020300000000000000000000000000000006020203000000000000380d0c0000000000122d0b2b050c38000000000000200f00001f00293f27131014002513243c0006080b0b0b010c000000003b1127
05010202020202020c3c3d240e22230e0f230000000d0c00000000000000000000000000000000000006020202081a372827140000000000000000000000000000200e212300000000000d30320e1400000000112829172a19010c0d03000000003c00001e0013292a281324002510240015103a200e233b1901030038251228
101127282a29273619010c241e38003d3c0000380a1a2a14000000000000000000000d081b0900001527282927283a0e220f000000000000000000000000000006033d000000063031313200003c000d0c000d04272a29133a0e333223001c0c004000001f00102a172912240025352438252924443d38000e3b281400252729
1213103510293713272919092e0d0202020306081a292a2400060c00000000000d081a273629140000210e3d0e20001e003d0000000000000000000000000000123a0038001521140000000000000a1a190b1a102837103a003c000000000033340202031e381235172711243825132400250501020c00001f2512240025282a
1011121312131011101112190b1a2a28272a27292a102924152919093840000a1a3a0f0f0e3d000000401f001e00003c00000000000000000000440000000000363800000000000000000040000a1a3a20223b12113a0e00000000000000000000000e221f00112817291024002505090000210f2333341d1e25352438253529
12131011101112132a292927282917171717171717271724252927190102081a3a001e3d3c3e00003e3f2e3e2f3f38000000000000000000000000380000000005090000000000000000000d081a3a000000000e0f001f00000000000000003800003c003d0a042a17291224002527190938001e000000383c0a042400251228
282729281213102817171729271717171717171717171724251717272a29222100003d00152c2c2d0b0b2b2c2c2d093e000000000000380d02082b2c1400000023333402020c000000153132202100000000001f1e003d00000000003800000000060202081a27292a13102400000e2819010c2e3e3f3f0d081a102400251329
171717172a292717171717171717171717171717171717242517171717050c0038000000252711131213102a1229192b1400000000000a1a35272a2a240000000000000e0f33341d38000000000000000000001e3c000d3014000006020c000000200e22230f0e232812050900003d203b29190b0b0b0b1a2a3a2000380a042a
17171717171717171717171717171717171717292a281724252a28171717190102021d00000e2121220f0f351127102924000000000a1a2927112927240000000000001e3d00000000000000000000000000403d0d303200000000200f23000000001e00001f3d002827121909380000000e0f0e220f0e0f230000000a1a1129
17171717171717171717171717171717172a13111013282425131129271711173a000000003c0000003c1f2829111210240000000a1a2728292a1329240000000000001e000000001c02020c0000380000000d3032004400000000001e00000000002e00003c0000172a291319010c00381e3d1f403c1f1e0038000a1a10132a
1717171717171717171717171717171729103a67210f2300000e0f20230e200e000000000006020c38003d0e102822230000000a1a292a1728172928240000000000003c000d301400003b191b1b1b1b1b1b1a2400003e3f000000001f0038000d080b010c380d1d17172728101019010c2e3e2f3e3f2e3d3e0d081a12372929
1717171717171717171717171717172728360000003c0000383c3d00003c002e0000000015292719010c003c0f1f0000380d081a272917171717171724000000003e380d3032000000000020212223202221230000152c2c14001c0c2f3f0d303220223b190b1a241717172929111210190b0b0b0b0b0b0b0b1a111027282717
171717171717171717171717171717172a050c0d02020800000102020202080b01030000252a11123619010c2f2e3f0d081a1210132a171717171717240000000033313200000000000000000000000000000000000021220000003331313200000000002021230017171717272a291310111229271313102a2810292a171717
0300000000000000000000000000000000000000060c003840000000000015102a272400252927132a2927190b0b0b1a27372913171717171717171724000000060300000000000603000000000000000000000000000000000000000000000000000000000000003e3f3e3f06082b2c2d01033e3f3e00000000000000000000
1014000000000000000038000000000d1d0000152919010c000000000000251117292400251717171728131127291112292a2817171717171717171724000000292814000000150e0f1400000000000000000000000000000000000000000000000000000000001535112712133a0e0f3b121128132d01033e3f3e3f3e3e3f3e
102400000000000d082b2c2c2c2c2d1a24000025271029190b0b011d0000251217172400251717171717122a282a101228171717171717171717171724000000111024000000001e1f000000000000000000380000000000000000000000000000380000000000000e0f222123001e3d2513121312132a29272a371012271717
1324430000000a1a35292827293a2023000000252911122a29283a00003825351717240025171717171717171728271717171717171717171717171724000000271124000000001f1e0000000000000000000000003800400000000d0c00000000000038000000003d1e000000003c0025103a20212223202122233b132a2817
0501020202081a121311293a0f0000000000000020212220212200000000251100004400380000000000000000000000000000000000000000000000000000002a1324430000001e1f38000d030000000000000602020c3e000d081a190103000000000000004000001f00000000000025110038000040000038000029112817
101127282a27113a20210f001e00003800003800000000000000004500000a0400060203000000000000000000000000000000000000000000000000000000001310243e3f3e3f2f2e0d081a27140000000015111310190b0b1a3622210e0f140000000000003e3f3e2f0000000000380a390102020c3e0d0300000028132a28
28292a292712120000001f003d0000000000001c0c000000000000000d081a27151028121400000000000000000000000000000000000000000000000000000028112d0b0b2b2c2c2d1a2727122400000000252a1213282a2927050c383d3c000000000d02080b0b0b2b140000000d081a292223233331322014000027291127
2a3a0e210f3b130000003c000000000000000025192b2c2d0b0b0b0b1a29372925292a282400000000000000000000003800000000000000000000000000000627281113271013121137102828273628352937292812111312102919010202020202081a11131211131224001c081a292a3a000000000000000000002a281027
27003d001e0010380000000006082b1400000000203b272829272a29282a28122529271224000000000000000000000006030000001c0c0000000000000015282927123a200e22202122202321220f230e0f222320210e210f0e0f212220222321272829291013272a362400000e220f23000000000000000000000029351328
130000381e00050c00004000210f0f000000000000002122220e0f0e0f220f102513290509000038000000004000001521221400000033340c00000000003813360f2200001e00000000000000001f001e1f000000001e001f1e1f0000380000000f233b272a271329132400001e001f0000000d030000000000000027102a17
110000003c001119010c0000383c3d380000000000000000383d1e1f1e003d11251228101901021d0000000006030000003800000603000033340c0000000027121f0000003c00000000000038001e001f1e000000003c001e1f3d000d020c00001f38000f21233b29050900003d401e00000a1a281400000000000028131129
36000000000d042829190b2b2c2c2d011d0000000000000000003c3d3c00003525102a1137131124000000150f0e140000000015101114000025190900000028131e0000000038000000000000003d001e1f4000380000001f3c0015320033341d1e00003c0000003b2a19010c3e3f2e0d081a3a0e0000000000000d04122917
12380006081a3a0f293612280f0f0e0f0000000000000000000608010c38001325292829292805093e0d03001e1f0000000006080436240000002033341d0013101f000000000000000d0c00000000003c2f3f000000003e2f00003800000000001f0000060c0000003b2812192b2c2d1a2220003c000000000d081a10292a17
100000210f23401f0e0f220f1f1e1f3d00380000003800001521203b192b2d04003b1311123a3b190b1a3a141f1e000000151013292924000000400000000012123d44000000000d081a190102020300152c2d1b1b1b1b2b2c14000067000000003d00152819090000002021220e220f220000004500000d30323b2813282717
05010202020c3e2e3d3c383d3c2e2f0d1d001c0c3e0d08090044000d042a1027000020210f000021222300001e3c0040000a0411270501020202020202020804130c3e0d0202081a29282a122927292425272a2917172829282400000000000000000025282919010c000000003c403d00000000000d303200000027122a1717
29122a2829192b2d01020202082b2d1a240025190b1a28190102081a2912282a000000003c000000000000003d00000d3032202122212220232021222021212328190b1a291029271717172a17171724252a2817171717272a240000000000000000002527122827190102020202020202020202081a24000000002829171717
0000000000000000000000000000000000000000000000000000000000060300000000000000000000380000000d30320000000000000000000000000000000000000000000000060300000006030000000000000038000000000000000000000000000000000000000000000000000000000000000000000000000000000006
00000000000000000000000000000000000000000000000000000006080427140000000000000000000d081b1b1a24000000000000000000000000000000000000000000060306043614001510111438000000000602020300000000000000000000000000000000000000000000000000000000000000000000000000001528
0000000000000000000000003800000000004000000000000000153b123a2000000000000000001c081a29282710240000000000000000000000000000003e3f00004315101228290503002528050102020300152222200f14000000000000000000000000000000000000000000000038000000000000000000000000000029
03000000000000000000000000000000000d02020c000000000000251324380000004000001c082b29293513102824000300000000000000380000001c082b2d01020202042817292a10142528292a27222114000000001f00000a0b0900000000000000000000003e0000000000060202020203000000000000000000440012
131400000000000d020300000000000630320e0f3334030000000025282400000602020202080429271011271113240010140000000000060203000025281113272912283a0e0e220f230025273a2023000000000000002e0a0b1a2919010c00000000004000000a2b14000000150f2122210e21140000000d080b010c000d04
2a24000000000a1a272814000000150e00001f1e00000e14003800253624001522200e220e0f22212912270f282a240011000000000015210f2214000021282a22212223003d1e001f000025272438000040000d1d1c080b1a283a0e22210f14000a093e3f0d081a3a00000040003c0038001f0000000d081a292835190b1a29
29243800000a1a3a212300000000003c00001e1f00001f00000000250509000000001f003c1e00002021223d2123000036000000000000001f00000000002023000000003e001f383c00002513240000000d30320000202121050c2e38002f0d081a190b0b1a3a2038003e3f3e3f0d3031143d000d081a27112a27282a132929
2824000630322000000000000000000000001f1e00003c060c0000003b19010c00003d00383d00000000000000000000050c0000000000001e0000000000000000000d3031142f00000000253524001c3032000000000000003b190b0b0b0b1a29282712362824670015313131313200001c02081a2221212220232122202122
3624152200004000000000000000000000003d1f000015131909000000203b19010203000000380000000000000000002a19010c000000001f00000000000000063032001c080b011d00002528240000000000000038000000000e2021220f220e202122210f0000000000000000000000000000000000000046470000000000
112400000000000d03000000380000000000001e00003835291909000000000e0f21221400000000000000000000000028271019010c00003d0000000000001527000000000e0f220000402505010202020202030000000000001e0000001f003d000000001f0000000000000000000000000000004647000056570000004647
0501020202303132231400000000000a0900001f00000029282a19090000003d1f0000000000000a09000000000000001310132827190900000000003e00002512380000001e1f0000060208043a200e0f0e21221400000000003d0000001e0000000000003c0000000000000038000000464700005657004658570000005657
270f200e2200000000003e00001c081a19010c2f3f0d08043a203b19010c00003c000000000d081a1909000000400000373a200f0e2121140000000a2b14002513000000001e3c0015220f222300001e1f3d000000000000000a090000003c000000000000000000000000060202030000565746475658475648584700464857
131f003d00000000000a2b1400003b272910190b0b1a133a0038000e3b192b1400000006081a12132a19010c000000001024381e1f38000000000a1a29243825050c0040003d000000001f000000003d1e00004000001c080b1a19010c000000000000000000000d03001521282a221446485756484848575658485746585857
113c43000000000d081a28240000252a28271211133a0f000000001f000e0f00000015101113272813103a33313131141224001f3c000000000a1a12272400252719010202030000003e2f3f000000003c000d0202082b2927352a2719010c0000004000000d081a221400252736240058485848584849485848584848584858
050202020202081a28271724000025171729291337243c6700003e3d003c3d0000002513281329272a282400000000000509003d4000000d081a282817246725292a292a12281406082b2c2d1b1b1b09000a1a292713112810121728122719010c3e3f0d081a1229240000252829240048495958485949584849595858484948
28291213102812272a171724000025171717282a0501020202080b01020202021d002527122917172727240000000000291901020202081a271129171724002527281717172715271129172a28122a191b1a271128172a172a29171727282829190b0b1a292a2928240000251028240000000049594900594900005959490000
0021222222221200002122120000000000000000000000000000000000000000000000000000000000000000212212000021220100000000000000001022120020646666666646022064664612000000000000000000000000000000000000000000000000000000000000216466460220646624000000000000000042664602
41a7aaaaaaaa7a1441a7aa7a4612000000000000000000000000000000000000000000000000000000002164a7aa7a1441a7aa47020000000000002074aa7a1462daeeeeeeeead2662daeead7a461200000000000000000000000000000000000000000000000000002164a7daeead2662dade7a2400000000000042a7edad26
62eaffffffffae2662eaffeead7a46020021222222221200002122120000000000000000212212002064a7daeeffae2662eaefad4702000000002074dafeae2662eaffffffffae2662eaffffeead7a1420646666666646022064664612000000000000216466460241a7daeeffffae2662eaffde7a240000000042a7edffae26
62eaffffffffae2662eaffffffeead2641a7aaaaaaaa7a1441a7aa7a4612000000002164a7aa7a1462daeeffffffae2662eaffefad470200002074dafeffae2662eaffffffffae2662eaffffffffae2662daeeeeeeeead2662daeead7a461200002164a7daeead2662eaffffffffae2662eaffffde7a24000042a7edffffae26
62eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffeead7a46022064a7daeeffae2662eaffffffffae2662eaffffefad47022074dafeffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffeead7a1441a7daeeffffae2662eaffffffffae2662eaffffffde7a1441a7edffffffae26
62eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffffeead2662daeeffffffae2662eaffffffffae2662eaffffffefad2662dafeffffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae26
62daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2641a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a14
2064666666664602206466666666460220646666666646022064666666664602206466666666460220646666666646022064666666664602206466666666460200212222222212000021222222221200002122222222120000212222222212000021222222221200002122222222120000212222222212000021222222221200
0021220110221200000000002122120000212222222212000021222222221200002122222222120000212212000000000021222222221200002122222222120020646624426646020000002164664602206466666666460220646666666646022064666666664602206466461200000020646666666646022064666666664602
41a7aa4774aa7a1400002164a7aa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aaaaaaaa7a1441a7aa7a4612000041a7aaaaaaaa7a1441a7aaaaaaaa7a1462dade7aa7edad26002164a7daeead2662daeeeeeeeead2662daeeeeeeeead2662daeeeeeeeead2662daeead7a46120062daeeeeeeeead2662daeeeeeeeead26
62eaefaddafeae262064a7daeeffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffeead7a460262eaffffffffae2662eaffffffffae2662eaffdeedffae2641a7daeeffffae2662eaffffffffae2662eaffffffeead2662daeeffffffae2662eaffffeead7a1462eaffffffeead2662daeeffffffae26
62eaffeffeffae2662daeeffffffae2662eaffffffffae2662eaffffeead7a1441a7daeeffffae2662eaffffffeead2662eaffffeead7a1441a7daeeffffae2662eaffffffffae2662eaffffffffae2662eaffffffffae2662eaffeead7a46022064a7daeeffae2662eaffffffffae2662eaffefad7a46022064a7dafeffae26
62eaffffffffae2662eaffffffffae2662daeeeeeeeead2662daeead7a461200002164a7daeead2662eaffffffffae2662eaffde7a461200002164a7edffae2662eaffffffffae2662eaffffffeead2641a7aaaaaaaa7a1441a7aa7a4612000000002164a7aa7a1462daeeffffffae2662eaffae4712000000002174eaffae26
62eaffffffffae2662eaffffeead7a1420646666666646022064664612000000000000216466460241a7daeeffffae2662eaefad2600000000000062dafeae2662eaffffffffae2662eaffeead7a46020021222222221200002122120000000000000000212212002064a7daeeffae2662eaef7a1400000000000041a7feae26
62daeeeeeeeead2662daeead7a461200000000000000000000000000000000000000000000000000002164a7daeead2662dade6a0200000000000020a6edad2641a7aaaaaaaa7a1441a7aa7a4612000000000000000000000000000000000000000000000000000000002164a7aa7a1441a7aa47010000000000001074aa7a14
2064666666664602206466461200000000000000000000000000000000000000000000000000000000000021646646022064662400000000000000004266460200212222222212000021221200000000000000000000000000000000000000000000000000000000000000002122120000212201000000000000000010221200
".Replace("\n", "").Replace("\r", "");

    }

}
