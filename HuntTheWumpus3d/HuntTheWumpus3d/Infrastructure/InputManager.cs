using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.InputListeners;
using MonoGame.Extended.ViewportAdapters;

namespace HuntTheWumpus3d.Infrastructure
{
    public class InputManager
    {
        private const float CursorBlinkDelay = 0.5f;
        private static InputManager _instance;
        private static readonly Logger Log = Logger.Instance;
        private float _currentBlinkDelay = CursorBlinkDelay;
        private bool _isCursorVisible = true;
        private string _typedString = string.Empty;

        private InputManager()
        {
        }

        public KeyboardListener KeyListener { get; set; } = new KeyboardListener();
        public MouseListener MouseListener { get; set; } = new MouseListener();

        public static InputManager Instance => _instance ?? (_instance = new InputManager());

        public void PerformOnceOnTypedStringWhen(Func<string, bool> isPredicateTrue, Action<string> action)
        {
            _typedString = string.Empty;
            EventHandler<KeyboardEventArgs> responseParser = null;

            responseParser = (sender, args) =>
            {
                if (args.Key == Keys.Back && _typedString.Length > 0)
                {
                    _typedString = _typedString.Substring(0, _typedString.Length - 1);
                }
                else if (args.Key == Keys.Enter && isPredicateTrue(_typedString))
                {
                    KeyListener.KeyTyped -= responseParser;
                    action(_typedString);
                    _typedString = string.Empty;
                }
                else if (args.Key == Keys.Enter)
                {
                    _typedString = string.Empty;
                }
                else
                {
                    _typedString += ParseArgsToString(args);
                }
            };
            KeyListener.KeyTyped += responseParser;
        }

        private static string ParseArgsToString(KeyboardEventArgs args)
        {
            var c = args.Character;

            // This is a ugly hack. The sprite font I'm using doesn't have a tab character, so when it's
            // entered the Draw method throws an exception. I should create a class like this that's a
            // MonoGame component that is constructed with a IServiceProvider. Resolve the content manager
            // through the IServiceProvider, get the SpriteFont from the content manager and uses its Characters
            // collection to check if the value added to the typed string is a contained in the collection.
            // That way it would work with any Sprite font, but this is just hard coded.
            return c.HasValue && c.Value != '\t' ? c.ToString() : "";
        }

        public void Update(GameTime gameTime)
        {
            _currentBlinkDelay -= (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (_currentBlinkDelay <= 0)
            {
                _isCursorVisible = !_isCursorVisible;
                _currentBlinkDelay = CursorBlinkDelay;
            }
            KeyListener.Update(gameTime);
            MouseListener.Update(gameTime);
        }

        public void Draw(SpriteBatch sb, SpriteFont font, ViewportAdapter va)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: va.GetScaleMatrix());

            const int y = Logger.YPosition + 20;
            const int x = Logger.XPosition;
            var position = new Vector2(x, y);

            sb.DrawString(font, _typedString, position, Color.White);

            if (_isCursorVisible)
                sb.DrawString(font, "_", new Vector2(font.MeasureString(_typedString).X + x, y), Color.White);

            Log.Messages.ForEach(m => sb.DrawString(font, m.Value, m.Position, m.Color));

            sb.End();
        }
    }
}