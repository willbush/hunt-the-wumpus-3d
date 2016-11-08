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
        private readonly KeyboardListener _keyboardListener;
        private readonly Logger _logger;
        private Vector3 _cameraPosition;
        private SpriteFont _font;

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
            _keyboardListener = new KeyboardListener();
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

            _map = new Map(GraphicsDevice, _isCheatMode);
            _world = new Matrix();
            _cameraPosition = new Vector3(0, 0, 4.5f);

            _keyboardListener.KeyPressed += (sender, args) =>
            {
                if (args.Key == Keys.Escape)
                    Exit();

                if (args.Key == Keys.F)
                    _isWireFrame = !_isWireFrame;
                if (args.Key == Keys.A)
                    _logger.Write("How are you doing today? I am fine thanks..");
            };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("output");
            _map.LoadContent();
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
            _keyboardListener.Update(gameTime);
            GraphicsDevice.RasterizerState = _isWireFrame ? _wireFrameState : RasterizerState.CullCounterClockwise;

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