using System;

namespace HuntTheWumpus3d
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new WumpusGame())
                game.Run();
        }
    }
}