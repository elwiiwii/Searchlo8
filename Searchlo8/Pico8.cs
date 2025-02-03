using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Searchlo8
{
    public class Pico8()
    {
        private int[] Map1 = new int[128 * 64];
        private (int, int) CameraOffset = (0, 0);
        private char[] spriteSheet1 = new char[128 * 128];


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            return false;
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            return false;
        }


        public void Camera(double x = 0, double y = 0) // https://pico-8.fandom.com/wiki/Camera
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);

            CameraOffset = (xFlr, yFlr);
        }


        public void Circ(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);

            for (int i = xFlr - rFlr; i <= xFlr + rFlr; i++)
            {
                for (int j = yFlr - rFlr; j <= yFlr + rFlr; j++)
                {
                    // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                    double offsetX = (i < xFlr) ? 0.35D : -0.35D;
                    double offsetY = (j < yFlr) ? 0.35D : -0.35D;
                    double gridCenterX = i + offsetX;
                    double gridCenterY = j + offsetY;

                    bool isCurrentInCircle = Math.Pow(gridCenterX - xFlr, 2) + Math.Pow(gridCenterY - yFlr, 2) <= rFlr * rFlr;

                    // Check all four adjacent grid spaces
                    bool isRightOutsideCircle = Math.Pow(i + 1 + offsetX - xFlr, 2) + Math.Pow(j + offsetY - yFlr, 2) > rFlr * rFlr;
                    bool isLeftOutsideCircle = Math.Pow(i - 1 + offsetX - xFlr, 2) + Math.Pow(j + offsetY - yFlr, 2) > rFlr * rFlr;
                    bool isUpOutsideCircle = Math.Pow(i + offsetX - xFlr, 2) + Math.Pow(j + 1 + offsetY - yFlr, 2) > rFlr * rFlr;
                    bool isDownOutsideCircle = Math.Pow(i + offsetX - xFlr, 2) + Math.Pow(j - 1 + offsetY - yFlr, 2) > rFlr * rFlr;

                    if (isCurrentInCircle && (isRightOutsideCircle || isLeftOutsideCircle || isUpOutsideCircle || isDownOutsideCircle))
                    {
                        
                    }
                }
            }
        }


        public void Circfill(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);

            for (int i = (xFlr - rFlr); i <= xFlr + rFlr; i++)
            {
                for (int j = (yFlr - rFlr); j <= yFlr + rFlr; j++)
                {
                    // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                    double offsetX = (i < xFlr) ? 0.35D : -0.35D;
                    double offsetY = (j < yFlr) ? 0.35D : -0.35D;
                    double gridCenterX = i + offsetX;
                    double gridCenterY = j + offsetY;

                    if (Math.Pow(gridCenterX - xFlr, 2) + Math.Pow(gridCenterY - yFlr, 2) <= rFlr * rFlr)
                    {
                        
                    }
                }
            }
        }


        public void Cls(double color = 0) // https://pico-8.fandom.com/wiki/Cls
        {
            int colorFlr = (int)Math.Floor(color);
        }


        public double Cos(double angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Cos
        {
            return Math.Cos(-angle * 2 * Math.PI);
        }


        public void Cstore()
        {

        }


        public void Del<T>(List<T> table, T value) // https://pico-8.fandom.com/wiki/Del
        {
            table.Remove(value);
        }


        public int Fget(int x, int y = 0)
        {
            return 1;
        }


        public void Line(double x1, double y1, double x2, double y2, int col)
        {

        }


        public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int? flags = null) // https://pico-8.fandom.com/wiki/Map
        {
            int cxFlr = (int)Math.Floor(celx);
            int cyFlr = (int)Math.Floor(cely);
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
            int cwFlr = (int)Math.Floor(celw);
            int chFlr = (int)Math.Floor(celh);

            for (int i = 0; i <= cwFlr; i++)
            {
                for (int j = 0; j <= chFlr; j++)
                {
                    Spr(Mget(i + cxFlr, j + cyFlr), sxFlr + i * 8, syFlr + j * 8);
                }
            }
        }


        public void Memcpy(int destaddr, int sourceaddr, int len) // https://pico-8.fandom.com/wiki/Memcpy - https://pico-8.fandom.com/wiki/Memory
        {
            if (destaddr == 0x1000 && sourceaddr == 0x2000 && len == 0x1000)
            {
                //var secondHalf = SpriteSheets.SpriteSheet2.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray();
                //secondHalf.CopyTo(spriteSheet1, secondHalf.Length);
            }
        }


        /*public int MgetOld(double celx, double cely)
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);

            string MapData = new(MapFile.Map1.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            char c = MapData[xFlr + (yFlr * 128)];

            int IntC = 0;

            if (c >= 48 && c <= 57)
            {
                IntC = c - '0';
            }
            else if (c >= 97 && c <= 102)
            {
                IntC = 10 + c - 'a';
            }

            return IntC;

        }*/


        public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);

            int mval = Map1[xFlr + (yFlr * 128)];

            return mval;
        }


        public double Mod(double x, double m)
        {
            double r = x % m;
            return r < 0 ? r + m : r;
        }


        public void Mset(double celx, double cely, double snum = 0) // https://pico-8.fandom.com/wiki/Mset
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);
            int sFlr = (int)Math.Floor(snum);

            Map1[xFlr + (yFlr * 128)] = sFlr;
        }


        public void Music(double n, double fadems = 0, double channelmask = 0) // https://pico-8.fandom.com/wiki/Music
        {
            int nFlr = (int)Math.Floor(n);

        }


        public void Pal() // https://pico-8.fandom.com/wiki/Pal
        {

        }


        public void Pal(double c0, double c1) // https://pico-8.fandom.com/wiki/Pal
        {
            
        }


        public void Palt() // https://pico-8.fandom.com/wiki/Palt
        {
            
        }


        public void Palt(double col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            
        }


        public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int cFlr = (int)Math.Floor(c);

            int charWidth = 4;
            //int charHeight = 5;

            for (int s = 0; s < str.Length; s++)
            {
                char letter = str[s];

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        
                    }
                }
            }
        }


        public int Pget(double x, double y)
        {
            return 1;
        }


        public void Pset(double x, double y, double c) // https://pico-8.fandom.com/wiki/Pset
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            //float yFlr = (float)(Math.Floor(y) - 0.5);
            int cFlr = (int)Math.Floor(c);

        }


        public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            int x1Flr = (int)Math.Floor(x1);
            int y1Flr = (int)Math.Floor(y1);
            int x2Flr = (int)Math.Floor(x2);
            int y2Flr = (int)Math.Floor(y2);
            int cFlr = (int)Math.Floor(c);

            //var rectEndX = (x2Flr - CameraOffset.Item1) * cellWidth;
            //var rectThickness = (y2Flr - y1Flr) * cellHeight;
            //batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[cFlr], rectThickness);

        }


        public void Reload() // https://pico-8.fandom.com/wiki/Reload
        {

        }


        public double Rnd(double lower = 1.0, double upper = 0.0) // https://pico-8.fandom.com/wiki/Rnd
        {
            Random random = new();
            if (lower < upper)
            {
                double n = random.NextDouble() * (upper - lower) + lower;
                return n;
            }
            double n2 = random.NextDouble() * (lower - upper) + upper;
            return n2;
        }


        public void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0) // https://pico-8.fandom.com/wiki/Sfx
        {
            int nFlr = (int)Math.Floor(n);
            int channelFlr = (int)Math.Floor(channel);

        }


        public double Sin(double angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Sin
        {
            return Math.Sin(-angle * 2 * Math.PI);
        }


        public void Spr(double spriteNumber, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Spr
        {
            int spriteNumberFlr = (int)Math.Floor(spriteNumber);
            int xFlr = (int)Math.Floor(x) - 8;
            int yFlr = (int)Math.Floor(y) - 8;
            int wFlr = (int)Math.Floor(w);
            int hFlr = (int)Math.Floor(h);

            var spriteWidth = 8;
            var spriteHeight = 8;

            int spriteX = spriteNumberFlr % 16 * spriteWidth;
            int spriteY = spriteNumberFlr / 16 * spriteHeight;

            int colorCache = 0;

            //for (int i = 0, j = 1; i < resetColors.Length; i++, j = 1)
            //{
            //    if (sprColors[i] != resetSprColors[i])
            //    {
            //        for (int k = 0; k < resetSprColors.Length; k++, j++)
            //        {
            //            if (sprColors[i] == resetSprColors[k])
            //            {
            //                colorCache += (j * (int)Math.Pow(10, i * 2)) * 1000;
            //                goto Continue;
            //            }
            //        }
            //    }
            //
            //    Continue:
            //
            //    var transparency = sprColors[i].A == 0 ? 1 : 2;
            //    colorCache += ((transparency * 16) * (int)Math.Pow(10, i * 2)) * 1000;
            //}

        }


        public void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw = -1, double dh = -1, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Sspr
        {
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
            int swFlr = (int)Math.Floor(sw);
            int shFlr = (int)Math.Floor(sh);
            int dxFlr = (int)Math.Floor(dw) > 8 ? (int)Math.Floor(dx) - 4 : (int)Math.Floor(dx) - 8;
            int dyFlr = (int)Math.Floor(dh) > 8 ? (int)Math.Floor(dy) - 4 : (int)Math.Floor(dy) - 8;
            int dwFlr = dw == -1 ? swFlr : (int)Math.Floor(dw) > 8 ? (int)Math.Floor(dw / 4) + 1 : (int)Math.Floor(dw / 8);
            int dhFlr = dh == -1 ? shFlr : (int)Math.Floor(dh) > 8 ? (int)Math.Floor(dh / 4) + 1 : (int)Math.Floor(dh / 8);

            var spriteWidth = swFlr;
            var spriteHeight = shFlr;

            //int spriteX = spriteNumberFlr % 16 * spriteWidth;
            //int spriteY = spriteNumberFlr / 16 * spriteHeight;

            int colorCache = 0;

            //for (int i = 0, j = 1; i < resetColors.Length; i++, j = 1)
            //{
            //    if (sprColors[i] != resetSprColors[i])
            //    {
            //        for (int k = 0; k < resetSprColors.Length; k++, j++)
            //        {
            //            if (sprColors[i] == resetSprColors[k])
            //            {
            //                colorCache += (j * (int)Math.Pow(10, i * 2)) * 1000;
            //                goto Continue;
            //            }
            //        }
            //    }
            //
            //    Continue:
            //
            //    var transparency = sprColors[i].A == 0 ? 1 : 2;
            //    colorCache += ((transparency * 16) * (int)Math.Pow(10, i * 2)) * 1000;
            //}

        }


        public int Sget(double x, double y, int? idk = null)
        {
            return 1;
        }


        public void Sset(double x, double y, double col)
        {

        }


        public int Stat(int i)
        {
            return i;
        }


    }


}