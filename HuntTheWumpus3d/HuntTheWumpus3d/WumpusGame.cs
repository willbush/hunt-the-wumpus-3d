using System;
using System.Collections.Generic;
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
            Color.Green,
            Color.Blue,
            Color.White,
            Color.Red,
            Color.Yellow,
            Color.Violet,
        };

        // Store a list of primitive models, plus which one is currently selected.
        private readonly GeometricShape[] _rooms = new GeometricShape[20];
        private Vector3 _cameraPosition;

        private int _currentColorIndex;

        private KeyboardState _currentKeyboardState;
        private MouseState _currentMouseState;

        // Are we rendering in wireframe mode?
        private bool _isWireFrame;
        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
        private Matrix _projection;
        private int _roomIndex;
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
            var vertices = BuildDodecahedron();
            var spheres = new List<Sphere>();
            vertices.ForEach(v => spheres.Add(new Sphere(GraphicsDevice, v)));
            var sphereCreationOrderToRoomNumber = new Dictionary<int, int>
            {
                {1, 1},
                {2, 7},
                {3, 9},
                {4, 8},
                {5, 18},
                {6, 19},
                {7, 17},
                {8, 15},
                {9, 6},
                {10, 16},
                {11, 5},
                {12, 3},
                {13, 10},
                {14, 2},
                {15, 11},
                {16, 20},
                {17, 12},
                {18, 14},
                {19, 4},
                {20, 13}
            };

            for (int i = 0; i < spheres.Count; ++i)
                _rooms[sphereCreationOrderToRoomNumber[i + 1] - 1] = spheres[i];

            _wireFrameState = new RasterizerState
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.None
            };
        }

        /// <summary>
        ///     Generates a list of vertices (in arbitrary order) for a tetrahedron centered on the origin.
        /// </summary>
        /// <returns></returns>
        private static List<Vector3> BuildDodecahedron()
        {
            var r = (float) Math.Sqrt(5);
            float phi = (float) (Math.Sqrt(5) - 1) / 2; // The golden ratio

            var a = (float) (1 / Math.Sqrt(3));
            float b = a / phi;
            float c = a * phi;

            var vertices = new List<Vector3>();
            var plusOrMinus = new[] {-1, 1};

            foreach (int i in plusOrMinus)
            {
                foreach (int j in plusOrMinus)
                {
                    vertices.Add(new Vector3(0, i * c * r, j * b * r));
                    vertices.Add(new Vector3(i * b * r, 0, j * c * r));
                    vertices.Add(new Vector3(i * c * r, j * b * r, 0));
                    vertices.AddRange(plusOrMinus.Select(k => new Vector3(i * a * r, j * a * r, k * a * r)));
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

            //// Draw the current primitive.
            var color = _colors[_currentColorIndex];

            _rooms.ToList().ForEach(r =>
            {
//                r.Color = color;
                r.Draw(_world, _view, _projection);
            });

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
                Exit();

            // Change primitive?
            var viewport = GraphicsDevice.Viewport;

            // Change color?
            if (IsPressed(Keys.R))
                _currentColorIndex = (_currentColorIndex + 1) % _colors.Count;

            // Toggle wireframe?
            if (IsPressed(Keys.E))
                _isWireFrame = !_isWireFrame;

            if (IsPressed(Keys.F))
            {
                if (_roomIndex < _rooms.Length)
                {
                    _rooms[_roomIndex++].Color = _colors[_currentColorIndex];
                    _currentColorIndex = (_currentColorIndex + 1) % _colors.Count;
                }
                else
                {
                    _rooms.ToList().ForEach(r => r.Color = Color.Black);
                    _roomIndex = 0;
                }
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