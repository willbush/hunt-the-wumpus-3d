using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HuntTheWumpus3d.Shapes
{
    public abstract class GeometricShape : IDisposable
    {
        private readonly List<ushort> _indices = new List<ushort>();
        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        private readonly List<VertexPositionNormal> _vertices = new List<VertexPositionNormal>();
        private BasicEffect _basicEffect;
        private IndexBuffer _indexBuffer;

        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        private VertexBuffer _vertexBuffer;
        public Color Color { get; set; } = Color.Black;
        public Vector3 Position { get; internal set; }
        public Matrix Rotation { get; internal set; } = Matrix.Identity;

        /// <summary>
        ///     Queries the index of the current vertex. This starts at
        ///     zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex => _vertices.Count;

        /// <summary>
        ///     Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Adds a new vertex to the primitive model. This should only be called
        ///     during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, Vector3 normal)
        {
            position += Position;
            _vertices.Add(new VertexPositionNormal(position, normal));
        }

        /// <summary>
        ///     Adds a new index to the primitive model. This should only be called
        ///     during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(index));

            _indices.Add((ushort) index);
        }

        /// <summary>
        ///     Once all the geometry has been specified by calling AddVertex and AddIndex,
        ///     this method copies the vertex and index data into GPU format buffers, ready
        ///     for efficient rendering.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            // Create a vertex declaration, describing the format of our vertex data.

            // Create a vertex buffer, and copy our vertex data into it.
            _vertexBuffer = new VertexBuffer(graphicsDevice,
                typeof(VertexPositionNormal),
                _vertices.Count, BufferUsage.None);

            _vertexBuffer.SetData(_vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            _indexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                _indices.Count, BufferUsage.None);

            _indexBuffer.SetData(_indices.ToArray());

            // Create a BasicEffect, which will be used to render the primitive.
            _basicEffect = new BasicEffect(graphicsDevice);

            _basicEffect.EnableDefaultLighting();
        }

        ~GeometricShape()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _basicEffect?.Dispose();
        }

        /// <summary>
        ///     Draws the primitive model, using the specified effect. Unlike the other
        ///     Draw overload where you just specify the world/view/projection matrices
        ///     and color, this method does not set any renderstates, so you must make
        ///     sure all states are set to sensible values before you call it.
        /// </summary>
        public void Draw(Effect effect)
        {
            var graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(_vertexBuffer);

            graphicsDevice.Indices = _indexBuffer;


            foreach (var effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                int primitiveCount = _indices.Count / 3;

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
            }
        }

        /// <summary>
        ///     Draws the primitive model, using a BasicEffect shader with default
        ///     lighting. Unlike the other Draw overload where you specify a custom
        ///     effect, this method sets important renderstates to sensible values
        ///     for 3D model rendering, so you do not need to set these states before
        ///     you call it.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // Set BasicEffect parameters.
            _basicEffect.World = Rotation * world;
            _basicEffect.View = view;
            _basicEffect.Projection = projection;
            _basicEffect.DiffuseColor = Color.ToVector3();
            _basicEffect.Alpha = Color.A / 255.0f;

            var device = _basicEffect.GraphicsDevice;
            device.DepthStencilState = DepthStencilState.Default;

            if (Color.A < 255)
            {
                // Set renderstates for alpha blended rendering.
                device.BlendState = BlendState.AlphaBlend;
            }
            else
            {
                // Set render states for opaque rendering.
                device.BlendState = BlendState.Opaque;
            }

            // Draw the model, using BasicEffect.
            Draw(_basicEffect);
        }
    }
}