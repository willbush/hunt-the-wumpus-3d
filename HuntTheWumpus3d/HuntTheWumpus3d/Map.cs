using System;
using System.Collections.Generic;
using System.Linq;
using HuntTheWumpus3d.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HuntTheWumpus3d
{
    public class Map
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly GeometricShape[] _rooms = new GeometricShape[20];

        public Map(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public void LoadContent()
        {
            var vertices = BuildDodecahedron();
            var spheres = new List<Sphere>();
            vertices.ForEach(v => spheres.Add(new Sphere(_graphicsDevice, v)));
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

            for (var i = 0; i < spheres.Count; ++i)
                _rooms[sphereCreationOrderToRoomNumber[i + 1] - 1] = spheres[i];
        }

        /// <summary>
        ///     Generates a list of vertices (in arbitrary order) for a tetrahedron centered on the origin.
        /// </summary>
        /// <returns></returns>
        private static List<Vector3> BuildDodecahedron()
        {
            var r = (float) Math.Sqrt(3);
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

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            _rooms.ToList().ForEach(r => r.Draw(world, view, projection));
        }
    }
}