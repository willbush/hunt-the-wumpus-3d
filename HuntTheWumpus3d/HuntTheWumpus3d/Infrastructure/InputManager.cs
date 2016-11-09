using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.InputListeners;

namespace HuntTheWumpus3d.Infrastructure
{
    public class InputManager
    {
        private static InputManager _instance;
        private string _typedString;

        private InputManager()
        {
        }

        public KeyboardListener KeyListener { get; set; } = new KeyboardListener();

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
                    _typedString += args.Character?.ToString() ?? "";
                }
            };
            KeyListener.KeyTyped += responseParser;
        }

        public void Update(GameTime gameTime)
        {
            KeyListener.Update(gameTime);
        }
    }
}