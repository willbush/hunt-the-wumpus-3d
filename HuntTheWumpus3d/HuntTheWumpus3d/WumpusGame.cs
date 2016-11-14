using System;
using HuntTheWumpus3d.Infrastructure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.InputListeners;
using MonoGame.Extended.ViewportAdapters;

namespace HuntTheWumpus3d
{
    public class WumpusGame : Game
    {
        private readonly InputManager _inputManager;
        private readonly Logger _log;
        private Vector3 _cameraPosition;
        private SpriteFont _font;

        private Map _map;
        private Matrix _projection;
        private SpriteBatch _spriteBatch;
        private Matrix _view;
        private BoxingViewportAdapter _viewportAdapter;

        private Matrix _world;
        private bool _mapRotationIsFrozen;

        public WumpusGame()
        {
            _inputManager = InputManager.Instance;
            _log = Logger.Instance;

            var g = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            g.ApplyChanges();
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
        }

        private bool IsCheatMode { get; set; }

        protected override void Initialize()
        {
            const int weight = 900;
            const int height = 520;
            _viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, weight, height);
            _world = new Matrix();
            _cameraPosition = new Vector3(0, 0, 4.5f);

            _map = new Map(GraphicsDevice);

            _log.Write("Press escape at any time to immediately quit.");
            Play();

            _inputManager.KeyListener.KeyReleased += (sender, args) =>
            {
                if (args.Key == Keys.Escape)
                    Exit();
                if (args.Key == Keys.RightControl)
                {
                    IsCheatMode = !IsCheatMode;
                    _map.IsCheatMode = IsCheatMode;

                    if (IsCheatMode)
                        _map.PrintHazards();

                    _log.Write($"Cheat mode toggled to {_map.IsCheatMode}");
                }
            };
            _inputManager.MouseListener.MouseClicked += (sender, args) =>
            {
                if (args.Button == MouseButton.Right)
                    _mapRotationIsFrozen = !_mapRotationIsFrozen;
            };
            base.Initialize();
        }

        private void Play()
        {
            var es = _map.TakeTurn();

            if (es.IsGameOver)
            {
                es.Print();
                RequestPlayAgain();
            }
            else
            {
                PromptPlayerForAction();
            }
        }

        private void PromptPlayerForAction()
        {
            _log.Write(Message.ActionPrompt);

            EventHandler<KeyboardEventArgs> actionHandler = null;
            actionHandler = (sender, args) =>
            {
                switch (args.Key)
                {
                    case Keys.S:
                        _inputManager.KeyListener.KeyReleased -= actionHandler;
                        _map.PlayerShootArrow(GameOverHandler);
                        break;
                    case Keys.M:
                        _inputManager.KeyListener.KeyReleased -= actionHandler;
                        _map.MovePlayer(GameOverHandler);
                        break;
                    case Keys.Q:
                        _inputManager.KeyListener.KeyReleased -= actionHandler;
                        RequestPlayAgain();
                        break;
                }
            };
            _inputManager.KeyListener.KeyReleased += actionHandler;
        }

        private void GameOverHandler(EndState es)
        {
            if (es.IsGameOver)
            {
                es.Print();
                RequestPlayAgain();
            }
            else
            {
                Play();
            }
        }

        private void RequestPlayAgain()
        {
            _log.Write(Message.PlayPrompt);

            EventHandler<KeyboardEventArgs> playAgainResponseHandler = null;
            playAgainResponseHandler = (sender, args) =>
            {
                switch (args.Key)
                {
                    case Keys.Y:
                        _inputManager.KeyListener.KeyReleased -= playAgainResponseHandler;
                        _log.Messages.Clear();
                        RequestMapReset();
                        break;
                    case Keys.N:
                        _inputManager.KeyListener.KeyReleased -= playAgainResponseHandler;
                        Exit();
                        break;
                }
            };
            _inputManager.KeyListener.KeyReleased += playAgainResponseHandler;
        }

        private void RequestMapReset()
        {
            _log.Write(Message.SetupPrompt);

            EventHandler<KeyboardEventArgs> resetResponseHandler = null;
            resetResponseHandler = (sender, args) =>
            {
                switch (args.Key)
                {
                    case Keys.Y:
                        _inputManager.KeyListener.KeyReleased -= resetResponseHandler;
                        _log.Messages.Clear();
                        _map.Reset();
                        Play();
                        break;
                    case Keys.N:
                        _inputManager.KeyListener.KeyReleased -= resetResponseHandler;
                        _map = new Map(GraphicsDevice, IsCheatMode);
                        Play();
                        break;
                }
            };
            _inputManager.KeyListener.KeyReleased += resetResponseHandler;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("output");
        }

        protected override void UnloadContent()
        {
            _spriteBatch.Dispose();
            Content.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            _inputManager.Update(gameTime);
            _map.Update(gameTime, _world, _view, _projection, GraphicsDevice.Viewport);

            UpdateWorldMatrix(gameTime);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            _view = Matrix.CreateLookAt(_cameraPosition, new Vector3(1.3f, 0, 0), Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            base.Update(gameTime);
        }

        private void UpdateWorldMatrix(GameTime gameTime)
        {
            if (_mapRotationIsFrozen) return;

            var time = (float) gameTime.TotalGameTime.TotalSeconds;
            float yaw;
            float pitch;
            float roll;

            var ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed)
            {
                const float multiplier = 0.01f;
                yaw = ms.X * multiplier;
                pitch = ms.Y * multiplier;
                roll = 0;
            }
            else
            {
                yaw = time * 0.4f;
                pitch = time * 0.7f;
                roll = time * 1.1f;
            }
            _world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _inputManager.Draw(_spriteBatch, _font, _viewportAdapter);
            _map.Draw(_spriteBatch, _font, _world, _view, _projection);

            base.Draw(gameTime);
        }
    }
}