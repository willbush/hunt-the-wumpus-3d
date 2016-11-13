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
        private readonly Map _map;

        public Player(int roomNumber, Map map) : base(roomNumber)
        {
            _initialRoomNum = roomNumber;
            _map = map;
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
        public void ShootArrow(Wumpus wumpus, Action<EndState> gameOverHandler)
        {
            OnRoomsToTraverseDo(rooms => ShootArrow(rooms, wumpus, gameOverHandler));
        }

        // Requests the player to give the list of rooms they want the arrow to traverse.
        private void OnRoomsToTraverseDo(Action<List<int>> callback)
        {
            Log.Write(Message.RoomNumPrompt);
            _inputManager.PerformOnceOnTypedStringWhen(CanTraverseRooms, s =>
            {
                var rooms = new List<int>();
                s.Trim().Split(' ').ToList().ForEach(r => rooms.Add(int.Parse(r)));
                callback(rooms);
            });
        }

        private bool CanTraverseRooms(string s)
        {
            var roomNumbers = s.Trim().Split(' ');
            if (roomNumbers.Length == 0 || roomNumbers.Length > 5)
            {
                Log.Write("Incorrect number of rooms entered.");
                Log.Write("Must be in range [1, 5]");
                return false;
            }

            var rooms = new List<int> {RoomNumber};
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

        // Traverses the given rooms or randomly selected adjacent rooms if the given rooms are not traversable.
        // Checks if the arrow hit the player, wumpus, or was a miss, and game state is set accordingly.
        private void ShootArrow(IReadOnlyCollection<int> roomsToTraverse, Wumpus wumpus,
            Action<EndState> gameOverHandler)
        {
            --CrookedArrowCount;

            var endState = CheckTargetHit(wumpus, Traverse(roomsToTraverse));

            if (endState != null)
            {
                gameOverHandler(endState);
            }
            else
            {
                Log.Write(Message.Missed);
                Log.Write($"You now have {CrookedArrowCount} crooked arrows.");
                gameOverHandler(CrookedArrowCount == 0
                    ? new EndState(true, $"{Message.OutOfArrows}\n{Message.LoseMessage}")
                    : new EndState());
            }
        }

        private EndState CheckTargetHit(Wumpus wumpus, IEnumerable<int> traversedRoom)
        {
            EndState endState = null;

            foreach (int r in traversedRoom)
            {
                var e = HitTarget(r, wumpus);
                if (e.IsGameOver)
                {
                    endState = e;
                    _map.RoomsArrowTraversed.Add(r);
                    break;
                }
                _map.RoomsArrowTraversed.Add(r);
            }
            return endState;
        }

        private EndState HitTarget(int currentRoom, Wumpus wumpus)
        {
            Log.Write(currentRoom.ToString());

            EndState endState;
            if (RoomNumber == currentRoom)
            {
                endState = new EndState(true, $"{Message.ArrowGotYou}\n{Message.LoseMessage}");
            }
            else if (wumpus.RoomNumber == currentRoom)
            {
                endState = new EndState(true, Message.WinMessage);
                wumpus.IsDiscovered = true;
            }
            else
            {
                endState = new EndState();
            }
            return endState;
        }

        /// <summary>
        ///     Attempts to traverse the requested rooms to traverse, but as soon as one
        ///     requested room is not adjacent to the current room, it starts traversing rooms randomly.
        /// </summary>
        /// <param name="roomsToTraverse"></param>
        /// <returns>the list of rooms that were traversed</returns>
        private List<int> Traverse(IReadOnlyCollection<int> roomsToTraverse)
        {
            int currentRoom = RoomNumber;

            var traversedRooms = roomsToTraverse.TakeWhile(
                nextRoom =>
                {
                    var adjacentRooms = Map.AdjacentTo[currentRoom];
                    if (!adjacentRooms.Contains(nextRoom)) return false;
                    currentRoom = nextRoom;
                    return true;
                }).ToList();

            int numLeftToTraverse = roomsToTraverse.Count - traversedRooms.Count;
            RandomlyTraverse(traversedRooms, numLeftToTraverse);
            return traversedRooms;
        }

        // Adds to the given list of traversed rooms a randomly selected next adjacent room where
        // said selected room is not the previously traversed room (preventing U-turns).
        private void RandomlyTraverse(IList<int> traversedRooms, int numberToTraverse)
        {
            var tuple = GetCurrentPrevious(traversedRooms);
            int currentRoom = tuple.Item1;
            int previousRoom = tuple.Item2;

            // while we need more rooms, get a randomly selected adjacent room that is not the previously traversed room.
            for (int traversed = traversedRooms.Count; traversed < numberToTraverse; ++traversed)
            {
                var rooms = Map.AdjacentTo[currentRoom].Where(r => r != previousRoom).ToArray();
                int nextRoom = rooms.ElementAt(Rand.Next(rooms.Length));

                traversedRooms.Add(nextRoom);
                previousRoom = currentRoom;
                currentRoom = nextRoom;
            }
        }

        private Tuple<int, int> GetCurrentPrevious(IList<int> traversedRooms)
        {
            int currentRoom, previousRoom;

            if (!traversedRooms.Any())
            {
                var rooms = Map.AdjacentTo[RoomNumber];
                int nextRoom = rooms.ElementAt(Rand.Next(rooms.Count));
                traversedRooms.Add(nextRoom);

                currentRoom = nextRoom;
                previousRoom = RoomNumber;
            }
            else if (traversedRooms.Count == 1)
            {
                currentRoom = traversedRooms[traversedRooms.Count - 1];
                previousRoom = RoomNumber;
            }
            else
            {
                currentRoom = traversedRooms[traversedRooms.Count - 1];
                previousRoom = traversedRooms[traversedRooms.Count - 2];
            }
            return new Tuple<int, int>(currentRoom, previousRoom);
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
        public override void Reset()
        {
            RoomNumber = _initialRoomNum;
            CrookedArrowCount = MaxArrows;
        }
    }
}