using System;
using System.Diagnostics.CodeAnalysis;

namespace Searchlo8
{
    public class Cyclo8 : ICart
    {
        #region globals
        private static Pico8 p8;

        private readonly double Base_frameadvback;
        private readonly double Base_frameadvfront;
        private readonly double Base_speedback;
        private readonly double Base_speedfront;
        private readonly double Base_speedlerp;
        private bool Bikefaceright;
        private double Bikeframe;
        private double Bodyrot;
        private int Camadvanx;
        private double Camoffx;
        private double Camoffy;
        private bool Chardown;
        private double Charx;
        private double Charx2;
        private double Chary;
        private double Chary2;
        private readonly List<double> Cloudss;
        private readonly List<double> Cloudsx;
        private readonly List<double> Cloudsy;
        public int Currentlevel;
        private bool Dbg_checkfound;
        private int Dbg_curcheckcount;
        private int Dbg_lastcheckidx;
        public readonly List<EntityClass> Entities;
        private double Flaganim;
        private double Goalcamx;
        private double Goalcamy;
        public bool Isdead;
        private bool Isfinish;
        public bool Isstarted;
        private readonly int Item_apple;
        private readonly int Item_checkpoint;
        private readonly int Item_finish;
        private readonly int Item_start;
        private readonly int Item_teleport;
        private int Itemnb;
        private List<ItemClass> Items;
        private double Last_check_x;
        private double Last_check_y;
        private readonly int Levelnb;
        private readonly List<LevelClass> Levels;
        private readonly double Limit_col;
        private readonly double Limit_wheel;
        private readonly LinkClass Link1;
        private int[] Pal;
        private readonly int Playeridx;
        private bool Restartafterfinish;
        private int Retries;
        private int Score;
        private readonly List<int> Sdflink;
        private readonly int Stepnb;
        private readonly double Str_air;
        private readonly double Str_bodyrot;
        private readonly double Str_gravity;
        private readonly double Str_link;
        private readonly double Str_reflect;
        private readonly double Str_wheel;
        private readonly double Str_wheel_size;
        public int Timer;
        private int Timerlasteleport;
        private double Timernextlevel;
        private readonly double Timernextlevel_dur;
        private int Totalleveldone;
        private int Totalretries;
        private int Totalscore;
        private int Totaltimer;
        #endregion
        
        public Cyclo8(Pico8 pico8)
        {
            p8 = pico8;

            Camoffx = 0;
            Camoffy = -64;
            Goalcamx = 0;
            Goalcamy = -64;
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

            Dbg_curcheckcount = 1;
            Dbg_checkfound = false;
            Dbg_lastcheckidx = 0;

            // physics settings :
            // nb of physics substeps
            Stepnb = 10;
            // strengh of rebound
            Str_reflect = 1.1;
            Str_gravity = 0.06;
            Str_air = 0.99;
            Str_wheel = 0.25;
            Str_wheel_size = 1.0;
            Str_link = 0.5;
            // rotation of the bike
            // according to arrow keys
            Str_bodyrot = 0.04;
            // acceleration factor
            Base_speedlerp = 0.5;
            // max speed front
            Base_speedfront = 0.18;
            // max speed back
            Base_speedback = 0.03;
            Base_frameadvfront = 0.3;
            Base_frameadvback = 0.15;

            Limit_col = 2.0;
            Limit_wheel = 1.5;

            Bodyrot = 0.0;

            Playeridx = 1;

            Currentlevel = 1;
            Levelnb = 7;

            Timernextlevel = 0;
            Timernextlevel_dur = 30 * 7;
            Timerlasteleport = 1000;

            Cloudsx = new(new double[60]);
            Cloudsy = new(new double[60]);
            Cloudss = new(new double[60]);
            Levels = new(new LevelClass[7]);
            Entities = new(new EntityClass[2]);
            Pal = [];

            Itemnb = 0;
            Item_apple = 1;
            Item_checkpoint = 2;
            Item_start = 3;
            Item_finish = 4;
            Item_teleport = 5;

            Items = new(new ItemClass[1000]);
            Link1 = LinkNew(1, 2);

            // array to link sprite to colision
            Sdflink = new(new int[11 + 16 * 3])
            {
                [1 - 1] = 1,
                [2 - 1] = 2,
                [3 - 1] = 3,
                [12 - 1] = 3,
                [6 - 1] = 4,
                [13 - 1] = 4,
                [8 - 1] = 5,
                [9 - 1] = 6,
                [10 - 1] = 7,
                [11 + 16 - 1] = 8,
                [0 + 16 * 3 - 1] = 9,
                [1 + 16 * 3 - 1] = 10,
                [2 + 16 * 3 - 1] = 11,
                [3 + 16 * 3 - 1] = 12,
                [4 + 16 * 3 - 1] = 13,
                [10 + 16 * 3 - 1] = 14,
                [11 + 16 * 3 - 1] = 15
            };
        }
        
        // map zone structure.
        // level is made of several zones
        private class ZoneClass(int inStartx, int inStarty, int inSizex, int inSizey)
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
        private class LevelClass(string inName, int inZkill, int inBacky, int inCamminx, int inCammaxx, int inCamminy, int inCammaxy)
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
        public class EntityClass(double inx, double iny)
        {
            public double X = inx;
            public double Y = iny;
            public double Vx = 0.0;
            public double Vy = 0.0;
            public double Rot = 0.0;
            public double Vrot = 0.0;
            public bool Isflying = true;
            // public int Lastcolx = X;
            // public int Lastcoly = Y;
            // public int Lastcolnx = 0;
            // public int Lastcolny = 0;
            public LinkClass? Link = null;
            public int Linkside = 1;
        }

        private static EntityClass EntityNew(double inx, double iny)
        {
            return new EntityClass(inx, iny);
        }

        private class ItemClass(double inx, double iny, int inType)
        {
            public double X = inx;
            public double Y = iny;
            public int Type = inType;
            public bool Active = true;
            public int Size = 8;
        }

        private static ItemClass ItemNew(double inX, double inY, int inType)
        {
            return new ItemClass(inX, inY, inType);
        }

        // a physic link between wheels
        public class LinkClass(int ent1, int ent2)
        {
            public int Ent1 = ent1;
            public int Ent2 = ent2;
            public double Baselen = 8.0;
            public double Length = 8.0;
            public double Dirx = 0.0;
            public double Diry = 0.0;
        }

        private static LinkClass LinkNew(int ent1, int ent2)
        {
            return new LinkClass(ent1, ent2);
        }

        private static double Lerp(double a, double b, double alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
        }

        private static double Saturate(double a)
        {
            return Math.Max(0, Math.Min(1, a));
        }

        private void CreateLevels()
        {
            int l = 1;
            Levels[l - 1] = LevelNew("long road", 256, 144, 512, 896, -1000, 128);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(64, 16, 64, 16);
            Levels[l - 1].Zonenb = 1;

            l = 2;
            Levels[l - 1] = LevelNew("easy wheely", 125, 16, 0, 400, -1000, 58);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(0, 0, 64, 16);
            Levels[l - 1].Zones[2 - 1] = ZoneNew(32, 16, 32, 4);
            Levels[l - 1].Zonenb = 2;

            l = 3;
            Levels[l - 1] = LevelNew("central pit", 256, 144, 0, 128, -1000, 128);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(0, 16, 32, 16);
            Levels[l - 1].Zonenb = 1;

            l = 4;
            Levels[l - 1] = LevelNew("spiral", 125, 16, 834, 896, 2, 2);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(104, 0, 24, 16);
            Levels[l - 1].Zonenb = 1;

            l = 5;
            Levels[l - 1] = LevelNew("sky fall", 125, 16, 512, 700, -1000, 0);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(64, 0, 40, 16);
            Levels[l - 1].Zonenb = 1;

            l = 6;
            Levels[l - 1] = LevelNew("here and there", 386, 272, 384, 896, -1000, 256);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(48, 32, 64, 16);
            Levels[l - 1].Zones[2 - 1] = ZoneNew(112, 32, 16, 8);
            Levels[l - 1].Zonenb = 2;
            Levels[l - 1].Startright = false;

            l = 7;
            Levels[l - 1] = LevelNew("ninja rise", 386, 256, 0, 385, -1000, 256);
            Levels[l - 1].Zones[1 - 1] = ZoneNew(0, 32, 48, 16);
            Levels[l - 1].Zones[2 - 1] = ZoneNew(32, 20, 32, 12);
            Levels[l - 1].Zonenb = 2;
        }

        public void Init()
        {
            // uncomment the next line
            // to regenerate sdf sprite
            // GenAllSdf();

            Pal = [5, 13, 15, 11, 9, 6, 7, 7, 14, 10, 7, 7, 7, 6, 15, 7];

            CreateEntities();
            CreateLevels();
            CreateClouds();
        }

        private void CreateClouds()
        {
            int i = 1;
            while (i <= 60)
            {
                Cloudsx[i - 1] = p8.Rnd(0, 10);
                Cloudsy[i - 1] = p8.Rnd(0, 5);
                Cloudss[i - 1] = p8.Rnd(0, 10);
                i += 1;
            }
        }

        private void StartLevel(int levelidx)
        {
            Currentlevel = levelidx;

            Items = new(new ItemClass[1000]);
            Itemnb = 0;
            Bikefaceright = Levels[Currentlevel - 1].Startright;

            FindReplaceItems();
            ResetCamera();
            ResetPlayer();
            Score = 0;
            Retries = 0;
            Timer = 0;
            Restartafterfinish = false;

            p8.Sfx(7, 2);
        }

        private void FindReplaceItemsZone(int startx, int starty, int sizex, int sizey)
        {
            for (int i = startx; i <= startx + sizex - 1; i++)
            {
                for (int j = starty; j <= starty + sizey - 1; j++)
                {
                    int col = p8.Mget(i, j);
                    int flags = p8.Fget(col);
                    int itemtype = 0;
                    
                    if (p8.Band(flags, 4) > 0)
                    {
                        itemtype = Item_teleport;
                        if (col == 56)
                        {
                            itemtype = Item_apple;
                        }
                    }
                    if (p8.Band(flags, 8) > 0)
                    {
                        itemtype = Item_teleport;
                        if (col == 67)
                        {
                            itemtype = Item_start;
                            Last_check_x = 8 * i + 4;
                            Last_check_y = 8 * j + 4;
                        }
                        if (col == 68)
                        {
                            itemtype = Item_finish;
                        }
                    }
                    //if we found an item
				    if (itemtype != 0)
                    {
                        Itemnb += 1;
                        Items[Itemnb - 1] = ItemNew(i * 8 + 3.5, j * 8 + 3.5, itemtype);

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
            // FindReplaceItemsZone(0,0,128,16);
            // FindReplaceItemsZone(32,16,64,8);
        }

        // display the level
        private void DrawMap(int flags)
        {
            // here is the list of zones
            // that make the level
            for (int i = 0; i < Levels[Currentlevel - 1].Zonenb; i++)
            {
                ZoneClass curzone = Levels[Currentlevel - 1].Zones[i];
                p8.Map(curzone.Startx, curzone.Starty, curzone.Startx * 8, curzone.Starty * 8, curzone.Sizex, curzone.Sizey, flags);
            }
            // p8.Map(0,0,0,0,128,16,flags);
            // p8.Map(32,16,32*8,16*8,64,8,flags);
        }

        // reset player state
	    // after a retry
	    private void ResetPlayer()
        {
            Entities[Playeridx - 1].X = Last_check_x;
            Entities[Playeridx - 1].Y = Last_check_y;
            Entities[Playeridx - 1].Vx = 0;
            Entities[Playeridx - 1].Vy = 0;
            Entities[Playeridx - 1].Vrot = 0;

            Entities[Playeridx + 1 - 1].X = Last_check_x + 8;
            Entities[Playeridx + 1 - 1].Y = Last_check_y;
            Entities[Playeridx + 1 - 1].Vx = 0;
            Entities[Playeridx + 1 - 1].Vy = 0;
            Entities[Playeridx + 1 - 1].Vrot = 0;

            // Camoffx = 0; Camoffy = -64;
            // Goalcamx = 0; Goalcamy = -64;

            // Bikefaceright = true;
            Isdead = false;

	    	if (Isfinish)
            {
                Restartafterfinish = true;
            }
	    	if (!Isfinish)
            {
                Retries += 1;
            }
        }

        private void ResetCamera()
        {
            Camoffx = Last_check_x - 16;  // -64
            Camoffy = Last_check_y - 64;  // -96
            Goalcamx = Camoffx;
            Goalcamy = Camoffy;
        }

        // create the 2 wheels
	    // and init some variables
	    private void CreateEntities()
        {
            Entities[1 - 1] = EntityNew(0, 0);
            Entities[2 - 1] = EntityNew(0 + 8, 0);
            Entities[1 - 1].Link = Link1;
            Entities[1 - 1].Linkside = 1;
            Entities[2 - 1].Link = Link1;
            Entities[2 - 1].Linkside = -1;
        }

        // get the value of sdf
        // at location lx,ly
        // according to a sprite
        // chosen at an offset ox,oy
        private int GetSdf(double lx, double ly, int ox, int oy)
        {
            int sx = (int)Math.Floor((lx + ox) / 8.0);
            int sy = (int)Math.Floor((ly + oy) / 8.0);

            // get the sprite at the offset
            int col = p8.Mget((lx + ox) / 8.0, ((ly + oy) / 8.0) - 1);
            int flags = p8.Fget(col);
            int isc = p8.Band(flags, 1);

	    	// check if its a colision
	    	if (isc == 0)
            {
                return 0;
            }

            // check if its in the level zone
            bool inlevelzone = false;
	    	for (int i = 0; i < Levels[Currentlevel-1].Zonenb; i++)
            {
                ZoneClass curzone = Levels[Currentlevel - 1].Zones[i];
	    		if ((sx >= curzone.Startx) && (sx < (curzone.Startx+curzone.Sizex)))
                {
                    if ((sy >= curzone.Starty) && (sy < (curzone.Starty+curzone.Sizey)))
                    {
                        inlevelzone = true;
                        break;
                    }
                }
            }
	    	if (!inlevelzone)
            {
                return 0;
            }

            // get the colision profile
            int? sdfval = Sdflink[col - 1];
	    	// if none is found, use the full square
	    	sdfval ??= 0;

            // proper coordinates in sdf
            double wx = 2 * 8 * p8.Mod((int)sdfval, 8) + lx - sx * 8 + 4;
            double wy = 2 * 8 * (int)Math.Floor((int)sdfval / 8.0) + 8 * 12 + ly - sy * 8 + 4;
            // get distance
            int dist = p8.Sget(wx, wy);

            return dist;
        }

        // get the combined sdf
	    // of the 4 closest cells
	    private double IsPointcol(double lx, double ly)
        {
            double v0 = GetSdf(lx, ly, -3, -3);
            double v1 = GetSdf(lx, ly, 4, -3);
            double v2 = GetSdf(lx, ly, 4, 4);
            double v3 = GetSdf(lx, ly, -3, 4);

            return Math.Max(Math.Max(v0, v1), Math.Max(v2, v3));
        }

        // get the colision distance
	    // and surface normal
	    private (double, double, double) IsColiding(double lx, double ly)
        {
            // we take the 4 points
            // at the center of the wheel
            double v0 = IsPointcol(lx - 0.5, ly - 0.5);
            double v1 = IsPointcol(lx + 0.5, ly - 0.5);
            double v2 = IsPointcol(lx + 0.5, ly + 0.5);
            double v3 = IsPointcol(lx - 0.5, ly + 0.5);

            // we iterpolate the distance
            // with bilinear
            double llx = lx - 0.5 - (int)Math.Floor(lx - 0.5);
            double lly = ly - 0.5 - (int)Math.Floor(ly - 0.5);
            double lerp1 = (1.0 - llx) * v0 + llx * v1;
            double lerp2 = (1.0 - llx) * v3 + llx * v2;
            double final = (1.0 - lly) * lerp1 + lly * lerp2;

            // the normal is a gradient
            double norx = (v0 - v1 + (v3 - v2)) * 0.5; // added brackets idk if correct
            double nory = (v0 - v3 + (v1 - v2)) * 0.5; // added brackets idk if correct

            // we ensure normal is normalized
            double len = Math.Sqrt(norx * norx + nory * nory + 0.001);
            norx /= len;
            nory /= len;

	    	// double final = IsPointcol(lx,ly);

	    	return (final, norx, nory);
        }

        // this take a velocity vector
	    // and reflect it by a normal
	    // a damping is applyed of the reflection
	    private (double, double) Reflect(double vx, double vy, double nx, double ny)
        {
            double dot = vx * nx + vy * ny;
            double bx = dot * nx;
            double by = dot * ny;

            double rx = vx - Str_reflect * bx;
            double ry = vy - Str_reflect * by;

	    	// we play some colision sounds
	    	// when both vector are opposite
	    	if (dot < -0.8)
            {
                p8.Sfx(0, 3);
            }
	    	else
            {
                if (dot < -0.2)
                {
                    p8.Sfx(6, 3);
                }
            }

            return (rx, ry);
        }

        // this update the state of a link
	    // between 2 wheels
	    private void UpLink(LinkClass link)
        {
            double dirx = Entities[link.Ent2 - 1].X - Entities[link.Ent1 - 1].X;
            double diry = Entities[link.Ent2 - 1].Y - Entities[link.Ent1 - 1].Y;

            link.Length = Math.Sqrt(dirx * dirx + diry * diry + 0.01);
            link.Dirx = dirx / link.Length;
            link.Diry = diry / link.Length;
        }

        // pre physic update of a wheel
	    private void UpStartEntity(EntityClass ent)
        {
            // apply gravity
            ent.Vy += Str_gravity;
            ent.Isflying = true;
        }

        // do one step of physic on a wheel
	    private void UpStepEntity(EntityClass ent)
        {
            // apply link force
	    	if (ent.Link != null)
            {
                // force according to base length
                double flink = (ent.Link.Length - ent.Link.Baselen) * Str_link;

                // add the force
                ent.Vx += ent.Link.Dirx * ent.Linkside * flink;
                ent.Vy += ent.Link.Diry * ent.Linkside * flink;

	    		// apply the rotation
	    		// due to the body
	    		// if not on the ground ?
	    		// if(ent.isflying) then
	    		if (true)
                {
                    // force perpendicular
                    // to the link axis
                    double perpx = ent.Link.Diry;
                    double perpy = -ent.Link.Dirx;

                    ent.Vx += perpx * Bodyrot / Stepnb * ent.Linkside;
                    ent.Vy += perpy * Bodyrot / Stepnb * ent.Linkside;
                }
            }

            // we test if the new location
            // is coliding
            double x2 = ent.X + ent.Vx / Stepnb;
            double y2 = ent.Y + ent.Vy / Stepnb;

            (double iscol, double norx, double nory) = IsColiding(x2, y2); // should all be 0

	    	// if coliding
	    	if (iscol > Limit_col)
            {
                // debug data
                // ent.Lastcolx = ent.X
                // ent.Lastcoly = ent.Y
                // ent.Lastcolnx = Norx
                // ent.Lastcolny = Nory

                // reflect the velocity by
                // the surface normal
                (ent.Vx, ent.Vy) = Reflect(ent.Vx, ent.Vy, norx, nory);

                // ensure we are not inside the colision
                ent.X += norx * (iscol - Limit_col);
                ent.Y += nory * (iscol - Limit_col);
            }

            // apply the motion
            ent.X += ent.Vx / Stepnb;
            ent.Y += ent.Vy / Stepnb;

	    	// if wheel is near the ground
	    	// we apply the wheel force
	    	if (iscol > Limit_wheel)
            {
                // force direction
                // perpendicular to the
                // surface normal
                double perpx = nory;
                double perpy = -norx;

                double angfac = 3.1415 * 8 * Str_wheel_size;
                // transform wheel speed to force
                double angrot = ent.Vrot * angfac;
                double wantx = angrot * perpx;
                double wanty = angrot * perpy;

                double distfactor = 1.0;  // Saturate((iscol - Limit_wheel)*1.0)

                // interpolate between
                // wheel motion
                // and entity motion
                double lerpx = Lerp(ent.Vx, wantx, Str_wheel * distfactor);
                double lerpy = Lerp(ent.Vy, wanty, Str_wheel * distfactor);

                ent.Vx = lerpx;
                ent.Vy = lerpy;

                // get the wheel speed along the surface
                double dotperp = (ent.Vx * perpx + ent.Vy * perpy);

                // the new wheel rotation is
                // the speed along the surface
                ent.Vrot = dotperp / angfac;

                // the wheel touch the ground
                ent.Isflying = false;
            }
        }

        // post physic update of a wheel
	    private void UpEndEntity(EntityClass ent)
        {
            // apply air friction
	    	if (!ent.Isflying)
            {
                ent.Vx *= Str_air;
                ent.Vy *= Str_air;
            }

            // make the wheel turn
            ent.Rot += ent.Vrot;
	    	// we could apply a wheel friction
	    	// ent.Vrot *= 0.94
        }

        // check if an item
	    // is near the player
	    private void CheckItem(ItemClass it)
        {
            // need to be carefull
            // with squaring because of overflow
            // so we first divide by the item size
            // before squaring
            double madx = (it.X - Charx) / it.Size;
            double mady = (it.Y - Chary) / it.Size;
            double sqrlen = madx * madx + mady * mady;

	    	// if colision with an item
	    	if ((!Isdead) && (sqrlen < 1))
            {
                // apples
	    		if ((it.Type == Item_apple) && it.Active)
                {
                    if (!Restartafterfinish)
                    {
                        it.Active = false;
                        Score += 1;
	    				if (Isfinish)
                        {
                            Totalscore += 1;  // special case
                        }
                        p8.Sfx(3, 3);
                    }
                }
	    		// teleports
	    		if ((it.Type == Item_teleport) && it.Active)
                {
                    if ((!Isfinish) &&(!Isdead))
                    {
                        p8.Sfx(7, 2);
                        Retries -= 1;  // free retry
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
                            Totalleveldone += 1;
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
            Dbg_curcheckcount = 1;
            Dbg_checkfound = false;
	    	foreach (ItemClass i in Items)
            {
                LoopNextCheckpoint(i);
            }
	    	if (Dbg_checkfound)
            {
                Dbg_lastcheckidx += 1;
                Retries -= 1;
                ResetPlayer();
            }
	    	else
            {
                Dbg_lastcheckidx = 0;
            }
        }

        private void LoopNextCheckpoint(ItemClass it)
        {
		    if (it.Type == Item_checkpoint)
            {
                if (Dbg_curcheckcount == Dbg_lastcheckidx + 1)
                {
                    it.Active = true;
                    Last_check_x = it.X;
                    Last_check_y = it.Y;
                    Dbg_checkfound = true;
                }
                Dbg_curcheckcount += 1;
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
                Timernextlevel += 1;
            }
            Bodyrot = 0.0;

	    	// player control
	    	if ((!Isdead) && (!Isfinish))
            {
                // flip button (c)
	    		if (p8.Btnp(4))
                {
                    Bikefaceright = !Bikefaceright;
                    p8.Sfx(8, 3);
                }
                double controlwheel = Playeridx;
                double otherwheel = Playeridx + 1;
                double wheelside = 1.0;
	    		// invert all values if bike face left
	    		if (!Bikefaceright)
                {
                    (controlwheel, otherwheel) = (otherwheel, controlwheel);
                    wheelside = -1.0;
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
                    Entities[Playeridx - 1].Vrot = Lerp(Entities[(int)Math.Floor(controlwheel) - 1].Vrot, -Base_speedfront * wheelside, Base_speedlerp);
                    // Entities[Playeridx - 1].Vrot = Lerp(Entities[(int)Math.Floor(otherwheel) - 1].Vrot, -Base_speedfront * wheelside, Base_speedlerp);
                    Bikeframe -= Base_frameadvfront;
                }
	    		// button down
	    		if (p8.Btn(3))
                {
                    // both wheels are slowed
                    Entities[Playeridx - 1].Vrot = Lerp(Entities[(int)Math.Floor(controlwheel) - 1].Vrot, Base_speedback * wheelside, Base_speedlerp);
                    Entities[Playeridx - 1].Vrot = Lerp(Entities[(int)Math.Floor(otherwheel) - 1].Vrot, Base_speedback * wheelside, Base_speedlerp);
                    Bikeframe += Base_frameadvback;
                }
            }

            // update the physics
	    	// using several substep
	    	// to improve colision
	    	foreach (EntityClass i in Entities)
            {
                UpStartEntity(i);
            }
	    	for (int i = 0; i <= Stepnb - 1; i++)
            {
                // update links
                UpLink(Link1);
	    		// update wheels
	    		foreach (EntityClass j in Entities)
                {
                    UpStepEntity(j);
                }
            }
	    	foreach (EntityClass i in Entities)
            {
                UpEndEntity(i);
            }

            bool isdown = false;

	    	// compute the body location
	    	// according to the 2 wheels
	    	// this is the upper body
	    	(Charx, Chary, Chardown) = GetBikeRot(Entities[1 - 1], Entities[2 - 1], 4.0);
	    	// this is the lower body
	    	(Charx2, Chary2, isdown) = GetBikeRot(Entities[1 - 1], Entities[2 - 1], 1.0);

            // make upper body a bit closer
            // to the lower body
            Charx += (Charx2 - Charx) * 0.5;
            
	    	// check the upper body colision
	    	(double coldist, double colnx, double colny) = IsColiding(Charx, Chary);
	    	if (coldist > 1.8)
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
	    	foreach (ItemClass i in Items)
            {
                if (i != null)
                {
                    CheckItem(i);
                }
            }

            bool needkillplayer = false;
            // check the killing floor
            if (Entities[Playeridx - 1].Y > Levels[Currentlevel - 1].Zkill)
            {
                needkillplayer = true;
            }
            // check the killing camera limit
            if (Entities[Playeridx - 1].X > Levels[Currentlevel - 1].Cammaxx + 128)
            {
                needkillplayer = true;
            }
            if (Entities[Playeridx - 1].X < Levels[Currentlevel - 1].Camminx)
            {
                needkillplayer = true;
            }
            if (Entities[Playeridx - 1].Y > Levels[Currentlevel - 1].Cammaxy + 128)
            {
                needkillplayer = true;
            }
            if (Entities[Playeridx - 1].Y < Levels[Currentlevel - 1].Camminy)
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
                Camadvanx = -32;
            }
	    	// make the camer look front
	    	// if(entities[playeridx].vx>0.8) then
	    	if (Bikefaceright)
            {
                Camadvanx = 32;
            }

            // update the camera goal
            Goalcamx = Goalcamx * 0.9 + (Entities[Playeridx - 1].X - 64 + Camadvanx) * 0.1;

            // in y there is a safe zone
            if (Camoffy > Entities[Playeridx - 1].Y - 64 + 32)
            {
                Goalcamy = Entities[Playeridx - 1].Y - 64 + 32;
            }
            if (Camoffy < Entities[Playeridx - 1].Y - 64 - 32)
            {
                Goalcamy = Entities[Playeridx - 1].Y - 64 - 32;
            }
            // or else the camera is only updated
            // when the wheel touch ground
            if (!Entities[Playeridx - 1].Isflying)
            {
                Goalcamy = Entities[Playeridx - 1].Y - 64;
            }

            // clamp the camera goal to the level limit
            Goalcamx = Math.Max(Goalcamx, Levels[Currentlevel - 1].Camminx);
            Goalcamx = Math.Min(Goalcamx, Levels[Currentlevel - 1].Cammaxx);

            Goalcamy = Math.Max(Goalcamy, Levels[Currentlevel - 1].Camminy);
            Goalcamy = Math.Min(Goalcamy, Levels[Currentlevel - 1].Cammaxy);

            // the camera location is lerped
            // Camoffx = Camoffx * 0.8 + Goalcamx * 0.2;
            // Camoffy = Camoffy * 0.7 + Goalcamy * 0.3;
            Camoffx = Lerp(Camoffx, Goalcamx, 0.2);
            Camoffy = Lerp(Camoffy, Goalcamy, 0.3);

	    	// increment the timer
	    	if (!Isfinish)
            {
                Timer += 1;
            }
        }

	    // draw a wheel entity
	    private static void DrawEntity(EntityClass ent)
        {
            int @base = 80;
            // the wheel sprite
            // depend on the wheel rotation
            double rfr = p8.Mod((int)Math.Floor(-ent.Rot * 4 * 5), 5);
	    	if (rfr < 0)
            {
                rfr += 5;
            }
            double cspr = @base + rfr;

	    	// if (Math.Abs(ent.Vrot) > 0.14)
	    	if (false)
            {
                rfr = p8.Mod((int)Math.Floor(-ent.Rot * 3), 3);
                if (rfr < 0)
                {
                    rfr += 3;
                }
                cspr = @base + 5 + rfr;
            }

	    	// to avoid the wheel appearing
	    	// to rotate backward
	    	// if the speed is too strong
	    	// we rotate slower but skip
	    	// a frame each time
	    	if (Math.Abs(ent.Vrot) > 0.14)
            {
                rfr = p8.Mod((int)Math.Floor(-ent.Rot * 3), 5);
	    		if (rfr < 0)
                {
                    rfr += 5;
                }
                rfr *= 2;
	    		if (rfr > 5)
                {
                    rfr -= 5;
                }
                cspr = @base + rfr;
            }
            p8.Spr(cspr, ent.X - 3.5, ent.Y - 3.5, 1, 1);

            // p8.Line(ent.Lastcolx, ent.Lastcoly, ent.Lastcolx + ent.Lastcolnx * 15, ent.Lastcoly + ent.Lastcolny * 15, 8);
            // p8.Line(ent.X, ent.Y, ent.X + ent.Vx * 15, ent.Y + ent.Vy * 15, 11);

	    	// p8.Circ(ent.Lastcolx, ent.Lastcoly, 3, 8);
        }

	    // take 2 wheel and give
	    // a point between
	    // with an perpendicular offset
	    private static (double, double, bool) GetBikeRot(EntityClass ent1, EntityClass ent2, double offset)
        {
            double dirx = ent2.X - ent1.X;
            double diry = ent2.Y - ent1.Y;

            // average to get the center
            double centx = ent1.X + dirx * 0.5;
            double centy = ent1.Y + diry * 0.5;

            // normalize the direction
            double length = Math.Sqrt(dirx * dirx + diry * diry + 0.01);
            dirx /= length;
            diry /= length;

            // get the perpendicular
            double perpx = diry;
            double perpy = -dirx;

            // offset the point
            // along the perpendicular
            centx += perpx * offset;
            centy += perpy * offset;

            // we want to know
            // is the point is below the bike
            bool isdown = false;
	    	if (perpy > 0.5)
            {
                isdown = true;
            }
            return (centx, centy, isdown);
        }

        private static void CenterText(int posx, int posy, string text, double col)
        {
            int sposx = posx - text.Length * 2;
            int sposy = posy;
            p8.Print(text, sposx + 1, sposy, 0);
            p8.Print(text, sposx - 1, sposy, 0);
            p8.Print(text, sposx, sposy + 1, 0);
            p8.Print(text, sposx, sposy - 1, 0);
            p8.Print(text, sposx, sposy, col);
        }

        // draw an item icon (apple, checkpoint)
	    private void DrawItem(ItemClass it)
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
                double sprite = 56;

	    		if (it.Type == Item_teleport)
                {
                    sprite = 103 + p8.Mod(Flaganim, 3);
                }

	    		if ((it.Type == Item_checkpoint) || (it.Type == Item_finish))
                {
                    sprite = 64 + p8.Mod(Flaganim, 3);

                    // change the flag pole color
                    int flagcolor = 12;
	    			if (it.Active)
                    {
                        if (it.Type == Item_finish)
                        {
                            flagcolor = 11;
                        }
	    				else
                        {
                            flagcolor = 8;
                        }
                    }
                    p8.Line(it.X - 1, it.Y - 3, it.X - 1, it.Y + 12, flagcolor);
                }

                p8.Spr(sprite, it.X - 3.5, it.Y - 3.5, 1, 1);

	    		// p8.Line(it.X, it.Y, Charx, Chary, 12);
            }
        }

	    // draw the introduction and victory big flag
	    private void DrawBigFlag(string text, int finishx, int finishy, int col)
        {
            // p8.Line(finishx - 1, finishy, finishx - 1, finishy + 15, 8);
            // p8.Line(finishx + 64, finishy, finishx + 64, finishy + 15, 8);
            // p8.Line(finishx - 1, finishy - 1, finishx + 64, finishy - 1, 8);
            // p8.Line(finishx - 1, finishy + 16, finishx + 64, finishy + 16, 8);
            p8.Rectfill(finishx, finishy, finishx + 63, finishy + 15, 0);
	    	for (int i = 0; i <= 31; i++)
            {
                double tmpx = finishx + p8.Mod(i, 16) * 4;
                double tmpy = finishy + (1 - p8.Mod(i, 2)) * 4 + (int)Math.Floor(i / 16.0) * 8;
                p8.Rectfill(tmpx, tmpy, tmpx + 3, tmpy + 3, 6);
            }
            CenterText(finishx + 32, finishy + 6, text, col);

            // p8.Pal(7, 10);

            double sprite = 64 + p8.Mod((int)Math.Floor(Flaganim * 0.7), 3);
            // p8.Spr(sprite, finishx - 6, finishy, 1, 1, true);
            // p8.Spr(sprite, finishx - 2 + 64, finishy, 1, 1, false);
            p8.Sspr((sprite - 64) * 8, 4 * 8, 8, 8, finishx - 20, finishy - 4, 32, 32, true);
            p8.Sspr((sprite - 64) * 8, 4 * 8, 8, 8, finishx - 16 + 64, finishy - 4, 32, 32, false);

	    	// p8.Pal();
        }

	    private static string GetTimeStr(int val)
        {
            // transform timer to min:sec:dec
            double t_cent = p8.Mod((int)Math.Floor(val * 10 / 30.0), 10);
            double t_sec = p8.Mod((int)Math.Floor(val / 30.0), 60);
            int t_min = (int)Math.Floor(val / (30.0 * 60.0));

            string fill_sec = "";
		    if (t_sec < 10)
            {
                fill_sec = "0";
            }

            return $"{t_min}:{fill_sec}{t_sec}:{t_cent}";
        }

        private static double Clampy(double v)
        {
            return v; // return max(0,min(128,v))
        }

        private static (double, double) Swap(double x1, double x2)
        {
            return (x2, x1);
        }
		
        private void Rectlight(double x, int y, double sx)
        {
            double mx = Math.Min(x, sx);
            double ex = Math.Max(x, sx);
		    for (int i = (int)Math.Floor(mx); i <= ex; i++)
            {
                p8.Pset(i, y, Pal[p8.Pget(i, y) + 1 - 1]);
            }
        }
		
        private void Otri(double x1, double y1, double x2, double y2, double x3, double y3)
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

            y1 += 0.001;

            double miny = Math.Min(y2, y3);
            double maxy = Math.Max(y2, y3);

            double fx = x2;
            if (y2 < y3)
            {
                fx = x3;
            }

            double cl_y1 = (Clampy(y1));
            double cl_miny = (Clampy(miny));
            double cl_maxy = (Clampy(maxy));

            double steps = (x3 - x1) / (y3 - y1);
            double stepe = (x2 - x1) / (y2 - y1);

            double sx = steps * (cl_y1 - y1) + x1;
            double ex = stepe * (cl_y1 - y1) + x1;
            
            for (int y = (int)Math.Floor(cl_y1); y <= cl_miny; y++)
            {
                Rectlight(sx, y, ex);
                sx += steps;
                ex += stepe;
            }

            sx = steps * (miny - y1) + x1;
            ex = stepe * (miny - y1) + x1;

            double df = 1 / (maxy - miny);

            double step2s = (fx - sx) * df;
            double step2e = (fx - ex) * df;

            double sx2 = sx + step2s * (cl_miny - miny);
            double ex2 = ex + step2e * (cl_miny - miny);
            
            for (int y = (int)Math.Floor(cl_miny); y <= cl_maxy; y++)
            {
                Rectlight(sx2, y, ex2);
                sx2 += step2s;
                ex2 += step2e;
            }
        }

        private void Lamp(double lampx, double lampy, double lampdirx, double lampdiry, double lampperpx, double lampperpy, int sidefac, int lamplen, int lampwid)
        {
            double lampp1x = lampx + lampdirx * lamplen * sidefac + lampperpx * lampwid;
            double lampp1y = lampy + lampdiry * lamplen * sidefac + lampperpy * lampwid;
            double lampp2x = lampx + lampdirx * lamplen * sidefac - lampperpx * lampwid;
            double lampp2y = lampy + lampdiry * lamplen * sidefac - lampperpy * lampwid;
            Otri((int)Math.Floor(lampx), (int)Math.Floor(lampy), (int)Math.Floor(lampp1x), (int)Math.Floor(lampp1y), (int)Math.Floor(lampp2x), (int)Math.Floor(lampp2y));
        }

        // main draw function
        public void Draw()
        {
            p8.Cls();

            p8.Camera(0, 0);

            // black will not be translucent
            // dark green will be
            p8.Palt(0, false);
            p8.Palt(3, true);
            p8.Palt(4, false);

	    	// start menu
	    	if (!Isstarted)
            {
                p8.Rectfill(0, 0, 127, 127, 1);

                int c = 16;

                CenterText(64, c, "nusan present", 5);
                DrawBigFlag("cyclo 8", 32, c + 12, 10);

                c = 58;
                CenterText(32, c, "up = gas", 7);
                CenterText(32, c + 8, "down = brake", 7);
                CenterText(64, c + 16, "left-right = rotate the bike", 7);
                CenterText(96, c, "c = flip bike", 7);
                CenterText(96, c + 8, "v = retry", 7);

                double flipcol = 6 + p8.Mod((int)Math.Floor(Flaganim * 0.5), 2);

                c = 94;
                CenterText(64, c, "starting level :", 7);
                CenterText(64, c + 8, $"< {Currentlevel} - {Levels[Currentlevel - 1].Name} >", 8);
                CenterText(64, c + 18, "press c to start", flipcol);

                Flaganim += 0.2;

                return;
            }

            // background color
            p8.Rectfill(0, 0, 127, 127, 4);

            p8.Camera(Camoffx, Camoffy);

            int treeoff = Levels[Currentlevel - 1].Backy;

            // draw the cloud in background :
            double paral2x = (Camoffx) * 0.75;
            double paral2y = (Camoffy - treeoff) * 0.75 + treeoff;

            int i = 1;
	    	while (i <= 30)
            {
                p8.Circfill(paral2x + i * 20 + Cloudsx[i - 1], paral2y + 45 + Cloudsy[i - 1], 10 + Cloudss[i - 1], 5);
                i += 1;
            }

            i = 1;
	    	while (i <= 30)
            {
                p8.Circfill(paral2x + i * 20 + Cloudsx[i - 1], paral2y + 62 + Cloudsy[i - 1], 10 + Cloudss[i - 1], 4);
                i += 1;
            }

            // draw the trees :
            double paralx = (Camoffx) * 0.5;
            double paraly = (Camoffy - treeoff) * 0.5 + treeoff;
            p8.Palt(3, false);
            p8.Palt(4, true);
            // draw the bottom of the trees
            p8.Rectfill(Camoffx, paraly + 64 + 8, Camoffx + 128, Camoffy + 128, 2);

            paralx = p8.Mod(paralx, 128) + (int)Math.Floor(paralx / 128) * 256;
            // draw 2 series of trees
            // warping infinitly
            p8.Map(112, 40, paralx, paraly + 16, 16, 8);
            p8.Map(112, 40, paralx + 128, paraly + 16, 16, 8);
            p8.Palt(3, true);
            p8.Palt(4, false);

            // draw the bottom line
            // in black to mask bottom
            // of the level
            p8.Rectfill(Camoffx, 108 + treeoff, Camoffx + 128, 110 + treeoff, 12);
            p8.Rectfill(Camoffx, 109 + treeoff, Camoffx + 128, Camoffy + treeoff + 128, 0);

            // draw_col()

            // draw the all level
            DrawMap(0);

	    	foreach (ItemClass j in Items)
            {
                if (j != null)
                {
                    DrawItem(j);
                }
            }
	    	
	    	// draw_entity(entities[1])
	    	// draw_entity(entities[2])
	    	foreach (EntityClass j in Entities)
            {
                DrawEntity(j);
            }

            // draw the player :
            double cspr = p8.Mod((int)Math.Floor(-Bikeframe), 4);
	    	if (cspr < 0)
            {
                cspr += 4;
            }
	    	if (Isdead)
            {
                cspr = 4;
            }

            int bodyadv = 0;
	    	if (Bodyrot > 0)
            {
                bodyadv = 1;
            }
	    	if (Bodyrot < 0)
            {
                bodyadv = -1;
            }

            double cspr2 = cspr + 16;

	    	if (Chardown)
            {
                cspr = 5;
	    		if (Isdead)
                {
                    cspr = 6;
                }
            }

            // player lower body
            p8.Spr(96 + cspr2, Charx2 - 3.5, Chary2 - 4.5, 1, 1, !Bikefaceright);
            // player upper body
            p8.Spr(96 + cspr, Charx - 3.5 + bodyadv * 2, Chary - 6, 1, 1, !Bikefaceright);

            int wheelidx = 1;
            int sidefac = -1;
	    	if (Bikefaceright)
            {
                wheelidx = 2;
                sidefac = 1;
            }

            double lampdirx = Entities[Playeridx - 1].Link.Dirx;
            double lampdiry = Entities[Playeridx - 1].Link.Diry;
            double lampperpx = lampdiry;
            double lampperpy = -lampdirx;
            double lampx = Entities[wheelidx - 1].X + lampperpx * 4 + lampdirx * sidefac * 2;
            double lampy = Entities[wheelidx - 1].Y + lampperpy * 4 + lampdiry * sidefac * 2;

            Lamp(lampx, lampy, lampdirx, lampdiry, lampperpx, lampperpy, sidefac, 20, 10);
            // Lamp(lampx, lampy, lampdirx, lampdiry, lampperpx, lampperpy, sidefac, 10, 5);
            // Lamp(lampx, lampy, lampdirx, lampdiry, lampperpx, lampperpy, sidefac, 5, 2);
            p8.Circ((int)Math.Floor(lampx), (int)Math.Floor(lampy), 1, 1);
            p8.Pset((int)Math.Floor(lampx), (int)Math.Floor(lampy), 7);

            // draw the foreground part of the level
            DrawMap(~0x2);

            // Otri((int)Math.Floor(lampx), (int)Math.Floor(lampy), (int)Math.Floor(lampp1x), (int)Math.Floor(lampp1y), (int)Math.Floor(lampp2x), (int)Math.Floor(lampp2y), 10);

            // p8.Circfill(Charx, Chary, 1, 11);

            // DrawCol();

            p8.Camera(0, 0);

	    	// display hud
	    	if (true)
            {
                if (Timerlasteleport < 30)
                {
                    if (p8.Mod(Timerlasteleport, 4) < 2)
                    {
                        CenterText(64, 64, "teleport", 12);
                    }
                    Timerlasteleport += 1;
                }
	    		
                // handle going to the next level
                if (Isfinish)
                {
                    double progress = Saturate((Timernextlevel / Timernextlevel_dur - 0.3) / 0.5);
                    p8.Rectfill(-1, 0, 128 * progress - 1, 128, 1);

                    if (progress > 0.9)
                    {
                        if (Currentlevel >= Levelnb)
                        {
                            int c = 36;
                            CenterText(64, c, "nusan present", 5);
                            DrawBigFlag("cyclo 8", 32, c + 12, 10);
                            CenterText(66, 72, "thanks for playing", 7);
                        }
                        else
                        {
                            CenterText(64, 64, "next level :", 7);
                            CenterText(64, 74, $"{Currentlevel + 1} - {Levels[Currentlevel + 1 - 1].Name}", 7);
                        }
                    }
                    CenterText(64, 14, $"total over {Totalleveldone} levels", 12);
                    CenterText(22, 22, $"retries:{Totalretries}", 12);
                    CenterText(64, 24, $"score:{Totalscore}", 12);
                    CenterText(112, 22, $"{GetTimeStr(Totaltimer)}", 12);
                }
                CenterText(22, 4, $"retries:{Retries}", 8);
                CenterText(64, 2, $"score:{Score}", 8);
                CenterText(112, 4, $"{GetTimeStr(Timer)}", 8);

                if (Isdead && (!Isfinish))
                {
                    CenterText(64, 20, "you are dead", 8);
                    CenterText(64, 28, "press v to retry", 8);
                }
                if (Isfinish)
                {
                    DrawBigFlag("victory", 32, 90, 8);
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
                p8.Print($"cpu {p8.Stat(1)}", 96, 112, 7);
            }
            Flaganim += 0.2;
        }

        public char[] SpriteData => @"
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
".Replace("\n", "").Replace("\r", "").ToCharArray();

        public string FlagData => @"
0001010101010100010101010101020201010101000000010101010100000202020202020000000101010101010102020101010101010101040101010202020208080808080200000000000000000000000000000000000000000000000000000000000000000004040400000000000000000000000000000000000000000000
0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
".Replace("\n", "").Replace("\r", "");

        public string MapData => @"
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
".Replace("\n", "").Replace("\r", "");

    }

}
