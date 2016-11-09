using System;
using System.Collections.Generic;
using System.Linq;
using HuntTheWumpus3d.Infrastructure;

namespace HuntTheWumpus3d.Entities
{
    public class Player : GameEntity
    {
        private const int MaxNumberOfArrows = 5;
        private static readonly Logger Log = Logger.Instance;
        private readonly int _initialRoomNum;
        private readonly InputManager _inputManager = InputManager.Instance;

        public Player(int roomNumber) : base(roomNumber)
        {
            _initialRoomNum = roomNumber;
        }

        public int MaxArrows { get; } = MaxNumberOfArrows;
        public int CrookedArrowCount { get; private set; } = MaxNumberOfArrows;

        /// <summary>
        ///     Requests where the player wants to move to, validates the input, and moves the player.
        /// </summary>
        public void Move(Action<Action<EndState>> checkPlayerMovement, Action<EndState> gameOverHandler)
        {
            Log.Write("Where to? ");
            _inputManager.PerformOnceOnTypedStringWhen(IsAdjacent, adjacentRoom =>
            {
                RoomNumber = int.Parse(adjacentRoom);
                checkPlayerMovement(gameOverHandler);
            });
        }

        private bool IsAdjacent(string s)
        {
            int adjacentRoom;
            if (int.TryParse(s, out adjacentRoom) && Map.IsAdjacent(RoomNumber, adjacentRoom)) return true;
            Log.Write("Not Possible - Where to? ");
            return false;
        }

        internal void Move(int roomNumber)
        {
            RoomNumber = roomNumber;
        }

        /// <summary>
        ///     Requests the player how many and what rooms the arrow should traverse.
        ///     The arrow traverses the traversable rooms or randomly selected adjacent ones and the
        ///     arrows traversal path is checked for a hit to determine end game state.
        /// </summary>
        /// <param name="wumpusRoomNumber">current wumpus room number</param>
        /// <param name="gameOverHandler"></param>
        /// <returns>game end state result</returns>
        public void ShootArrow(int wumpusRoomNumber, Action<EndState> gameOverHandler)
        {
            OnNumOfRoomsToTraverseDo(numToTraverse =>
            {
                if (numToTraverse > 0)
                {
                    CrookedArrowCount = CrookedArrowCount - 1;
                    OnRoomsToTraverseDo(numToTraverse, rooms => ShootArrow(rooms, wumpusRoomNumber, gameOverHandler));
                }
                else
                {
                    Log.Write("OK, suit yourself...");
                    gameOverHandler(new EndState());
                }
            });
        }

        // Requests the player to give the list of rooms they want the arrow to traverse.
        private void OnRoomsToTraverseDo(int numOfRooms, Action<List<int>> callback)
        {
            Log.Write(Message.RoomNumPrompt);
            _inputManager.PerformOnceOnTypedStringWhen(s => CanTraverseRooms(s, numOfRooms), s =>
            {
                var rooms = new List<int>();
                s.Split(' ').ToList().ForEach(r => rooms.Add(int.Parse(r)));
                callback(rooms);
            });
        }

        private static bool CanTraverseRooms(string s, int numOfRooms)
        {
            var roomNumbers = s.Split(' ');
            if (roomNumbers.Length != numOfRooms)
            {
                Log.Write($"Incorrect number of rooms entered. Should be: {numOfRooms}");
                return false;
            }

            var rooms = new List<int>();
            foreach (string r in roomNumbers)
            {
                int roomNumber;
                if (!int.TryParse(r, out roomNumber) || roomNumber < 0 || roomNumber > Map.NumOfRooms)
                {
                    Log.Write("Bad number - try again:");
                    return false;
                }
                if (IsTooCrooked(roomNumber, rooms))
                {
                    Log.Write(Message.TooCrooked);
                    return false;
                }
                rooms.Add(roomNumber);
            }
            return true;
        }

        //Requests from the player how many rooms they want the arrow they're shooting to traverse.
        private void OnNumOfRoomsToTraverseDo(Action<int> callback)
        {
            Log.Write(Message.NumOfRoomsToShootPrompt);
            _inputManager.PerformOnceOnTypedStringWhen(IsNumWithinBounds, s => { callback(int.Parse(s)); });
        }

        private static bool IsNumWithinBounds(string s)
        {
            const int lowerBound = 0;
            const int upperBound = 5;
            int n;
            return int.TryParse(s, out n) && n >= lowerBound && n <= upperBound;
        }

        // Traverses the given rooms or randomly selected adjacent rooms if the given rooms are not traversable.
        // Checks if the arrow hit the player, wumpus, or was a miss, and game state is set accordingly.
        private void ShootArrow(IReadOnlyCollection<int> roomsToTraverse, int wumpusRoomNum,
            Action<EndState> gameOverHandler)
        {
            var endstate = Traverse(roomsToTraverse).Select(r => HitTarget(r, wumpusRoomNum))
                .FirstOrDefault(e => e.IsGameOver);

            if (endstate != null) gameOverHandler(endstate);

            Log.Write(Message.Missed);
            gameOverHandler(CrookedArrowCount == 0
                ? new EndState(true, $"{Message.OutOfArrows}\n{Message.LoseMessage}")
                : new EndState());
        }

        private EndState HitTarget(int currentRoom, int wumpusRoomNum)
        {
            Log.Write(currentRoom.ToString());
            EndState endState;
            if (RoomNumber == currentRoom)
            {
                endState = new EndState(true, $"{Message.ArrowGotYou}\n{Message.LoseMessage}");
            }
            else if (wumpusRoomNum == currentRoom)
            {
                endState = new EndState(true, Message.WinMessage);
            }
            else
            {
                endState = new EndState();
            }
            return endState;
        }

        // Attempts to traverse the requested rooms to traverse, but as soon as one
        // requested room is not adjacent to the current room, it starts traversing rooms randomly.
        private IEnumerable<int> Traverse(IReadOnlyCollection<int> roomsToTraverse)
        {
            int currentRoom = RoomNumber;

            ICollection<int> traversedRooms = roomsToTraverse.TakeWhile(
                nextRoom =>
                {
                    var adjacentRooms = Map.AdjacentTo[currentRoom];
                    if (!adjacentRooms.Contains(nextRoom)) return false;
                    currentRoom = nextRoom;
                    return true;
                }).ToList();

            int numLeftToTraverse = roomsToTraverse.Count - traversedRooms.Count;
            RandomlyTraverse(traversedRooms, currentRoom, numLeftToTraverse);
            return traversedRooms;
        }

        // Adds to the given list of traversed rooms a randomly selected next adjacent room where
        // said selected room is not the previously traversed room (preventing U-turns).
        private static void RandomlyTraverse(
            ICollection<int> traversedRooms,
            int currentRoom,
            int numberToTraverse)
        {
            int previousRoom;

            // if no traversed rooms, randomly select an adjacent next room and set the previous to the room the player is in.
            if (!traversedRooms.Any())
            {
                var rooms = Map.AdjacentTo[currentRoom];
                int firstRoom = rooms.ElementAt(new Random().Next(rooms.Count));

                previousRoom = currentRoom;
                currentRoom = firstRoom;
            }
            else
            {
                previousRoom = currentRoom;
            }

            // while we need more rooms, get a randomly selected adjacent room that is not the previously traversed room.
            for (var traversed = 0; traversed < numberToTraverse; ++traversed)
            {
                var rooms = Map.AdjacentTo[currentRoom].Where(r => r != previousRoom).ToArray();
                int nextRoom = rooms.ElementAt(new Random().Next(rooms.Length));

                traversedRooms.Add(currentRoom);
                previousRoom = currentRoom;
                currentRoom = nextRoom;
            }
        }

        // A requested room number is too crooked for an arrow to go into when:
        // The requested room is the same as the previously requested room
        // (essentially asking the arrow to stay in the same room).
        // The requested room is the same as request before last.
        // (essentially asking for the arrow to make a U-turn).
        private static bool IsTooCrooked(int roomNumber, IReadOnlyList<int> rooms)
        {
            return (rooms.Count > 0 && rooms.Last() == roomNumber) ||
                   (rooms.Count > 1 && rooms[rooms.Count - 2] == roomNumber);
        }

        public override void PrintLocation()
        {
            Log.Write($"You are in room {RoomNumber}");
            Map.PrintAdjacentRoomNumbers(RoomNumber);
        }

        /// <summary>
        ///     Resets player to initial state.
        /// </summary>
        public void Reset()
        {
            RoomNumber = _initialRoomNum;
            CrookedArrowCount = MaxArrows;
        }
    }
}