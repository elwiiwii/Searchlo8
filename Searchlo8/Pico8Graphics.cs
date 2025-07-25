using System;
using System.Xml.Linq;
using FixMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Searchlo8
{
    public class Pico8Graphics : IDisposable
    {
        #region globals
        private int _btnState;
        public Cyclo8F32Raw _cart;
        private Dictionary<string, int[]> _memory;
        private Dictionary<string, SoundEffect> SoundEffectDictionary;
        private Dictionary<string, SoundEffect> MusicDictionary;
        private Texture2D Pixel;
        private SpriteBatch Batch;
        private GraphicsDevice GraphicsDevice;
        private (int, int) CameraOffset;
        private Dictionary<int, Texture2D> spriteTextures = [];
        #endregion

        //public Pico8(Dictionary<string, SoundEffect> soundEffectDictionary, Dictionary<string, SoundEffect> musicDictionary, Texture2D pixel, SpriteBatch batch, GraphicsDevice graphicsDevice)
        public Pico8Graphics(Texture2D pixel, SpriteBatch batch, GraphicsDevice graphicsDevice)
        {
            _btnState = 0;
            _memory = [];
            //SoundEffectDictionary = soundEffectDictionary;
            //MusicDictionary = musicDictionary;
            Pixel = pixel;
            Batch = batch;
            GraphicsDevice = graphicsDevice;
            CameraOffset = (0, 0);
            _cart = new(this);
            //LoadGame(_cart);
        }

        private int[] DataToArray(string s, int n)
        {
            int[] val = new int[s.Length / n];
            for (int i = 0; i < s.Length / n; i++)
            {
                val[i] = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
            }

            return val;
        }

        public void LoadGame(Cyclo8F32Raw cart, int lvl)
        {
            _cart = cart;
            _memory = new() {
                { "sprites", DataToArray(_cart.SpriteData, 1) },
                { "flags", DataToArray(_cart.FlagData, 2) },
                { "map", DataToArray(_cart.MapData, 2) }
            };
            Array.Copy(colors, resetColors, colors.Length);
            Array.Copy(colors, sprColors, colors.Length);
            Array.Copy(colors, resetSprColors, colors.Length);
            _cart.Init();
            _cart.LoadLevel(lvl);
            //while (!_cart.Isdead)
            //{
            //    Step();
            //    //Console.WriteLine($"{_cart.Timer.ToString()} {_cart.Entities[0].Rot.ToString()}");
            //}
            //Console.WriteLine("DEAD");
        }

        private void Step()
        {
            _cart.Update();
            _cart.Draw();
        }

        public void Update()
        {
            _cart.Update();
        }

        public void Draw()
        {
            _cart.Draw();
        }

        private void SetInputs(int l = 0, int r = 0, int u = 0, int d = 0, int z = 0, int x = 0)
        {
            SetBtnState(l * 1 + r * 2 + u * 4 + d * 8 + z * 16 + x * 32);
        }

        private void SetBtnState(int state)
        {
            _btnState = state;
        }


        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color(r, g, b);
        }


        // pico-8 colors https://pico-8.fandom.com/wiki/Palette
        public Color[] colors =
        [
                HexToColor("000000"), // 00 black
                HexToColor("1D2B53"), // 01 dark-blue
                HexToColor("7E2553"), // 02 dark-purple
                HexToColor("008751"), // 03 dark-green
                HexToColor("AB5236"), // 04 brown
                HexToColor("5F574F"), // 05 dark-grey
                HexToColor("C2C3C7"), // 06 light-grey
                HexToColor("FFF1E8"), // 07 white
                HexToColor("FF004D"), // 08 red
                HexToColor("FFA300"), // 09 orange
                HexToColor("FFEC27"), // 10 yellow
                HexToColor("00E436"), // 11 green
                HexToColor("29ADFF"), // 12 blue
                HexToColor("83769C"), // 13 lavender
                HexToColor("FF77A8"), // 14 pink
                HexToColor("FFCCAA"), // 15 light-peach
                
                /*
                HexToColor("291814"), // 16 brownish-black
                HexToColor("111D35"), // 17 darker-blue
                HexToColor("422136"), // 18 darker-purple
                HexToColor("125359"), // 19 blue-green
                HexToColor("742F29"), // 20 dark-brown
                HexToColor("49333B"), // 21 darker-grey
                HexToColor("A28879"), // 22 medium-grey
                HexToColor("F3EF7D"), // 23 light-yellow
                HexToColor("BE1250"), // 24 dark-red
                HexToColor("FF6C24"), // 25 dark-orange
                HexToColor("A8E72E"), // 26 lime-green
                HexToColor("00B543"), // 27 medium-green
                HexToColor("065AB5"), // 28 true-blue
                HexToColor("754665"), // 29 mauve
                HexToColor("FF6E59"), // 30 dark-peach
                HexToColor("FF9D81"), // 31 peach
                */
        ];
        public Color[] resetColors = new Color[16];
        public Color[] resetSprColors = new Color[16];
        public Color[] sprColors = new Color[16];


        //private Pico8Functions(Color[] resetColors)
        //{
        //    this.resetColors = colors;
        //}

        private Texture2D CreateTextureFromSpriteData(int[] spriteData, int spriteX, int spriteY, int spriteWidth, int spriteHeight)
        {
            //spriteData = new string(spriteData.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            Texture2D texture = new(GraphicsDevice, spriteWidth, spriteHeight);

            Color[] colorData = new Color[spriteWidth * spriteHeight];

            //int j = 0;

            for (int i = spriteX + (spriteY * 128), j = 0; j < (spriteWidth * spriteHeight); i++, j++)
            {
                int colorIndex = spriteData[i];
                Color color = sprColors[colorIndex]; // Convert the PICO-8 color index to a Color
                if (colorIndex != 3)
                {
                    colorData[j] = color;
                }

                if (i % spriteWidth == spriteWidth - 1) { i += 128 - spriteWidth; }
                //j++;
            }

            texture.SetData(colorData);

            return texture;
        }


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            KeyboardState state = Keyboard.GetState();
            return i == 2 && state.IsKeyDown(Keys.W);
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            return false;
        }


        public void Camera(int x, int y) // https://pico-8.fandom.com/wiki/Camera
        {
            int xFlr = x >> 16;
            int yFlr = y >> 16;

            CameraOffset = (xFlr, yFlr);
        }


        public void Circ(int x, int y, int r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            int xFlr = x >> 16;
            int yFlr = y >> 16;
            int rFlr = r >> 16;
            int cFlr = c >> 16;

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

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
                        // Calculate the position and size of the line
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw the line
                        Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Circfill(int x, int y, int r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            int xFlr = x >> 16;
            int yFlr = y >> 16;
            int rFlr = r >> 16;
            int cFlr = c >> 16;

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

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
                        // Calculate the position and size
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw
                        Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Cls() // https://pico-8.fandom.com/wiki/Cls
        {
            GraphicsDevice.Clear(resetColors[0]);
        }


        public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
        {
            return _memory["flags"][n >> 16] << 16;
        }


        public void Line(int x1, int y1, int x2, int y2, int col)
        {
            return;
        }


        public void Map(int celx, int cely, int sx, int sy, int celw, int celh, int flags = 0) // https://pico-8.fandom.com/wiki/Map
        {
            int cxFlr = celx >> 16;
            int cyFlr = cely >> 16;
            int sxFlr = sx >> 16;
            int syFlr = sy >> 16;
            int cwFlr = celw >> 16;
            int chFlr = celh >> 16;

            for (int i = 0; i < cwFlr; i++)
            {
                for (int j = 0; j < chFlr; j++)
                {
                    if (flags == 0 || flags == Fget(Mget((cxFlr + i) << 16, (cyFlr + j) << 16)))
                    {
                        Spr(Mget((i + cxFlr) << 16, (j + cyFlr) << 16), (sxFlr + i * 8) << 16, (syFlr + j * 8) << 16);
                    }
                }
            }
        }


        public int Mget(int celx, int cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = Math.Abs(celx >> 16);
            int yFlr = Math.Abs(cely >> 16);

            string s = $"0x{_cart.MapData.Substring(xFlr * 2 + (yFlr * 256), 2)}";
            return Convert.ToInt32(s, 16) << 16;
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

            _memory["map"][xFlr + yFlr * 128] = sFlr;
        }


        public void Pal() // https://pico-8.fandom.com/wiki/Pal
        {
            Array.Copy(resetSprColors, sprColors, resetSprColors.Length);
            Array.Copy(resetColors, colors, resetColors.Length);
        }


        public void Palt() // https://pico-8.fandom.com/wiki/Palt
        {
            sprColors[0].A = 0;
            resetSprColors[0].A = 0;
            for (int i = 1; i <= 15; i++)
            {
                sprColors[i].A = 255;
                resetSprColors[i].A = 255;
            }
        }


        public void Palt(int col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            int cFlr = col >> 16;
            if (t)
            {
                sprColors[cFlr] = sprColors[0];
                sprColors[cFlr].A = 0;
                resetSprColors[cFlr].A = 0;
            }
            else
            {
                sprColors[cFlr] = resetSprColors[cFlr];
                sprColors[cFlr].A = 255;
                resetSprColors[cFlr].A = 255;
            }
        }


        public void Print(string str, int x, int y, int c) // https://pico-8.fandom.com/wiki/Print
        {
            int xFlr = x >> 16;
            int yFlr = y >> 16;
            int cFlr = c >> 16;

            int charWidth = 4;
            //int charHeight = 5;

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int s = 0; s < str.Length; s++)
            {
                char letter = str[s];

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Font.chars[letter][i, j] == 1)
                        {
                            var charStartX = (s * charWidth + xFlr + j - CameraOffset.Item1) * cellWidth;
                            //var charEndX = charStartX + cellWidth - CameraOffset.Item1;
                            var charStartY = (yFlr + i - CameraOffset.Item2) * cellHeight;

                            Vector2 position = new Vector2(charStartX, charStartY);
                            Vector2 size = new(cellWidth, cellHeight);

                            Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                        }
                    }
                }
            }
        }


        public int Pget(int x, int y)
        {
            return 1;
        }


        public void Pset(int x, int y, int c) // https://pico-8.fandom.com/wiki/Pset
        {
            int xFlr = x >> 16;
            int yFlr = y >> 16;
            int cFlr = c >> 16;

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            // Calculate the position and size of the line
            Vector2 position = new((xFlr - CameraOffset.Item1) * cellWidth, (yFlr - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            // Draw the line
            Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Rectfill(int x1, int y1, int x2, int y2, int c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            int x1Flr = x1 >> 16;
            int y1Flr = y1 >> 16;
            int x2Flr = x2 >> 16;
            int y2Flr = y2 >> 16;
            int cFlr = c >> 16;

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            var rectStartX = (x1Flr - CameraOffset.Item1) * cellWidth;
            var rectStartY = (y1Flr - CameraOffset.Item2) * cellHeight;

            var rectSizeX = (x2Flr - x1Flr + 1) * cellWidth;
            var rectSizeY = (y2Flr - y1Flr + 1) * cellHeight;

            //var rectEndX = (x2Flr - CameraOffset.Item1) * cellWidth;
            //var rectThickness = (y2Flr - y1Flr) * cellHeight;
            //batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[cFlr], rectThickness);

            Vector2 position = new(rectStartX, rectStartY);
            Vector2 size = new(rectSizeX, rectSizeY);

            Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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

            string c = $"0x{_cart.SpriteData[xFlr + (yFlr * 128)]}";

            return Convert.ToInt32(c, 16) << 16;
        }


        public void Spr(int spriteNumber, int x, int y, int w = 65536, int h = 65536, bool flip_x = false) // https://pico-8.fandom.com/wiki/Spr
        {
            int sFlr = spriteNumber >> 16;
            int xFlr = (x >> 16) - 8;
            int yFlr = (y >> 16) - 8;
            int wFlr = w >> 16;
            int hFlr = h >> 16;

            var spriteWidth = 8;
            var spriteHeight = 8;

            int spriteX = sFlr % 16 * spriteWidth;
            int spriteY = sFlr / 16 * spriteHeight;

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

            for (int i = 0; i < resetSprColors.Length; i++)
            {
                if (sprColors[i] != resetSprColors[i])
                {
                    for (int j = 0; j < resetSprColors.Length; j++)
                    {
                        if (sprColors[i] == resetSprColors[j])
                        {
                            colorCache += (i * 100 + j) * 1000;
                            break;
                        }
                    }
                }
            }

            if (!spriteTextures.TryGetValue(sFlr + colorCache, out var texture))
            {
                texture = CreateTextureFromSpriteData(_memory["sprites"], spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
                spriteTextures[sFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = Batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? xFlr + (2 * spriteWidth * wFlr) - spriteWidth : xFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, (yFlr + spriteHeight - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (SpriteEffects.None);

            Batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
        }


        public void Sspr(int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, bool flip_x) // https://pico-8.fandom.com/wiki/Sspr
        {
            int sxFlr = sx >> 16;
            int syFlr = sy >> 16;
            int swFlr = sw >> 16;
            int shFlr = sh >> 16;
            int dxFlr = dx >> 16;
            int dyFlr = dy >> 16;
            int dwFlr = dw >> 16;
            int dhFlr = dh >> 16;

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

            for (int i = 0; i < resetSprColors.Length; i++)
            {
                if (sprColors[i] != resetSprColors[i])
                {
                    for (int j = 0; j < resetSprColors.Length; j++)
                    {
                        if (sprColors[i] == resetSprColors[j])
                        {
                            colorCache += (i * 100 + j) * 1000;
                            break;
                        }
                    }
                }
            }

            var spriteNumberFlr = (sxFlr * 100) + (syFlr * 100) + (swFlr * 100) + (shFlr * 100);

            if (!spriteTextures.TryGetValue(spriteNumberFlr + colorCache, out var texture))
            {
                texture = CreateTextureFromSpriteData(_memory["sprites"], sxFlr, syFlr, swFlr, shFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = Batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? dxFlr + (2 * spriteWidth * swFlr) - spriteWidth : dxFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, (dyFlr + spriteHeight - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(dwFlr * cellWidth, dhFlr * cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | SpriteEffects.None;

            Batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
        }


        public int Stat(int i)
        {
            return i;
        }


        public void Dispose()
        {
            foreach (var texture in spriteTextures.Values)
            {
                texture.Dispose();
            }
            spriteTextures.Clear();

            //if (channelMusic != null)
            //{
            //    foreach (var song in channelMusic)
            //    {
            //        song.Dispose();
            //    }
            //    channelMusic.Clear();
            //}
            //if (soundEffects != null)
            //{
            //    foreach (var soundEffect in soundEffects)
            //    {
            //        soundEffect?.Dispose();
            //    }
            //}
            //if (music != null)
            //{
            //    foreach (var song in music)
            //    {
            //        song?.Dispose();
            //    }
            //}
            //pixel.Dispose();
            //batch.Dispose();
            //graphicsDevice.Dispose();

        }

    }
}