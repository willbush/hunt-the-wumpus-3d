using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HuntTheWumpus3d.Shapes
{
    public class Sphere : GeometricShape
    {
        private const float Diameter = 0.55f;
        private const int Tessellation = 16;

        /// <summary>
        ///     Constructs a new sphere primitive,
        ///     with the specified size and tessellation level.
        /// </summary>
        public Sphere(GraphicsDevice graphicsDevice, Vector3 position = new Vector3())
        {
            Position = position;

            const int verticalSegments = Tessellation;
            const int horizontalSegments = Tessellation * 2;
            const float radius = Diameter / 2;

            // Start with a single vertex at the bottom of the sphere.
            AddVertex(Vector3.Down * radius, Vector3.Down);

            // Create rings of vertices at progressively higher latitudes.
            for (var i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = (i + 1) * MathHelper.Pi /
                                 verticalSegments - MathHelper.PiOver2;

                var dy = (float) Math.Sin(latitude);
                var dxz = (float) Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (var j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float) Math.Cos(longitude) * dxz;
                    float dz = (float) Math.Sin(longitude) * dxz;

                    var normal = new Vector3(dx, dy, dz);

                    AddVertex(normal * radius, normal);
                }
            }

            // Finish with a single vertex at the top of the sphere.
            AddVertex(Vector3.Up * radius, Vector3.Up);

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (var i = 0; i < horizontalSegments; i++)
            {
                AddIndex(0);
                AddIndex(1 + (i + 1) % horizontalSegments);
                AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (var i = 0; i < verticalSegments - 2; i++)
            {
                for (var j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                }
            }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (var i = 0; i < horizontalSegments; i++)
            {
                AddIndex(CurrentVertex - 1);
                AddIndex(CurrentVertex - 2 - (i + 1) % horizontalSegments);
                AddIndex(CurrentVertex - 2 - i);
            }
            InitializePrimitive(graphicsDevice);
        }
    }
}