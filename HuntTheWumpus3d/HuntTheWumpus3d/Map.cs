using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuntTheWumpus3d.Entities;
using HuntTheWumpus3d.Infrastructure;
using HuntTheWumpus3d.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HuntTheWumpus3d
{
    public class Map
    {
        public const int NumOfRooms = 20;
        private static readonly Logger Logger = Logger.Instance;
        private readonly List<DeadlyHazard> _deadlyHazards;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<Hazard> _hazards;
        private readonly GeometricShape[] _rooms = new GeometricShape[20];
        private readonly HashSet<int> _roomsWithStaticHazards;
        private readonly List<SuperBats> _superBats;
        private float _arrowShotLerpAmount;
        private float _playerLerpAmount;
        private float _shotAnimationTime = 1f;

        public Map(GraphicsDevice graphicsDevice, bool isCheatMode = false)
        {
            _graphicsDevice = graphicsDevice;
            IsCheatMode = isCheatMode;
            CreateRooms();
            var occupiedRooms = new HashSet<int>();

            Player = new Player(GetRandomAvailableRoom(occupiedRooms), this);
            Wumpus = new Wumpus(GetRandomAvailableRoom(occupiedRooms));

            var bottomlessPit1 = new BottomlessPit(GetRandomAvailableRoom(occupiedRooms));
            var bottomlessPit2 = new BottomlessPit(GetRandomAvailableRoom(occupiedRooms));
            var superbats1 = new SuperBats(GetRandomAvailableRoom(occupiedRooms));
            var superbats2 = new SuperBats(GetRandomAvailableRoom(occupiedRooms));

            _hazards = new List<Hazard> {Wumpus, bottomlessPit1, bottomlessPit2, superbats1, superbats2};
            _deadlyHazards = new List<DeadlyHazard> {Wumpus, bottomlessPit1, bottomlessPit2};
            _superBats = new List<SuperBats> {superbats1, superbats2};

            _roomsWithStaticHazards = new HashSet<int>
            {
                superbats1.RoomNumber,
                superbats2.RoomNumber,
                bottomlessPit1.RoomNumber,
                bottomlessPit2.RoomNumber
            };

            if (isCheatMode)
                PrintHazards();
        }

        public List<int> RoomsArrowTraversed { get; set; } = new List<int>();

        public bool IsCheatMode { get; set; }
        public Player Player { get; }
        private Wumpus Wumpus { get; }

        // Each key is the room number and its value is the set of adjacent rooms.
        // A dictionary of hash sets is definitely overkill given the constant number of elements, 
        // but with it comes a lot of Linq expression convenience. 
        internal static Dictionary<int, HashSet<int>> AdjacentTo { get; } = new Dictionary<int, HashSet<int>>
        {
            {1, new HashSet<int> {2, 5, 8}},
            {2, new HashSet<int> {1, 3, 10}},
            {3, new HashSet<int> {2, 4, 12}},
            {4, new HashSet<int> {3, 5, 14}},
            {5, new HashSet<int> {1, 4, 6}},
            {6, new HashSet<int> {5, 7, 15}},
            {7, new HashSet<int> {6, 8, 17}},
            {8, new HashSet<int> {1, 7, 9}},
            {9, new HashSet<int> {8, 10, 18}},
            {10, new HashSet<int> {2, 9, 11}},
            {11, new HashSet<int> {10, 12, 19}},
            {12, new HashSet<int> {3, 11, 13}},
            {13, new HashSet<int> {12, 14, 20}},
            {14, new HashSet<int> {4, 13, 15}},
            {15, new HashSet<int> {6, 14, 16}},
            {16, new HashSet<int> {15, 17, 20}},
            {17, new HashSet<int> {7, 16, 18}},
            {18, new HashSet<int> {9, 17, 19}},
            {19, new HashSet<int> {11, 18, 20}},
            {20, new HashSet<int> {13, 16, 19}}
        };

        private void CreateRooms()
        {
            var vertices = BuildDodecahedron();
            var spheres = new List<Sphere>();
            vertices.ForEach(v => spheres.Add(new Sphere(_graphicsDevice, v)));
            var sphereCreationOrderToRoomNumber = new Dictionary<int, int>
            {
                {1, 1},
                {2, 7},
                {3, 9},
                {4, 8},
                {5, 18},
                {6, 19},
                {7, 17},
                {8, 15},
                {9, 6},
                {10, 16},
                {11, 5},
                {12, 3},
                {13, 10},
                {14, 2},
                {15, 11},
                {16, 20},
                {17, 12},
                {18, 14},
                {19, 4},
                {20, 13}
            };

            for (var i = 0; i < spheres.Count; ++i)
                _rooms[sphereCreationOrderToRoomNumber[i + 1] - 1] = spheres[i];
        }

        /// <summary>
        ///     Generates a list of vertices (in arbitrary order) for a tetrahedron centered on the origin.
        /// </summary>
        /// <returns></returns>
        private static List<Vector3> BuildDodecahedron()
        {
            var r = (float) Math.Sqrt(3);
            float phi = (float) (Math.Sqrt(5) - 1) / 2; // The golden ratio

            var a = (float) (1 / Math.Sqrt(3));
            float b = a / phi;
            float c = a * phi;

            var vertices = new List<Vector3>();
            var plusOrMinus = new[] {-1, 1};

            foreach (int i in plusOrMinus)
            {
                foreach (int j in plusOrMinus)
                {
                    vertices.Add(new Vector3(0, i * c * r, j * b * r));
                    vertices.Add(new Vector3(i * b * r, 0, j * c * r));
                    vertices.Add(new Vector3(i * c * r, j * b * r, 0));
                    vertices.AddRange(plusOrMinus.Select(k => new Vector3(i * a * r, j * a * r, k * a * r)));
                }
            }
            return vertices;
        }

        public void Update(GameTime time)
        {
            ResetRoomColors();
            ColorAdjacentRoomsToPlayer();
            UpdateHazardColors();
            UpdatePlayerColor(time);
            UpdateAnyShotRoomColors(time);
        }

        private void ResetRoomColors()
        {
            foreach (var r in _rooms)
            {
                r.Color = Color.White;
            }
        }

        private void ColorAdjacentRoomsToPlayer()
        {
            foreach (int i in AdjacentTo[Player.RoomNumber])
                _rooms[i - 1].Color = Color.LightBlue;
        }

        private void UpdateHazardColors()
        {
            if (IsCheatMode)
            {
                _hazards.ForEach(h => _rooms[h.RoomNumber - 1].Color = h.EntityColor);
            }
            else
            {
                _hazards.ForEach(h =>
                {
                    if (h.IsDiscovered)
                        _rooms[h.RoomNumber - 1].Color = h.EntityColor;
                });
            }
        }

        private void UpdatePlayerColor(GameTime time)
        {
            _playerLerpAmount = (float) (Math.Sin(time.TotalGameTime.TotalSeconds * 5) * 0.5) + 0.5f;
            var currentRoomColor = _rooms[Player.RoomNumber - 1].Color;
            _rooms[Player.RoomNumber - 1].Color = Color.Lerp(Color.Blue, currentRoomColor, _playerLerpAmount);
        }

        private void UpdateAnyShotRoomColors(GameTime time)
        {
            if (RoomsArrowTraversed.Any() && _shotAnimationTime > 0)
            {
                _arrowShotLerpAmount = (float) (Math.Sin(time.TotalGameTime.TotalSeconds * 5) * 0.5) + 0.5f;

                foreach (int r in RoomsArrowTraversed)
                {
                    var room = _rooms[r - 1];
                    room.Color = Color.Lerp(room.Color, Color.Red, _arrowShotLerpAmount);
                }
                _shotAnimationTime -= 0.002f;
            }
            else
            {
                RoomsArrowTraversed.Clear();
                _shotAnimationTime = 1.0f;
            }
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            _rooms.ToList().ForEach(r => r.Draw(world, view, projection));
        }

        /// <summary>
        ///     Reset map state to its initial state.
        /// </summary>
        public void Reset()
        {
            Player.Reset();
            _hazards.ForEach(h => h.Reset());
            RoomsArrowTraversed.Clear();

            if (IsCheatMode)
                PrintHazards();
        }

        /// <summary>
        ///     Gets a random available room that's not a member of the give4n occupied rooms set.
        /// </summary>
        private static int GetRandomAvailableRoom(ISet<int> occupiedRooms)
        {
            var availableRooms = Enumerable.Range(1, NumOfRooms).Where(r => !occupiedRooms.Contains(r)).ToArray();
            if (availableRooms.Length == 0)
                throw new InvalidOperationException("All rooms are already occupied.");

            int index = Rand.Next(0, availableRooms.Length);
            int unoccupiedRoom = availableRooms[index];
            occupiedRooms.Add(unoccupiedRoom);
            return unoccupiedRoom;
        }

        public static int GetAnyRandomRoomNumber()
        {
            return Rand.Next(1, NumOfRooms + 1); // Random number in range [1, 20]
        }

        /// <summary>
        ///     Updates the state of the game on the map.
        /// </summary>
        public EndState TakeTurn()
        {
            Logger.Write("");
            var es = Wumpus.TakeTurn(this);

            if (!es.IsGameOver)
            {
                var roomsAdjacentToPlayer = AdjacentTo[Player.RoomNumber];
                _hazards.ForEach(h =>
                {
                    if (roomsAdjacentToPlayer.Contains(h.RoomNumber))
                        h.PrintHazardWarning();
                });
                Player.PrintLocation();
            }
            return es;
        }

        /// <summary>
        ///     Gets a room number that is adjacent to the given number that's contains no hazards.
        /// </summary>
        public int GetSafeRoomNextTo(int roomNumber)
        {
            var safeAdjacentRooms = AdjacentTo[roomNumber].Except(_roomsWithStaticHazards).ToArray();
            return safeAdjacentRooms.ElementAt(Rand.Next(safeAdjacentRooms.Length));
        }

        public void PlayerShootArrow(Action<EndState> gameOverHandler)
        {
            // Clear just in case the player is playing fast before the last shoot animation ended.
            RoomsArrowTraversed.Clear();
            Player.ShootArrow(Wumpus, gameOverHandler);
        }

        public void MovePlayer(Action<EndState> gameOverHandler)
        {
            Player.Move(CheckPlayerMovement, gameOverHandler);
        }

        // Game is over if the player moves into a deadly room.
        // If the game's not over but the power got snatched, then loop and check again until the player
        // doesn't get snatched or gets killed.
        private void CheckPlayerMovement(Action<EndState> callback)
        {
            EndState endState;
            do
            {
                endState = _deadlyHazards
                    .Select(h => h.DetermineEndState(Player.RoomNumber))
                    .FirstOrDefault(s => s.IsGameOver) ?? new EndState();
            } while (!endState.IsGameOver && _superBats.Any(b => b.TrySnatch(Player)));

            callback(endState);
        }

        public static void PrintAdjacentRoomNumbers(int roomNum)
        {
            var sb = new StringBuilder();
            foreach (int room in AdjacentTo[roomNum])
                sb.Append(room + " ");

            Logger.Write($"Tunnels lead to {sb}");
        }

        public static bool IsAdjacent(int currentRoom, int adjacentRoom)
        {
            return AdjacentTo[currentRoom].Contains(adjacentRoom);
        }

        public void PrintHazards()
        {
            Logger.Write("");
            _hazards.ForEach(h => h.PrintLocation());
        }
    }
}