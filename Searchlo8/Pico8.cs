using System.Drawing;
using System.Xml.Linq;
using FixMath;

namespace Searchlo8
{
    public class Pico8
    {
        #region globals
        private int _btnState;
        private Cyclo8 _cart;
        private Cyclo8 _game;
        private int[] _sprites;
        private int[] _flags;
        private int[] _map;
        #endregion

        public Pico8()
        {
            _btnState = 0;
            _sprites = [];
            _flags = [];
            _map = [];
            _cart = new(this);
            LoadGame(_cart);
        }

        public Cyclo8 game
        {
            get => _game;
            set => _game = value;
        }

        private static int[] DataToArray(string s, int n)
        {
            int[] val = new int[s.Length / n];
            for (int i = 0; i < s.Length / n; i++)
            {
                val[i] = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
            }

            return val;
        }
        
        public void LoadGame(Cyclo8 cart)
        {
            _cart = cart;
            _game = _cart;
            _sprites = DataToArray(_game.SpriteData, 1);
            _flags = DataToArray(_game.FlagData, 2);
            _map = DataToArray(_game.MapData, 2);
            _game.Init();
        }

        public void Step()
        {
            _game.Update();
            //_game.Draw();
        }

        public void SetInputs(int l = 0, int r = 0, int u = 0, int d = 0, int z = 0, int x = 0)
        {
            SetBtnState(l * 1 + r * 2 + u * 4 + d * 8 + z * 16 + x * 32);
        }

        public void SetBtnState(int state)
        {
            _btnState = state;
        }


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            return (_btnState & (int)Math.Pow(2, i >> 16)) != 0;
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            return (_btnState & (int)Math.Pow(2, i >> 16)) != 0;
        }


        public void Camera(int x, int y) // https://pico-8.fandom.com/wiki/Camera
        {
            return;
        }


        public void Circ(int x, int y, float r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            return;
        }


        public void Circfill(int x, int y, float r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            return;
        }


        public void Cls() // https://pico-8.fandom.com/wiki/Cls
        {
            return;
        }


        public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
        {
            return _flags[n >> 16] << 16;
        }


        public void Line(int x1, int y1, int x2, int y2, int col)
        {
            return;
        }


        public void Map(int celx, int cely, int sx, int sy, int celw, int celh, int flags = 0) // https://pico-8.fandom.com/wiki/Map
        {
            return;
        }


        public int Mget(int celx, int cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = Math.Abs(celx >> 16);
            int yFlr = Math.Abs(cely >> 16);

            return _map[xFlr + yFlr * 128] << 16;
        }


        public int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }


        public void Mset(int celx, int cely, int snum) // https://pico-8.fandom.com/wiki/Mset
        {
            _map[celx + cely * 128] = snum >> 16;
        }


        public void Palt() // https://pico-8.fandom.com/wiki/Palt
        {
            return;
        }


        public void Palt(int col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            return;
        }


        public void Print(string str, int x, int y, int c) // https://pico-8.fandom.com/wiki/Print
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


        public void Rectfill(int x1, int y1, int x2, int y2, int c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            return;
        }


        public int Rnd(int lower, int upper) // https://pico-8.fandom.com/wiki/Rnd
        {
            Random random = new();
            if (lower < upper)
            {
                int n = random.Next() * (upper - lower) + lower;
                return n;
            }
            int n2 = random.Next() * (lower - upper) + upper;
            return n2;
        }


        public void Sfx(int n, int channel) // https://pico-8.fandom.com/wiki/Sfx
        {
            return;
        }


        public int Sget(int x, int y, int? idk = null)
        {
            int xFlr = x >> 16;
            int yFlr = y >> 16;

            if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
            {
                return 0;
            }

            return _sprites[xFlr + yFlr * 128];
        }


        public void Spr(int spriteNumber, int x, int y, int w = 1, int h = 1, bool flip_x = false) // https://pico-8.fandom.com/wiki/Spr
        {
            return;
        }


        public void Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, bool flip_x) // https://pico-8.fandom.com/wiki/Sspr
        {
            return;
        }


        public int Stat(int i)
        {
            return i;
        }

    }

}