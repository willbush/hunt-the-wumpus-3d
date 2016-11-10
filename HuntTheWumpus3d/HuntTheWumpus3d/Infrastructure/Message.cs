namespace HuntTheWumpus3d.Infrastructure
{
    public class Message
    {
        public const string ActionPrompt = "Shoot, Move or Quit(S - M - Q)? ";
        public const string PlayPrompt = "Play again? (Y-N)";
        public const string SetupPrompt = "Same Setup? (Y-N)";
        public const string NumOfRoomsToShootPrompt = "No. or rooms (0-5 and Enter)?";
        public const string RoomNumPrompt = "Enter a space separated list of rooms (e.g. 1 2 3 4 5)";

        public const string PitWarning = "I feel a draft!";
        public const string WumpusWarning = "I Smell a Wumpus.";
        public const string BatWarning = "Bats nearby!";

        public const string BatSnatch = "Zap--Super Bat snatch! Elsewhereville for you!";
        public const string WumpusBump = "...Oops! Bumped a wumpus!";

        public const string OutOfArrows = "You've run out of arrows!";
        public const string ArrowGotYou = "Ouch! Arrow got you!";
        public const string Missed = "Missed!";
        public const string TooCrooked = "Arrows aren't that crooked - try another room!";

        public const string FellInPit = "YYYIIIIEEEE... fell in a pit!";
        public const string WumpusGotYou = "Tsk tsk tsk - wumpus got you!";
        public const string LoseMessage = "Ha ha ha - you lose!";
        public const string WinMessage = "Aha! You got the Wumpus!\nHee hee hee - the Wumpus'll getcha next time!!";
    }
}