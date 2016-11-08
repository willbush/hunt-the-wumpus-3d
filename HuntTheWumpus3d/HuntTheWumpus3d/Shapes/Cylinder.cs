using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HuntTheWumpus3d.Shapes
{
    public class Cylinder : GeometricShape
    {
        private const float Diameter = 0.2f;
        private const int Tessellation = 32;

        /// <summary>
        ///     Constructs a new cylinder primitive,
        ///     with the specified size and tessellation level.
        /// </summary>
        public Cylinder(GraphicsDevice graphicsDevice, Vector3 p1, Vector3 p2)
        {
            float height = Vector3.Distance(p1, p2);
            height /= 2;

            float radius = Diameter / 2;

            // Create a ring of triangles around the outside of the cylinder.
            for (var i = 0; i < Tessellation; i++)
            {
                var normal = GetCircleVector(i, Tessellation);

                AddVertex(normal * radius + p1 * height, normal);
                AddVertex(normal * radius + p2 * height, normal);

                AddIndex(i * 2);
                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 2) % (Tessellation * 2));

                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 3) % (Tessellation * 2));
                AddIndex((i * 2 + 2) % (Tessellation * 2));
            }

            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(Tessellation, height, radius, p1);
            CreateCap(Tessellation, height, radius, p2);

            InitializePrimitive(graphicsDevice);
        }

        /// <summary>
        ///     Helper method creates a triangle fan to close the ends of the cylinder.
        /// </summary>
        private void CreateCap(int tessellation, float height, float radius, Vector3 normal)
        {
            // Create cap indices.
            for (var i = 0; i < tessellation - 2; i++)
            {
                if (normal.Y > 0)
                {
                    AddIndex(CurrentVertex);
                    AddIndex(CurrentVertex + (i + 1) % tessellation);
                    AddIndex(CurrentVertex + (i + 2) % tessellation);
                }
                else
                {
                    AddIndex(CurrentVertex);
                    AddIndex(CurrentVertex + (i + 2) % tessellation);
                    AddIndex(CurrentVertex + (i + 1) % tessellation);
                }
            }

            // Create cap vertices.
            for (var i = 0; i < tessellation; i++)
            {
                var position = GetCircleVector(i, tessellation) * radius + normal * height;
                AddVertex(position, normal);
            }
        }

        /// <summary>
        ///     Helper method computes a point on a circle.
        /// </summary>
        private static Vector3 GetCircleVector(int i, int tessellation)
        {
            float angle = i * MathHelper.TwoPi / tessellation;

            var dx = (float) Math.Cos(angle);
            var dz = (float) Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
    }
}