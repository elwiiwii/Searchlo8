using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FixMath;

namespace Searchlo8
{
    public class Pico8
    {
        #region globals
        private int _btnState;
        private Cyclo8 _cart;
        #endregion

        public Pico8()
        {
            _btnState = 0;
            _cart = new(this);
            LoadGame(_cart);
        }

        private void LoadGame(Cyclo8 cart)
        {
            _cart = cart;
            _cart.Init();
            _cart.LoadLevel(3);
            while (!_cart.Isdead)
            {
                Step();
                //Console.WriteLine($"{_cart.Timer.ToString()} {_cart.Entities[0].Rot.ToString()}");
            }
            //Console.WriteLine("DEAD");
        }

        private void Step()
        {
            _cart.Update();
            //_cart.Draw();
        }

        private void SetInputs(int l = 0, int r = 0, int u = 0, int d = 0, int z = 0, int x = 0)
        {
            SetBtnState(l * 1 + r * 2 + u * 4 + d * 8 + z * 16 + x * 32);
        }

        private void SetBtnState(int state)
        {
            _btnState = state;
        }


        public int Band(int first, int second)
        {
            char[] cfirst = Convert.ToString(first, 2).ToCharArray();
            char[] csecond = Convert.ToString(second, 2).ToCharArray();
            Array.Reverse(cfirst);
            Array.Reverse(csecond);

            int val = 0;
            for (int i = 0; i < Math.Min(cfirst.Length, csecond.Length); i++)
            {
                if (cfirst[i] == '1' && csecond[i] == '1')
                {
                    val += (int)Math.Pow(2, i);
                }
            }
            return val;
        }


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            return false;
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            return false;
        }


        public void Camera(F32 x, F32 y) // https://pico-8.fandom.com/wiki/Camera
        {
            return;
        }


        public void Circ(F32 x, F32 y, F32 r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            return;
        }


        public void Circfill(F32 x, F32 y, F32 r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            return;
        }


        public void Cls() // https://pico-8.fandom.com/wiki/Cls
        {
            return;
        }


        public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
        {
            return Convert.ToInt32(_cart.FlagData.Substring(n * 2, 2));
        }


        public void Line(F32 x1, F32 y1, F32 x2, F32 y2, int col)
        {
            return;
        }


        public void Map(int celx, int cely, F32 sx, F32 sy, int celw, int celh, int? flags = null) // https://pico-8.fandom.com/wiki/Map
        {
            return;
        }


        public int Mget(F32 celx, F32 cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = Math.Abs(F32.FloorToInt(celx));
            int yFlr = Math.Abs(F32.FloorToInt(cely));

            string s = $"0x{_cart.MapData.Substring(xFlr * 2 + (yFlr * 256), 2)}";
            return Convert.ToInt32(s, 16);
        }


        /*public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);

            char c = _cart.MapData[xFlr + (yFlr * 128)];

            int intC = 0;

            if (c >= 48 && c <= 57)
            {
                intC = c - '0';
            }
            else if (c >= 97 && c <= 102)
            {
                intC = 10 + c - 'a';
            }

            return intC;
        }*/


        public F32 Mod(F32 x, F32 m)
        {
            F32 r = x % m;
            return r < 0 ? r + m : r;
        }


        public void Mset(F32 celx, F32 cely, int snum) // https://pico-8.fandom.com/wiki/Mset
        {
            return;
        }


        public void Music(F32 n, F32 fadems, F32 channelmask) // https://pico-8.fandom.com/wiki/Music
        {
            return;
        }


        public void Palt(int col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            return;
        }


        public void Print(string str, int x, int y, F32 c) // https://pico-8.fandom.com/wiki/Print
        {
            return;
        }


        public int Pget(int x, int y)
        {
            return 1;
        }


        public void Pset(int x, int y, int c) // https://pico-8.fandom.com/wiki/Pset
        {
            return;
        }


        public void Rectfill(F32 x1, F32 y1, F32 x2, F32 y2, F32 c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            return;
        }


        public F32 Rnd(double lower, double upper) // https://pico-8.fandom.com/wiki/Rnd
        {
            Random random = new();
            if (lower < upper)
            {
                double n = random.NextDouble() * (upper - lower) + lower;
                return F32.FromDouble(n);
            }
            double n2 = random.NextDouble() * (lower - upper) + upper;
            return F32.FromDouble(n2);
        }


        public void Sfx(int n, int channel) // https://pico-8.fandom.com/wiki/Sfx
        {
            return;
        }


        public void Spr(F32 spriteNumber, F32 x, F32 y, int w, int h) // https://pico-8.fandom.com/wiki/Spr
        {
            return;
        }


        public void Spr(F32 spriteNumber, F32 x, F32 y, int w, int h, bool flip_x) // https://pico-8.fandom.com/wiki/Spr
        {
            return;
        }


        public void Sspr(F32 sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, bool flip_x) // https://pico-8.fandom.com/wiki/Sspr
        {
            return;
        }


        public int Sget(F32 x, F32 y, int? idk = null)
        {
            int xFlr = F32.FloorToInt(x);
            int yFlr = F32.FloorToInt(y);

            if (xFlr < 0 || yFlr < 0)
            {
                return 0;
            }

            char c = _cart.SpriteData[xFlr + (yFlr * 128)];

            return Convert.ToInt32(c);
        }


        public int Stat(int i)
        {
            return i;
        }


    }


}