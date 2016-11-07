using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private readonly List<GeometricShape> _hallways = new List<GeometricShape>();

        // Store a list of primitive models, plus which one is currently selected.
        private readonly List<GeometricShape> _rooms = new List<GeometricShape>();
        private Vector3 _cameraPosition;

        private int _currentColorIndex;

        private KeyboardState _currentKeyboardState;
        private MouseState _currentMouseState;

        // Are we rendering in wireframe mode?
        private bool _isWireFrame;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
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
            _world = new Matrix();
            _cameraPosition = new Vector3(0, 0, 5.5f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            var r = (float) Math.Sqrt(5);
            MakeDodecahedron(r).ForEach(v => { _rooms.Add(new Sphere(GraphicsDevice, v)); });

            _wireFrameState = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
        }

        /// <summary>
        /// Generates a list of vertices (in arbitrary order) for a tetrahedron centered on the origin.
        /// </summary>
        /// <param name="r">The distance of each vertex from origin.</param>
        /// <returns></returns>
        private static List<Vector3> MakeDodecahedron(float r)
        {
            // Calculate constants that will be used to generate vertices
            float phi = (float) (Math.Sqrt(5) - 1) / 2; // The golden ratio

            var a = (float) (1 / Math.Sqrt(3));
            float b = a / phi;
            float c = a * phi;

            // Generate each vertex
            var vertices = new List<Vector3>();
            foreach (int i in new[] {-1, 1})
            {
                foreach (int j in new[] {-1, 1})
                {
                    vertices.Add(new Vector3(0, i * c * r, j * b * r));
                    vertices.Add(new Vector3(i * c * r, j * b * r, 0));
                    vertices.Add(new Vector3(i * b * r, 0, j * c * r));
                    vertices.AddRange(new[] {-1, 1}.Select(k => new Vector3(i * a * r, j * a * r, k * a * r)));
                }
            }
            return vertices;
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
            var m = Matrix.CreateRotationX(MathHelper.ToRadians(40f));

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            _view = Matrix.CreateLookAt(_cameraPosition, Vector3.Zero, Vector3.Up);
            _projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the current primitive.
            var color = _colors[_currentColorIndex];

            _rooms.ForEach(r => r.Draw(_world, _view, _projection, color));
            _hallways.ForEach(r => r.Draw(_world, _view, _projection, color));

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
                _isWireFrame = !_isWireFrame;
            }
        }

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