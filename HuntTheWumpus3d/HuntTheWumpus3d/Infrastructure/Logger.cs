using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HuntTheWumpus3d.Infrastructure
{
    internal class Logger
    {
        private const int MessageLimit = 25;
        private const int MessageOffset = 20;
        private const int XPosition = 570;
        private const int YPosition = 480;
        private static Logger _instance;

        private Logger()
        {
            Messages = new List<Message>();
        }

        internal List<Message> Messages { get; }

        public static Logger Instance => _instance ?? (_instance = new Logger());

        public void Write(string message, Color color = default(Color))
        {
            if (color == default(Color))
                color = Color.White;

            if (Messages.Count >= MessageLimit)
                Messages.RemoveAt(0);

            Messages.ForEach(m => m.Position.Y -= MessageOffset);

            Messages.Add(new Message(message, new Vector2(XPosition, YPosition), color));
        }

        internal class Message
        {
            public readonly Color Color;
            public readonly string Value;
            public Vector2 Position;

            internal Message(string value, Vector2 position, Color color)
            {
                Value = value;
                Position = position;
                Color = color;
            }
        }
    }
}