using System;

namespace HuntTheWumpus3d
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] arguments)
        {
            bool isCheatMode = arguments.Length > 0 && arguments[0].ToLower() == "cheat";

            using (var game = new WumpusGame(isCheatMode))
                game.Run();
        }
    }
}