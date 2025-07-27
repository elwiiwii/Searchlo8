using System.Drawing;
using System.Xml.Linq;
using FixMath;

namespace Searchlo8
{
    public class Pico8
    {
        #region globals
        private int _btnState;
        public Cyclo8 _cart;
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
            get => _cart;
            set => _cart = value;
        }

        private int[] DataToArray(string s, int n)
        {
            if (string.IsNullOrEmpty(s))
            {
                Console.WriteLine($"WARNING: Empty or null data string in thread {Thread.CurrentThread.ManagedThreadId}");
                return new int[8192]; // Return a default-sized array
            }
            
            int[] val = new int[s.Length / n];
            for (int i = 0; i < s.Length / n; i++)
            {
                val[i] = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
            }

            return val;
        }
        
        public void LoadGame(Cyclo8 cart)
        {
            game = cart;
            _sprites = DataToArray(game.SpriteData, 1);
            _flags = DataToArray(game.FlagData, 2);
            _map = DataToArray(game.MapData, 2);
            
            // Ensure all arrays have reasonable sizes
            if (_sprites.Length == 0)
            {
                Console.WriteLine($"WARNING: _sprites is empty after LoadGame in thread {Thread.CurrentThread.ManagedThreadId}");
                _sprites = new int[8192]; // Default size
            }
            
            if (_flags.Length == 0)
            {
                Console.WriteLine($"WARNING: _flags is empty after LoadGame in thread {Thread.CurrentThread.ManagedThreadId}");
                _flags = new int[256]; // Default size for flags
            }
            
            if (_map.Length == 0)
            {
                Console.WriteLine($"WARNING: _map is empty after LoadGame in thread {Thread.CurrentThread.ManagedThreadId}");
                _map = new int[8192]; // Default size
            }
            
            game.Init();
        }

        public void Step()
        {
            game.Update();
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
            return (_btnState & (int)Math.Pow(2, i)) != 0;
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            return (_btnState & (int)Math.Pow(2, i)) != 0;
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
            int index = n >> 16;
            
            // Safety check
            if (_flags == null || index >= _flags.Length)
            {
                return 0;
            }
            
            return _flags[index] << 16;
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

            if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
            {
                return 0;
            }

            int index = xFlr + yFlr * 128;
            
            // Safety check
            if (_map == null || index >= _map.Length)
            {
                return 0;
            }

            return (_map[index]) << 16;
        }


        public int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }


        public void Mset(int celx, int cely, int snum) // https://pico-8.fandom.com/wiki/Mset
        {
            int xFlr = Math.Abs(celx >> 16);
            int yFlr = Math.Abs(cely >> 16);
            int sFlr = snum >> 16;

            if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
            {
                return;
            }

            _map[xFlr + yFlr * 128] = sFlr;
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

            int index = xFlr + yFlr * 128;
            
            // Safety check
            if (_sprites == null || index >= _sprites.Length)
            {
                return 0;
            }

            return _sprites[index] << 16;
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