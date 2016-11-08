using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HuntTheWumpus3d
{
    public class WumpusGame : Game
    {
        private Vector3 _cameraPosition;

        private KeyboardState _currentKeyboardState;
        private MouseState _currentMouseState;

        // Are we rendering in wireframe mode?
        private bool _isWireFrame;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
        private Map _map;
        private Matrix _projection;
        private Matrix _view;

        // store a wireframe rasterize state
        private RasterizerState _wireFrameState;

        private Matrix _world;

        public WumpusGame()
        {
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
            _map = new Map(GraphicsDevice);
            _world = new Matrix();
            _cameraPosition = new Vector3(0, 0, 5.5f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _map.LoadContent();
            _wireFrameState = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            GraphicsDevice.RasterizerState = _isWireFrame ? _wireFrameState : RasterizerState.CullCounterClockwise;

            // Create camera matrices, making the object spin.
            var time = (float) gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            _world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            _view = Matrix.CreateLookAt(_cameraPosition, Vector3.Zero, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _map.Draw(_world, _view, _projection);

            // Reset the fill mode renderstate.
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            base.Draw(gameTime);
        }

        /// <summary>
        ///     Handles input for quitting or changing settings.
        /// </summary>
        private void HandleInput()
        {
            _lastKeyboardState = _currentKeyboardState;
            _lastMouseState = _currentMouseState;

            _currentKeyboardState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();

            if (IsPressed(Keys.Escape))
                Exit();

            if (IsPressed(Keys.E))
                _isWireFrame = !_isWireFrame;
        }

        private bool IsPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyUp(key);
        }

        private bool LeftMouseIsPressed(Rectangle r)
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _lastMouseState.LeftButton != ButtonState.Pressed &&
                   r.Contains(_currentMouseState.X, _currentMouseState.Y);
        }
    }
}