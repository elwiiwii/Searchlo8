using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Searchlo8
{
    public class Cyclo8
    {
        #region globals
        private static Pico8 p8;
        private static Cyclo8 g;
        private static Random random = new();

        private double Base_frameadvback;
        private double Base_frameadvfront;
        private double Base_speedback;
        private double Base_speedfront;
        private double Base_speedlerp;
        private bool Bikefaceright;
        private int Bikeframe;
        private double Bodyrot;
        private int Camadvanx;
        private int Camoffx;
        private int Camoffy;
        private bool Chardown;
        private int Charx;
        private int Charx2;
        private int Chary;
        private int Chary2;
        private List<double> Cloudss;
        private List<double> Cloudsx;
        private List<double> Cloudsy;
        private int Currentlevel;
        private bool Dbg_checkfound;
        private int Dbg_curcheckcount;
        private int Dbg_lastcheckidx;
        private List<EntityClass> Entity;
        private List<EntityClass> Entities;
        private int Flaganim;
        private int Goalcamx;
        private int Goalcamy;
        private bool Has_check;
        private bool Isdead;
        private bool Isfinish;
        private bool Isstarted;
        private List<ItemClass> Item;
        private int Item_apple;
        private int Item_checkpoint;
        private int Item_finish;
        private int Item_start;
        private int Item_teleport;
        private int Itemnb;
        private List<ItemClass> Items;
        private int Last_check_x;
        private int Last_check_y;
        private List<LevelClass> Level;
        private int Levelnb;
        private List<LevelClass> Levels;
        private double Limit_col;
        private double Limit_wheel;
        private int Link;
        private LinkClass Link1;
        private int Playeridx;
        private bool Restartafterfinish;
        private int Retries;
        private int Score;
        private List<int> Sdflink;
        private int Stepnb;
        private double Str_air;
        private double Str_bodyrot;
        private double Str_gravity;
        private double Str_link;
        private double Str_reflect;
        private double Str_wheel;
        private double Str_wheel_size;
        private int Timer;
        private int Timerlasteleport;
        private int Timernextlevel;
        private int Timernextlevel_dur;
        private int Totalleveldone;
        private int Totalretries;
        private int Totalscore;
        private int Totaltimer;
        private List<ZoneClass> Zone;
        #endregion
        #region constructor
        private Cyclo8(Pico8 pico8)
        {
            p8 = pico8;
            g = this;

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
            Has_check = false;

            Dbg_curcheckcount = 1;
            Dbg_checkfound = false;
            Dbg_lastcheckidx = 0;

            // physics settings :
            // nb of physics substeps
            Stepnb = 10;
            // strengh of rebound
            Str_reflect = 1.1d;
            Str_gravity = 0.06d;
            Str_air = 0.99d;
            Str_wheel = 0.25d;
            Str_wheel_size = 1.0d;
            Str_link = 0.5d;
            // rotation of the bike
            // according to arrow keys
            Str_bodyrot = 0.04d;
            // acceleration factor
            Base_speedlerp = 0.5d;
            // max speed front
            Base_speedfront = 0.18d;
            // max speed back
            Base_speedback = 0.03d;
            Base_frameadvfront = 0.3d;
            Base_frameadvback = 0.15d;

            Limit_col = 2.0d;
            Limit_wheel = 1.5d;

            Bodyrot = 0.0d;

            Playeridx = 1;

            Currentlevel = 1;
            Levelnb = 7;

            Timernextlevel = 0;
            Timernextlevel_dur = 30 * 7;
            Timerlasteleport = 1000;

            Zone = [];
            Cloudsx = [];
            Cloudsy = [];
            Cloudss = [];
            Level = [];
            Levels = [];
            Entity = [];
            Entities = [];
            Item = [];

            Itemnb = 0;
            Item_apple = 1;
            Item_checkpoint = 2;
            Item_start = 3;
            Item_finish = 4;
            Item_teleport = 5;

            Items = new();
            Link = new();
            Link1 = LinkNew(1, 2);

            // array to link sprite to colision
            Sdflink = [];
            Sdflink[1 - 1] = 1;
            Sdflink[2 - 1] = 2;
            Sdflink[3 - 1] = 3;
            Sdflink[12 - 1] = 3;
            Sdflink[6 - 1] = 4;
            Sdflink[13 - 1] = 4;
            Sdflink[8 - 1] = 5;
            Sdflink[9 - 1] = 6;
            Sdflink[10 - 1] = 7;
            Sdflink[11 + 16 - 1] = 8;
            Sdflink[0 + 16 * 3 - 1] = 9;
            Sdflink[1 + 16 * 3 - 1] = 10;
            Sdflink[2 + 16 * 3 - 1] = 11;
            Sdflink[3 + 16 * 3 - 1] = 12;
            Sdflink[4 + 16 * 3 - 1] = 13;
            Sdflink[10 + 16 * 3 - 1] = 14;
            Sdflink[11 + 16 * 3 - 1] = 15;

        }
        #endregion

        // map zone structure.
        // level is made of several zones
        private class ZoneClass(int inStartx, int inStarty, int inSizex, int inSizey)
        {
            public int Startx = inStartx;
            public int Starty = inStarty;
            public int Sizex = inSizex;
            public int Sizey = inSizey;
        }

        private ZoneClass ZoneNew(int inStartX, int inStartY, int inSizeX, int inSizeY)
        {
            return new ZoneClass(inStartX, inStartY, inSizeX, inSizeY);
        }

        // level structure
        private class LevelClass(string inName, int inZkill, int inBacky, int inCamminx, int inCammaxx, int inCamminy, int inCammaxy)
        {
            public string Name = inName;
            public List<ZoneClass> Zones = [];
            public int Zonenb = 0;
            public int Zkill = inZkill;
            public int Backy = inBacky;
            public int Camminx = inCamminx;
            public int Cammaxx = inCammaxx;
            public int Camminy = inCamminy;
            public int Cammaxy = inCammaxy;
            public bool Startright = true;
        }
		
        private LevelClass LevelNew(string inName, int inZkill, int inBacky, int inCamminx, int inCammaxx, int inCamminy, int inCammaxy)
        {
            return new LevelClass(inName, inZkill, inBacky, inCamminx, inCammaxx, inCamminy, inCammaxy);
        }

        // entity = the 2 wheels
        private class EntityClass(int inx, int iny, bool isFinish = false)
        {
            public int X = inx;
            public int Y = iny;
            public double Vx = 0.0d;
            public double Vy = 0.0d;
            public double Rot = 0.0d;
            public double Vrot = 0.0d;
            public bool Isflying = true;
            // public int Lastcolx = X;
            // public int Lastcoly = Y;
            // public int Lastcolnx = 0;
            // public int Lastcolny = 0;
            public LinkClass link = null;
            public int linkside = 1;
        }

        private EntityClass EntityNew(int inx, int iny, bool isFinish = false)
        {
            return new EntityClass(inx, iny, isFinish);
        }

        private class ItemClass(double inx, double iny, int inType)
        {
            public double X = inx;
            public double Y = iny;
            public int Type = inType;
            public bool Active = true;
            public int Size = 8;
        }

        private ItemClass ItemNew(double inX, double inY, int inType)
        {
            return new ItemClass(inX, inY, inType);
        }

        // a physic link between wheels
        private class LinkClass(int ent1, int ent2)
        {
            public int Ent1 = ent1;
            public int Ent2 = ent2;
            public double Baselen = 8.0d;
            public double length = 8.0d;
            public double dirx = 0.0d;
            public double diry = 0.0d;
        }

        private LinkClass LinkNew(int ent1, int ent2)
        {
            return new LinkClass(ent1, ent2);
        }

        private double Lerp(int a, int b, int alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
        }

        private int Saturate(int a)
        {
            return Math.Max(0, Math.Min(1, a));
        }

        private void BlurPass(List<int> insdf, List<double> outsdf)
        {
            for (int i = 1; i <= 14; i++)
            {
                for (int j = 1; j <= 14; j++)
                {
                    int idx = i + j * 16;
                    double sum = 0;
                    double wei = 0;
                    for (int sx = -1; sx <= 1; sx++)
                    {
                        for (int sy = -1; sy <= 1; sy++)
                        {
                            double lwei = Math.Sqrt(sx * sx + sy * sy + 0.01d);
                            sum += insdf[idx + sx + sy * 16 - 1] * lwei;
                            wei += lwei;
                        }
                    }
                    outsdf[idx - 1] = sum / wei;
                }
            }
        }

        // this function is not used at runtime
        // it create a distance field in a 16x16 sprite
        // based on a 8x8 sprite colision
        private void GenSdf(int ix, int iy, int num)
        {
            int rx = 8 * ix;
            int ry = 8 * iy;

            int wx = 2 * 8 * (num % 8);
            int wy = 2 * 8 * (int)Math.Floor(num / 8.0d) + 8 * 12;

            // init to 0
            // we will ping-pong
            // beetween sdf and sdf2
            List<double> sdf = new();
            List<double> sdf2 = new();
            for (int i = 0; i <= 15; i++)
            {
                for (int j = 0; j <= 15; j++)
                {
                    sdf[i + j * 16 - 1] = 0.0d;
                }
            }

            // fill the sdf sprite with the base sprite colision
            for (int i = 0; i <= 7; i++)
            {
                for (int j = 0; j <= 7; j++)
                {
                    int sc = p8.Sget(rx + i, ry + j, 8);
                    int idx = i + 4 + (j + 4) * 16;
                    // 3 is the transparent color
                    if (sc != 3)
                    {
                        sdf[idx - 1] = 15.0d;
                    }
                    else
                    {
                        sdf[idx - 1] = 0.0d;
                    }
                    // p8.Sset(wx+i + 4, wy+j + 4, sc);
                }
            }

            // first propagation of distance field
            // along x axis
            for (int i = 0; i <= 15; i++)
            {
                for (int j = 0; j <= 15; j++)
                {
                    int idx = i + j * 16;
                    // we search the nearest colision along x
                    double mindist = 15.0d;
                    for (int s = 0; s <= 15; s++)
                    {
                        //if we find a colision on the same row
                        if (sdf[s + j * 16 - 1] >= 8.0d)
                        {
                            // set the distance
                            int curdist = Math.Abs(s - i);
                            mindist = Math.Min(mindist, curdist);
                        }
                    }
                    sdf2[idx - 1] = mindist;
                }
            }

            // second propagation of distance field
            // along y axis
            for (int i = 0; i <= 15; i++)
            {
                for (int j = 0; j <= 15; j++)
                {
                    int idx = i + j * 16;
                    // we search the nearest colision along x,y
                    double mindist = 15.0d;
                    for (int s = 0; s <= 15; s++)
                    {
                        // we compute the final distance
                        // with pythagore
                        int disty = Math.Abs(s - j);
                        double distx = sdf2[i + s * 16 - 1];
                        double curdist = Math.Sqrt(distx * distx + disty * disty + 0.001);
                        mindist = Math.Min(mindist, curdist);
                    }
                    sdf[idx - 1] = mindist;
                }
            }

            // blur_pass(sdf,sdf2)
            // blur_pass(sdf2,sdf)

            // we encode the final sdf
            // in sprites
            // we want a maximum range of 4
            // because the wheel is of radius 4
            for (int i = 0; i <= 15; i++)
            {
                for (int j = 0; j <= 15; j++)
                {
                    int idx = i + j * 16;
                    p8.Sset(wx + i, wy + j, Math.Max(0, Math.Min((int)Math.Floor(15.0 - (sdf[idx - 1] - 1) * 4.0), 15)));
                    // p8.Sset(wx+i, wy+j, Math.Max(0,Math.Min((int)Math.Floor(sdf[idx]),15)));
                }
            }

            // we save everything in the cartridge
            p8.Cstore();
        }

        private void GenAllSdf()
        {
            // here is the list of all sprites
            // that will generate a sdf
            GenSdf(0, 1, 0);
            GenSdf(1, 0, 1);
            GenSdf(2, 0, 2);
            GenSdf(3, 0, 3);
            GenSdf(6, 0, 4);
            GenSdf(8, 0, 5);
            GenSdf(9, 0, 6);
            GenSdf(10, 0, 7);
            GenSdf(11, 1, 8);
            GenSdf(0, 3, 9);
            GenSdf(1, 3, 10);
            GenSdf(2, 3, 11);
            GenSdf(3, 3, 12);
            GenSdf(4, 3, 13);
            GenSdf(10, 3, 14);
            GenSdf(11, 3, 15);
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

        private void _Init()
        {
            // uncomment the next line
            // to regenerate sdf sprite
            // GenAllSdf();

            List<int> pal = [5, 13, 15, 11, 9, 6, 7, 7, 14, 10, 7, 7, 7, 6, 15, 7];

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

            Items = new();
            Itemnb = 0;
            Has_check = false;
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

                    if ((flags & 4) > 0)
                    {
                        itemtype = Item_teleport;
                        if (col == 56)
                        {
                            itemtype = Item_apple;
                        }
                    }
                    if ((flags & 8) > 0)
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
            for (int i = 0; i <= Levels[Currentlevel - 1].Zonenb; i++)
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
            for (int i = 0; i <= Levels[Currentlevel - 1].Zonenb; i++)
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
            Entities[1 - 1].link = Link1;
            Entities[1 - 1].linkside = 1;
            Entities[2 - 1].link = Link1;
            Entities[2 - 1].linkside = -1;
        }

        // get the value of sdf
	    // at location lx,ly
	    // according to a sprite
	    // chosen at an offset ox,oy
	    private int GetSdf(int lx, int ly, int ox, int oy)
        {
            int sx = (int)Math.Floor((lx + ox) / 8.0d);
            int sy = (int)Math.Floor((ly + oy) / 8.0d);

            // get the sprite at the offset
            int col = p8.Mget((lx + ox) / 8.0d, (ly + oy) / 8.0d);
            int flags = p8.Fget(col);
            int isc = (flags & 1);

	    	// check if its a colision
	    	if (isc == 0)
            {
                return 0;
            }

            // check if its in the level zone
            bool inlevelzone = false;
	    	for (int i = 0; i <= Levels[Currentlevel-1].Zonenb; i++)
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
            int sdfval = Sdflink[col - 1];
	    	// if none is found, use the full square
	    	if (sdfval == null)
            {
                sdfval = 0;
            }

            // proper coordinates in sdf
            int wx = 2 * 8 * (sdfval % 8) + lx - sx * 8 + 4;
            int wy = 2 * 8 * (int)Math.Floor(sdfval / 8.0d) + 8 * 12 + ly - sy * 8 + 4;
            // get distance
            int dist = p8.Sget(wx, wy);

            return dist;
        }




	    
	    
	    








    }

}
