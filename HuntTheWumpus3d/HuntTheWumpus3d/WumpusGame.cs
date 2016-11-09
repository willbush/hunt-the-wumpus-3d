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
        private readonly bool _isCheatMode;
        private readonly Logger _logger;
        private Vector3 _cameraPosition;
        private SpriteFont _font;
        private readonly InputHandler _inputHandler;

        // Are we rendering in wireframe mode?
        private bool _isWireFrame;
        private Map _map;
        private Matrix _projection;
        private SpriteBatch _spriteBatch;
        private Matrix _view;
        private BoxingViewportAdapter _viewportAdapter;

        // store a wireframe rasterize state
        private RasterizerState _wireFrameState;

        private Matrix _world;

        public WumpusGame(bool isCheatMode)
        {
            _isCheatMode = isCheatMode;
            _inputHandler = InputHandler.Instance;
            _logger = Logger.Instance;

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

        protected override void Initialize()
        {
            const int weight = 900;
            const int height = 520;
            _viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, weight, height);
            _world = new Matrix();
            _cameraPosition = new Vector3(0, 0, 4.5f);

            _map = new Map(GraphicsDevice, _isCheatMode);

            Play();

            _inputHandler.KeyListener.KeyPressed += (sender, args) =>
            {
                if (args.Key == Keys.Escape)
                    Exit();

                if (args.Key == Keys.F)
                    _isWireFrame = !_isWireFrame;
            };
            base.Initialize();
        }

        private void Play()
        {
            _map.Update();
            _logger.Write(Message.ActionPrompt);

            EventHandler<KeyboardEventArgs> actionToPerformOnKeyPress = null;
            actionToPerformOnKeyPress = (sender, args) =>
            {
                switch (args.Key)
                {
                    case Keys.S:
                        _inputHandler.KeyListener.KeyPressed -= actionToPerformOnKeyPress;
                        _map.GetEndState("S");
                        break;
                    case Keys.M:
                        _inputHandler.KeyListener.KeyPressed -= actionToPerformOnKeyPress;
                        _map.GetEndState("M");
                        break;
                    case Keys.Q:
                        _inputHandler.KeyListener.KeyPressed -= actionToPerformOnKeyPress;
                        EndState e = _map.GetEndState("Q");
                        if (e.IsGameOver)
                        {
                            e.Print();
                            RequestPlayAgain();
                        }
                        break;
                }
            };
            _inputHandler.KeyListener.KeyPressed += actionToPerformOnKeyPress;
        }

        private void RequestPlayAgain()
        {
            _logger.Write(Message.PlayPrompt);

            EventHandler<KeyboardEventArgs> playAgainResponseHandler = null;
            playAgainResponseHandler = (sender, args) =>
            {
                switch (args.Key)
                {
                    case Keys.Y:
                        _inputHandler.KeyListener.KeyPressed -= playAgainResponseHandler;
                        RequestMapReset();
                        break;
                    case Keys.N:
                        _inputHandler.KeyListener.KeyPressed -= playAgainResponseHandler;
                        Exit();
                        break;
                }
            };
            _inputHandler.KeyListener.KeyPressed += playAgainResponseHandler;
        }

        private void RequestMapReset()
        {
            _logger.Write(Message.SetupPrompt);

            EventHandler<KeyboardEventArgs> resetResponseHandler = null;
            resetResponseHandler = (sender, args) =>
            {
                switch (args.Key)
                {
                    case Keys.Y:
                        _inputHandler.KeyListener.KeyPressed -= resetResponseHandler;
                        _map.Reset();
                        Play();
                        break;
                    case Keys.N:
                        _inputHandler.KeyListener.KeyPressed -= resetResponseHandler;
                        _map = new Map(GraphicsDevice, _isCheatMode);
                        Play();
                        break;
                }
            };
            _inputHandler.KeyListener.KeyPressed += resetResponseHandler;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("output");
            _wireFrameState = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
        }

        protected override void UnloadContent()
        {
            _spriteBatch.Dispose();
            Content.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = _isWireFrame ? _wireFrameState : RasterizerState.CullCounterClockwise;
            _inputHandler.Update(gameTime);

            // Create camera matrices, making the object spin.
            var time = (float) gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            _world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            _view = Matrix.CreateLookAt(_cameraPosition, Vector3.Right, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: _viewportAdapter.GetScaleMatrix());
            _logger.Messages.ForEach(m => _spriteBatch.DrawString(_font, m.Value, m.Position, m.Color));
            _spriteBatch.End();
            _map.Draw(_world, _view, _projection);

            // Reset the fill mode renderstate.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            base.Draw(gameTime);
        }
    }
}