using System.Collections.Generic;
using HuntTheWumpus3d.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HuntTheWumpus3d
{
    public class WumpusGame : Game
    {
        // Store a list of tint colors, plus which one is currently selected.
        private readonly List<Color> _colors = new List<Color>
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.White,
            Color.Black
        };

        // Store a list of primitive models, plus which one is currently selected.
        private readonly List<GeometricShape> _primitives = new List<GeometricShape>();

        private int _currentColorIndex;

        private KeyboardState _currentKeyboardState;
        private MouseState _currentMouseState;

        private int _currentPrimitiveIndex;

        // Are we rendering in wireframe mode?
        private bool _isWireframe;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;

        // store a wireframe rasterize state
        private RasterizerState _wireFrameState;

        public WumpusGame()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _primitives.Add(new Sphere(GraphicsDevice));
            _primitives.Add(new Cylinder(GraphicsDevice));

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
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.RasterizerState = _isWireframe ? _wireFrameState : RasterizerState.CullCounterClockwise;

            // Create camera matrices, making the object spin.
            var time = (float) gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            var cameraPosition = new Vector3(0, 0, 2.5f);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            var world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            var view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            var projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            // Draw the current primitive.
            var currentPrimitive = _primitives[_currentPrimitiveIndex];
            var color = _colors[_currentColorIndex];

            currentPrimitive.Draw(world, view, projection, color);

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

            // Check for exit.
            if (IsPressed(Keys.Escape))
            {
                Exit();
            }

            // Change primitive?
            var viewport = GraphicsDevice.Viewport;
            int halfWidth = viewport.Width / 2;
            int halfHeight = viewport.Height / 2;
            var topOfScreen = new Rectangle(0, 0, viewport.Width, halfHeight);

            if (IsPressed(Keys.A) || LeftMouseIsPressed(topOfScreen))
            {
                _currentPrimitiveIndex = (_currentPrimitiveIndex + 1) % _primitives.Count;
            }

            // Change color?
            var botLeftOfScreen = new Rectangle(0, halfHeight, halfWidth, halfHeight);
            if (IsPressed(Keys.B) || LeftMouseIsPressed(botLeftOfScreen))
            {
                _currentColorIndex = (_currentColorIndex + 1) % _colors.Count;
            }

            // Toggle wireframe?
            var botRightOfScreen = new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight);
            if (IsPressed(Keys.Y) || LeftMouseIsPressed(botRightOfScreen))
            {
                _isWireframe = !_isWireframe;
            }
        }

        /// <summary>
        ///     Checks whether the specified key or button has been pressed.
        /// </summary>
        private bool IsPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _lastKeyboardState.IsKeyUp(key);
        }

        private bool LeftMouseIsPressed(Rectangle rect)
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _lastMouseState.LeftButton != ButtonState.Pressed &&
                   rect.Contains(_currentMouseState.X, _currentMouseState.Y);
        }
    }
}