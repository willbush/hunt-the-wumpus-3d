﻿using System;

namespace HuntTheWumpus3d
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (var game = new WumpusGame())
                game.Run();
        }
    }
}