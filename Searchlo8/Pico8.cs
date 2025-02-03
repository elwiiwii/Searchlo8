﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Searchlo8
{
    public class Pico8(Dictionary<string, SoundEffect> soundEffectDictionary, Dictionary<string, SoundEffect> musicDictionary, Texture2D pixel, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile) : IDisposable
    {
        private Dictionary<int, Texture2D> spriteTextures = [];
        private int[] Map1 = new int[128 * 64];
        private (int, int) CameraOffset = (0, 0);
        private char[] spriteSheet1 = new char[128 * 128];

        public List<SoundEffectInstance>? channelMusic = [];
        private List<SoundEffectInstance>? channel0 = [];
        private List<SoundEffectInstance>? channel1 = [];
        private List<SoundEffectInstance>? channel2 = [];
        private List<SoundEffectInstance>? channel3 = [];

        public bool prev0 = false;
        public bool prev1 = false;
        public bool prev2 = false;
        public bool prev3 = false;
        public bool prev4 = false;
        public bool prev5 = false;

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

        private Texture2D CreateTextureFromSpriteData(char[] spriteData, int spriteX, int spriteY, int spriteWidth, int spriteHeight)
        {
            //spriteData = new string(spriteData.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            Texture2D texture = new(graphicsDevice, spriteWidth, spriteHeight);

            Color[] colorData = new Color[spriteWidth * spriteHeight];

            //int j = 0;

            for (int i = spriteX + (spriteY * 128), j = 0; j < (spriteWidth * spriteHeight); i++, j++)
            {
                char c = spriteData[i];
                int colorIndex = Convert.ToInt32(c.ToString(), 16); // Convert hex to int
                Color color = sprColors[colorIndex]; // Convert the PICO-8 color index to a Color
                colorData[j] = color;

                if (i % spriteWidth == spriteWidth - 1) { i += 128 - spriteWidth; }
                //j++;
            }

            texture.SetData(colorData);

            return texture;
        }


        public Entity Add(List<Entity> table, Entity value, int index = -1) // https://pico-8.fandom.com/wiki/Add
        {
            if (index == -1) { table.Add(value); return value; }
            table.Insert(index, value);
            return value;
        }


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            KeyboardState state = Keyboard.GetState();

            if (i == 0) { return state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keyboardOptionsFile.Left.Bind1)); }
            if (i == 1) { return state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keyboardOptionsFile.Right.Bind1)); }
            if (i == 2) { return state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keyboardOptionsFile.Up.Bind1)); }
            if (i == 3) { return state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keyboardOptionsFile.Down.Bind1)); }
            if (i == 4) { return state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keyboardOptionsFile.Menu.Bind1)); }
            if (i == 5) { return state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keyboardOptionsFile.Use.Bind1)); }
            else { return false; }
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            if (i == 0 && Btn(i) && !prev0) { return true; }
            if (i == 1 && Btn(i) && !prev1) { return true; }
            if (i == 2 && Btn(i) && !prev2) { return true; }
            if (i == 3 && Btn(i) && !prev3) { return true; }
            if (i == 4 && Btn(i) && !prev4) { return true; }
            if (i == 5 && Btn(i) && !prev5) { return true; }
            else { return false; }
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

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Cls(double color = 0) // https://pico-8.fandom.com/wiki/Cls
        {
            int colorFlr = (int)Math.Floor(color);

            graphicsDevice.Clear(resetColors[colorFlr]);
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


        public void Init()
        {
            Array.Copy(colors, resetColors, colors.Length);
            Array.Copy(colors, sprColors, colors.Length);
            Array.Copy(colors, resetSprColors, colors.Length);

            spriteSheet1 = SpriteSheets.SpriteSheet1.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray();
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
                var secondHalf = SpriteSheets.SpriteSheet2.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray();
                secondHalf.CopyTo(spriteSheet1, secondHalf.Length);
            }
        }


        public int MgetOld(double celx, double cely)
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

        }


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

            if (channelMusic != null)
            {
                foreach (var song in channelMusic)
                {
                    song.Dispose();
                }
            }

            if (nFlr == 1)
            {
                SoundEffectInstance instance = musicDictionary[$"music_{nFlr}"].CreateInstance();
                channelMusic.Add(instance);
                instance.Play();
            }
            else if (nFlr == 4)
            {
                SoundEffectInstance instance = musicDictionary[$"music_{nFlr}"].CreateInstance();
                channelMusic.Add(instance);
                instance.Play();
            }
            else
            {
                return;
            }

        }


        public void Pal() // https://pico-8.fandom.com/wiki/Pal
        {
            Array.Copy(resetSprColors, sprColors, resetSprColors.Length);
            Array.Copy(resetColors, colors, resetColors.Length);
        }


        public void Pal(double c0, double c1) // https://pico-8.fandom.com/wiki/Pal
        {
            int c0Flr = (int)Math.Floor(c0);
            int c1Flr = (int)Math.Floor(c1);

            sprColors[c0Flr] = c1Flr == 0 ? resetColors[c1Flr] : resetSprColors[c1Flr];
            colors[c0Flr] = resetColors[c1Flr];
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


        public void Palt(double col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            int colFlr = (int)Math.Floor(col);

            if (t)
            {
                sprColors[colFlr] = sprColors[0];
                sprColors[colFlr].A = 0;
                resetSprColors[colFlr].A = 0;
            }
            else
            {
                sprColors[colFlr] = resetSprColors[colFlr];
                sprColors[colFlr].A = 255;
                resetSprColors[colFlr].A = 255;
            }
        }


        public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int cFlr = (int)Math.Floor(c);

            int charWidth = 4;
            //int charHeight = 5;

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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

                            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                        }
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

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            // Calculate the position and size of the line
            Vector2 position = new((xFlr - CameraOffset.Item1) * cellWidth, (yFlr - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            // Draw the line
            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            int x1Flr = (int)Math.Floor(x1);
            int y1Flr = (int)Math.Floor(y1);
            int x2Flr = (int)Math.Floor(x2);
            int y2Flr = (int)Math.Floor(y2);
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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

            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Reload() // https://pico-8.fandom.com/wiki/Reload
        {
            spriteSheet1 = SpriteSheets.SpriteSheet1.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray();
            Map1 = new int[128 * 64];
            spriteTextures = [];
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

            List<SoundEffectInstance>? c = channelFlr switch
            {
                0 => channel0,
                1 => channel1,
                2 => channel2,
                3 => channel3,
                _ => throw new ArgumentOutOfRangeException(nameof(channel)),
            };

            if (c != null)
            {
                foreach (var sfxInstance in c)
                {
                    sfxInstance.Dispose();
                }

                SoundEffectInstance instance = soundEffectDictionary[$"sfx_{nFlr}"].CreateInstance();

                c.Add(instance);

                instance.Play();
            }
            else
            {
                return;
            }

        }


        public double Sin(double angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Sin
        {
            return Math.Sin(-angle * 2 * Math.PI);
        }


        public void SoundDispose()
        {
            if (channelMusic != null)
            {
                foreach (var song in channelMusic)
                {
                    song.Dispose();
                }
            }

            for (int i = 0; i < 4; i++)
            {
                List<SoundEffectInstance>? c = i switch
                {
                    0 => channel0,
                    1 => channel1,
                    2 => channel2,
                    3 => channel3,
                    _ => throw new ArgumentOutOfRangeException(nameof(i)),
                };
                if (c != null)
                {
                    foreach (var sfxInstance in c)
                    {
                        sfxInstance.Dispose();
                    }
                }
            }
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

            if (!spriteTextures.TryGetValue(spriteNumberFlr + colorCache, out var texture))
            {
                texture = CreateTextureFromSpriteData(spriteSheet1, spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? xFlr + (2 * spriteWidth * wFlr) - spriteWidth : xFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? yFlr + (2 * spriteHeight * hFlr) - spriteHeight : yFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

            batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
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
                texture = CreateTextureFromSpriteData(spriteSheet1, sxFlr, syFlr, swFlr, shFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? dxFlr + (2 * spriteWidth * swFlr) - spriteWidth : dxFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? dyFlr + (2 * spriteHeight * shFlr) - spriteHeight : dyFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(dwFlr * cellWidth, dhFlr * cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

            batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
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


        public void Update()
        {
            prev0 = Btn(0);
            prev1 = Btn(1);
            prev2 = Btn(2);
            prev3 = Btn(3);
            prev4 = Btn(4);
            prev5 = Btn(5);
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