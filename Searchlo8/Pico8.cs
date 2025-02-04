using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            _cart.LoadLevel(4);
            while (!_cart.Isdead)
            {
                Step();
                Console.WriteLine(_cart.Timer.ToString());
            }
            Console.WriteLine("DEAD");
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


        public void Camera(double x = 0, double y = 0) // https://pico-8.fandom.com/wiki/Camera
        {
            return;
        }


        public void Circ(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            return;
        }


        public void Circfill(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            return;
        }


        public void Cls(double color = 0) // https://pico-8.fandom.com/wiki/Cls
        {
            return;
        }


        public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
        {
            return Convert.ToInt32(_cart.FlagData.Substring(n * 2, 2));
        }


        public void Line(double x1, double y1, double x2, double y2, int col)
        {
            return;
        }


        public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int? flags = null) // https://pico-8.fandom.com/wiki/Map
        {
            return;
        }


        public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = Math.Abs((int)Math.Floor(celx));
            int yFlr = Math.Abs((int)Math.Floor(cely));

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


        public double Mod(double x, double m)
        {
            double r = x % m;
            return r < 0 ? r + m : r;
        }


        public void Mset(double celx, double cely, double snum = 0) // https://pico-8.fandom.com/wiki/Mset
        {
            return;
        }


        public void Music(double n, double fadems = 0, double channelmask = 0) // https://pico-8.fandom.com/wiki/Music
        {
            return;
        }


        public void Palt(double col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            return;
        }


        public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
        {
            return;
        }


        public int Pget(double x, double y)
        {
            return 1;
        }


        public void Pset(double x, double y, double c) // https://pico-8.fandom.com/wiki/Pset
        {
            return;
        }


        public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            return;
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
            return;
        }


        public void Spr(double spriteNumber, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Spr
        {
            return;
        }


        public void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw = -1, double dh = -1, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Sspr
        {
            return;
        }


        public int Sget(double x, double y, int? idk = null)
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);

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