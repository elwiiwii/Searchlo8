﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Searchlo8
{

    partial class FNAGame : Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
            ArgumentNullException.ThrowIfNull(args);

            using FNAGame g = new();
            g.Run();
        }

#nullable enable

        private SpriteBatch batch;
        private GraphicsDeviceManager graphics;
        private Pico8 p8;

        //private Dictionary<string, Texture2D> textureDictionary = new();
        //private Dictionary<string, SoundEffect> musicDictionary = new();
        //private Dictionary<string, SoundEffect> soundEffectDictionary = new();

        private Texture2D pixel;

#nullable disable

        private int currentGameMode = 0;
        private double elapsedSeconds = 0.0;
        //string graphicsFolderPath = "Content/Graphics";
        //string musicFolderPath = "Content/Music";
        //string sfxFolderPath = "Content/Sfx";

        private FNAGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // Allow the user to resize the window
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            // All content loaded will be in a "Content" folder
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            graphics.IsFullScreen = false;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 30));
            graphics.SynchronizeWithVerticalRetrace = true;
        }


        protected override void Initialize()
        {
            base.Initialize();

            //optionsFile = OptionsFile.Initialize();
            p8.LoadGame(p8._cart, 4);

            UpdateViewport();
        }


        protected override void Update(GameTime gameTime)
        {
            p8.Update();
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            p8.Pal();
            p8.Palt();
            p8.Draw();

            // Draw the grid
            /*for (int i = 0; i <= 128; i++)
            {
                // Draw vertical lines
                batch.DrawLine(pixel, new Vector2(i * cellWidth, 0), new Vector2(i * cellWidth, viewportHeight), Color.White, 1);
                // Draw horizontal lines
                batch.DrawLine(pixel, new Vector2(0, i * cellHeight), new Vector2(viewportWidth, i * cellHeight), Color.White, 1);
            }*/

            batch.End();

            base.Draw(gameTime);
        }


        protected override void LoadContent()
        {
            // Create the batch...
            batch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel texture for drawing lines
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            //logo = Content.Load<Texture2D>("Graphics/CSharpCraftLogo.png");
            //optionsMenuBackground = Content.Load<Texture2D>("Graphics/OptionsMenuBackground.png");
            //optionsMenuTab = Content.Load<Texture2D>("Graphics/OptionsMenuTab.png");
            //selectorHalf = Content.Load<Texture2D>("Graphics/SelectorHalf.png");

            //for (int i = 0; i <= 6; i++)
            //{
            //    string fileName = $"Content/Music/music_{i}.wav";
            //    using var stream = TitleContainer.OpenStream(fileName);
            //    {
            //        music.Add(SoundEffect.FromStream(stream));
            //    }
            //}

            //for (int i = 0; i <= 21; i++)
            //{
            //    string fileName = $"Content/Sfx/sfx_{i}.wav";
            //    using var stream = TitleContainer.OpenStream(fileName);
            //    {
            //        soundEffects.Add(SoundEffect.FromStream(stream));
            //    }
            //}

            //string[] graphicsFiles = Directory.GetFiles(graphicsFolderPath, "*.png");
            //foreach (var file in graphicsFiles)
            //{
            //    using var stream = TitleContainer.OpenStream(file);
            //    {
            //        Texture2D texture = Texture2D.FromStream(GraphicsDevice, stream);
            //        string textureName = Path.GetFileNameWithoutExtension(file);
            //        textureDictionary.Add(textureName, texture);
            //    }
            //}
            //
            //string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.wav");
            //foreach (var file in musicFiles)
            //{
            //    using var stream = TitleContainer.OpenStream(file);
            //    {
            //        SoundEffect music = SoundEffect.FromStream(stream);
            //        string musicName = Path.GetFileNameWithoutExtension(file);
            //        musicDictionary.Add(musicName, music);
            //    }
            //}
            //
            //string[] sfxFiles = Directory.GetFiles(sfxFolderPath, "*.wav");
            //foreach (var file in sfxFiles)
            //{
            //    using var stream = TitleContainer.OpenStream(file);
            //    {
            //        SoundEffect soundEffect = SoundEffect.FromStream(stream);
            //        string soundEffectName = Path.GetFileNameWithoutExtension(file);
            //        soundEffectDictionary.Add(soundEffectName, soundEffect);
            //    }
            //}

            //p8 = new Pico8(soundEffectDictionary, musicDictionary, pixel, batch, GraphicsDevice);
            p8 = new Pico8(pixel, batch, GraphicsDevice);

        }


        protected override void UnloadContent()
        {
            batch.Dispose();
            pixel.Dispose();
            p8.Dispose();

            //foreach (var texture in textureDictionary.Values)
            //{
            //    texture.Dispose();
            //}
            //foreach (var music in musicDictionary.Values)
            //{
            //    music.Dispose();
            //}
            //foreach (var soundEffect in soundEffectDictionary.Values)
            //{
            //    soundEffect.Dispose();
            //}

            base.UnloadContent();

        }


        private void UpdateViewport()
        {
            // Calculate the size of the largest square that fits in the client area
            int maxSize = Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height);

            // Round the size down to the nearest multiple of 128
            int size = (maxSize / 128) * 128;

            // Calculate the exact center of the client area
            double centerX = Window.ClientBounds.Width / 2.0f;
            double centerY = Window.ClientBounds.Height / 2.0f;

            // Calculate the top left corner of the square so that its center aligns with the client area's center
            int left = (int)Math.Round(centerX - size / 2.0f);
            int top = (int)Math.Round(centerY - size / 2.0f);

            // Set the viewport to the square area
            GraphicsDevice.Viewport = new Viewport(left, top, size, size);
        }


        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewport();
        }

    }
}