using System.Collections.Generic;
using System;

namespace Hex
{
    /*game logic
     * max units on tile = 99
     * max morale = 99
     * max moves per turn = 5
     *  
     * 
     * 
     * */



    /// <summary>
    /// Class running complete "abstract" game logic of each game session. Includes everything from map creation to turn resolution and game rules.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    public class GameSession
    {


        /// <summary>
        /// Game board, each tile represents one tile in game.
        /// </summary>
        /// <remarks>
        /// Origin (0, 0) is in lower left corner, x coordinate increases to right and y up. Odd columns are raised, meaning (1, 0) is above-right to (0, 0).
        /// </remarks>
        public Tile[,] Tiles { get; private set; }

        public Tile GetTile(Tuple<int, int> tile)
        {
            return Tiles[tile.Item1, tile.Item2];
        }

        /// <summary>
        /// Horizontal size of game board.
        /// </summary>
        public int SizeX { get { return _sizeX; } }

        /// <summary>
        /// Vertical size of game board.
        /// </summary>
        public int SizeY { get { return _sizeY; } }
        public Faction[] Factions { get; private set; }

        private readonly int _sizeX;
        private readonly int _sizeY;


        #region game parameters
        public readonly int MaxUnitsPerTile = 99;
        public readonly int MaxMorale = 99;
        public int MovesPerTurn { get; private set; }
        public readonly int UnitSpeed = 2;
        #endregion


        public Faction CurrentlyPlaingFaction { get { return Factions[currentlyPlayingFaction]; } }

        /// <summary>
        /// Counts turns from beginning of the game. Turn is understood as number of times each player played (but game begins with turn 1).
        /// </summary>
        public int CurrentTurn { get; private set; }

        /// <summary>
        /// Number of moves remaining for current player this turn.
        /// </summary>
        public int NumberOfMovesRemaining { get; private set; }

        private int currentlyPlayingFaction;


        public GameSession(int sizeX, int sizeY, int numOfFactions)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            Tiles = new Tile[sizeX, sizeY];
            Factions = Faction.CreateFactions(numOfFactions);
            currentlyPlayingFaction = 1;
            CurrentTurn = 1;

            MovesPerTurn = 5;

        }

        public Tile this[int x, int y]
        {
            get { return Tiles[x, y]; }
        }

        public void MakeMove(Move move)
        {


            NumberOfMovesRemaining--;
            if (NumberOfMovesRemaining == 0)
            {
                currentlyPlayingFaction = currentlyPlayingFaction % Factions.Length;
                if (currentlyPlayingFaction == 0) currentlyPlayingFaction++;
                NumberOfMovesRemaining = MovesPerTurn;
            }
        }

        /// <summary>
        /// Checks if a move is currently legal, meaning if there is unit controlled by faction active right now on given tile and if target tile is reachable by that unit this turn.
        /// </summary>
        public bool IsMoveValid(Move move, out List<Tuple<Tuple<int, int>, int>> range)
        {
            range = null;

            if (GetTile(move.FromTile).OccupiedByFaction.ID != currentlyPlayingFaction)
                return false;

            if (GetTile(move.FromTile).UnitOnTile.UnitMovedThisTurn == true)
                return false;

            if (UnitSpeed != 2)
                throw new NotImplementedException("Move checking implemented only for unit speed 2 at the moment");

            if (Distance(move) > UnitSpeed)
                return false;

            return false;
        }

        /// <summary>
        /// Measures distance between two tiles on hexagonal grid, ignoring terrain.
        /// </summary>
        public static int Distance(Move move)
        {
            //uses axial to cubic coordinates transform, see http://www.redblobgames.com/grids/hexagons/ for reference.
            int x1 = move.FromY;
            int z1 = move.FromX - (move.FromY - Math.Abs(move.FromY % 2)) / 2;
            int y1 = -x1 - z1;

            int x2 = move.ToY;
            int z2 = move.ToX - (move.ToY - Math.Abs(move.ToY % 2)) / 2;
            int y2 = -x2 - z2;

            return (Math.Abs(x1 - x2) + Math.Abs(y1 - y2) + Math.Abs(z1 - z2)) / 2;

        }

        /// <summary>
        /// Computes list of tiles reachable tiles with given amount of moves in one turn. Takes into account terrain, enemy units and all other game rules.
        /// </summary>
        /// <param name="from">Starting tile</param>
        /// <param name="numberOfMoves">How big radius should be</param>
        /// <remarks>Resulting list does not include starting tile. Includes tiles that need multiple moves to enter and are not "fully" entered yet when moves run out.</remarks>
        /// <returns>Returns tuples in format ((x, y), moves needed to reach there) </returns>
        public List<Tuple<Tuple<int, int>, int>> ReachableTilesThisTurn(Tuple<int, int> from, int numberOfMoves)
        {
            //flood like algorithm

            List<Tuple<Tuple<int, int>, int>> result = new List<Tuple<Tuple<int, int>, int>>(18);

            if (numberOfMoves <= 0)
                return result;

            //processed tiles, should never be visited again
            List<Tuple<int, int>> visited = new List<Tuple<int, int>>(1);
            //how many moves to get there
            List<int> visitedMovesNeeded = new List<int>(1);

            //for optimalization. processed but can be visited from active tiles.
            List<Tuple<int, int>> oldFringe = new List<Tuple<int, int>>(6);
            List<int> oldFringeMovesNeeded = new List<int>(6);

            List<Tuple<int, int>> turnEndingTileVisited = new List<Tuple<int, int>>(3);
            List<int> turnEndingTileVisitedMovesNeeded = new List<int>(3);

            //reachable but through end turning interface. singled out because tile could be reached other way later.
            List<Tuple<int, int>> turnEndingFromInterfaceVisited = new List<Tuple<int, int>>(3);
            List<int> turnEndingFromInterfaceVisitedMovesNeeded = new List<int>(3);

            //currently active tiles, whose neighbours are to be searched
            List<Tuple<int, int>> newFringe = new List<Tuple<int, int>>(12);
            //if tile needs multiple moves to be entered
            List<int> newFringeRemainingCostToMove = new List<int>(12);

            newFringe.Add(from);
            newFringeRemainingCostToMove.Add(1);

            int movesElapsed=0;
            for (int i = 0; i < numberOfMoves; i++)
            {

                List<Tuple<int, int>> addToNewFringe = new List<Tuple<int, int>>(12);
                List<int> addToNewFringeRemainingCostToMove = new List<int>(12);

                for (int j = 0; j < newFringe.Count; j++)
                {

                    if (newFringeRemainingCostToMove[j] > 1)
                    {
                        addToNewFringe.Add(newFringe[j]);
                        addToNewFringeRemainingCostToMove.Add(newFringeRemainingCostToMove[j] - 1);
                        continue;
                    }

                    //visit all neighbours
                    for (int k = 0; k < 6; k++)
                    {
                        Tile currentTile = GetTile(newFringe[j]);
                        Tuple<int, int> currentNeighbourPos = GetNeighbouringTile(newFringe[j], (HexDirections)k);
                        Tile currentNeighbour = GetTile(currentNeighbourPos);

#if DEBUG
                        if (visited.Contains(currentNeighbourPos))
                            System.Diagnostics.Debug.Fail("ReachableTilesThisTurn(...) is bugged", "newFringe is a neighbour to visited (processed) tile");
#endif

                        if (Tile.MovesRequired(currentTile.Type, currentNeighbour.Type) == int.MaxValue)
                            continue;

                        if (IsTileUnreachable(currentNeighbourPos))
                            continue;

                        if (turnEndingTileVisited.Contains(currentNeighbourPos))
                            continue;

                        if (addToNewFringe.Contains(currentNeighbourPos))
                            continue;

                        if (newFringe.Contains(currentNeighbourPos))
                            continue;

                        if (oldFringe.Contains(currentNeighbourPos))
                            continue;


                        //implicitly assumed from this point: can enter neighbouring tile

                        if (IsTileTurnEnding(currentNeighbourPos))
                        {
                            turnEndingTileVisited.Add(currentNeighbourPos);
                            turnEndingTileVisitedMovesNeeded.Add(movesElapsed);
                            continue;
                        }

                        if (Tile.IsInterfaceTurnEnding(currentTile.Type, currentNeighbour.Type))
                        {
                            turnEndingFromInterfaceVisited.Add(currentNeighbourPos);
                            turnEndingFromInterfaceVisitedMovesNeeded.Add(movesElapsed);
                            continue;
                        }

                        if (!Tile.IsInterfaceTurnEnding(currentTile.Type, currentNeighbour.Type) &&
                            turnEndingFromInterfaceVisited.Contains(currentNeighbourPos))
                        {
                            int indexToRemove = turnEndingFromInterfaceVisited.IndexOf(currentNeighbourPos);
                            turnEndingFromInterfaceVisited.RemoveAt(indexToRemove);
                            turnEndingFromInterfaceVisitedMovesNeeded.RemoveAt(indexToRemove);
                            newFringe.Add(currentNeighbourPos);
                            newFringeRemainingCostToMove.Add(Tile.MovesRequired(currentTile.Type, currentNeighbour.Type));
                            continue;
                        }

                        addToNewFringe.Add(currentNeighbourPos);
                        addToNewFringeRemainingCostToMove.Add(Tile.MovesRequired(currentTile.Type, currentNeighbour.Type));
                    }

                }

                for (int j = 0; j < newFringe.Count; j++)
                {
                    if (newFringeRemainingCostToMove[j] == 1)
                    {
                        oldFringe.Add(newFringe[j]);
                        oldFringeMovesNeeded.Add(movesElapsed);
                    }
                }

                newFringe = addToNewFringe;
                newFringeRemainingCostToMove = addToNewFringeRemainingCostToMove;

                addToNewFringe = new List<Tuple<int, int>>();
                addToNewFringeRemainingCostToMove = new List<int>();


                List<Tuple<int, int>> addToOldFringe = new List<Tuple<int, int>>(6);
                List<int> addToOldFringeMovesNeeded = new List<int>(6);

                for (int j = 0; j < oldFringe.Count; j++)
                {
                    bool moveToVisited = true;

                    for (int k = 0; k < 6; k++)
                    {
                        if (newFringe.Contains(GetNeighbouringTile(oldFringe[j], (HexDirections)k)))
                        {
                            addToOldFringe.Add(oldFringe[j]);
                            addToOldFringeMovesNeeded.Add(oldFringeMovesNeeded[j]);
                            moveToVisited = false;
                            break;
                        }
                    }

                    if (moveToVisited)
                    {
                        visited.Add(oldFringe[j]);
                        visitedMovesNeeded.Add(oldFringeMovesNeeded[j]);
                    }
                }

                oldFringe = addToOldFringe;
                oldFringeMovesNeeded = addToOldFringeMovesNeeded;

                addToOldFringe = new List<Tuple<int, int>>();
                addToOldFringeMovesNeeded = new List<int>();

                movesElapsed = i + 1;
            }

            for (int i = 0; i < visited.Count; i++)
                result.Add(Tuple.Create(visited[i], visitedMovesNeeded[i]));

            for (int i = 0; i < oldFringe.Count; i++)
                result.Add(Tuple.Create(oldFringe[i], oldFringeMovesNeeded[i]));

            for (int i = 0; i < turnEndingTileVisited.Count; i++)
                result.Add(Tuple.Create(turnEndingTileVisited[i], turnEndingTileVisitedMovesNeeded[i]));

            for (int i = 0; i < turnEndingFromInterfaceVisited.Count; i++)
                result.Add(Tuple.Create(turnEndingFromInterfaceVisited[i], turnEndingFromInterfaceVisitedMovesNeeded[i]));

            for (int i = 0; i < newFringe.Count; i++)
                result.Add(Tuple.Create(newFringe[i], numberOfMoves));

            result.Remove(Tuple.Create(from, 0));

            return result;
        }

        /// <summary>
        /// Checks if entering given tile is turn-ending. Does not account for interface between tiles.
        /// </summary>
        public bool IsTileTurnEnding(Tuple<int, int> tile)
        {
            if (GetTile(tile).UnitOnTile != Unit.Empty) //&& GetTile(tile).UnitOnTile.OwnedByFaction != currentlyPlayingFaction)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if any unit can enter given tile.
        /// </summary>
        public bool IsTileUnreachable(Tuple<int, int> tile)
        {
            return GetTile(tile).UnitOnTile.UnitCount >= MaxUnitsPerTile;
        }


        /*
        /// <summary>
        /// Measures land distance between two tiles on hexagonal grid, taking into account terrain.
        /// </summary>
        /// <returns>
        /// Return shortest land distance, will return -1 if starting tile is not land or is not reachable through land.
        /// </returns>
        public int LandDistance(Move move)
        {
            var visited = new List<Tuple<int, int>>(20);


        }*/


        /// <summary>
        /// Returns coordinates of neighbouring tile. If game board borders are taken into account (default), returns same tile if source tile or resulting neighbour are outside of board.
        /// </summary>
        public Tuple<int, int> GetNeighbouringTile(Tuple<int, int> tile, HexDirections dir, bool ignoreBorders = false)
        {
            Tuple<int, int> result;
            switch (dir)
            {
                case HexDirections.Up:
                    result = Tuple.Create(tile.Item1, tile.Item2 + 1);
                    break;
                case HexDirections.UpRight:
                    result = Tuple.Create(tile.Item1 + 1, (Math.Abs(tile.Item1 % 2) == 0) ? tile.Item2 : tile.Item2 + 1);
                    break;
                case HexDirections.DownRight:
                    result = Tuple.Create(tile.Item1 + 1, (Math.Abs(tile.Item1 % 2) == 0) ? tile.Item2 - 1 : tile.Item2);
                    break;
                case HexDirections.Down:
                    result = Tuple.Create(tile.Item1, tile.Item2 - 1);
                    break;
                case HexDirections.DownLeft:
                    result = Tuple.Create(tile.Item1 - 1, (Math.Abs(tile.Item1 % 2) == 0) ? tile.Item2 - 1 : tile.Item2);
                    break;
                case HexDirections.LeftUp:
                    result = Tuple.Create(tile.Item1 - 1, (Math.Abs(tile.Item1 % 2) == 0) ? tile.Item2 : tile.Item2 + 1);
                    break;
                default:
                    throw new ArgumentException("dir", "direction entered is not valid");
            }

            if (ignoreBorders == false && IsOutOfBounds(result))
                result = tile;

            return result;
        }

        public bool IsOutOfBounds(Tuple<int, int> tile)
        {
            return tile.Item1 < 0 ||
                    tile.Item2 < 0 ||
                    tile.Item1 > SizeX - 1 ||
                    tile.Item2 > SizeY - 1;
        }






        #region cloning
        /// <summary>
        /// Intended for creating a copy, does not call other constructors.
        /// </summary>
        private GameSession(int sizeX, int sizeY, Tile[,] tiles, Faction[] factions)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            Tiles = tiles;
            Factions = factions;
        }

        //TODO: predelat
        /// <summary>
        /// Creates copy of current game state (including copy of all reference members), intended to increase safety while passing data to scripted AI or other components.
        /// </summary>
        /// <returns>Return new copy.</returns>
        public GameSession CreateCompleteCopy()
        {
            throw new NotImplementedException();
            Tile[,] tiles = new Tile[SizeX, SizeY];
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    //tiles[i, j] = Tiles[i, j].CreateCompleteCopy();
                }
            }

            Faction[] factions = new Faction[Factions.Length];
            for (int i = 0; i < factions.Length; i++)
            {
                factions[i] = Factions[i].CreateCompleteCopy();
            }

            GameSession copy = new GameSession(SizeX, SizeY, tiles, factions);
            return copy;
        }
        #endregion


    }

    public enum HexDirections { Up, UpRight, DownRight, Down, DownLeft, LeftUp, U = Up, UR = UpRight, DR = DownRight, D = Down, DL = DownLeft, LU = LeftUp }

    public struct Move
    {
        public Tuple<int, int> FromTile;
        public Tuple<int, int> ToTile;

        /// <summary>
        /// X (horizontal) coordinate of starting tile.
        /// </summary>
        public int FromX { get { return FromTile.Item1; } }

        /// <summary>
        /// Y (vertical) coordinate of starting tile.
        /// </summary>
        public int FromY { get { return FromTile.Item2; } }

        /// <summary>
        /// X (horizontal) coordinate of target tile.
        /// </summary>
        public int ToX { get { return ToTile.Item1; } }

        /// <summary>
        /// Y (vertical) coordinate of target tile.
        /// </summary>
        public int ToY { get { return ToTile.Item2; } }
    }




    public class Tile
    {
        public TerrainTypes Type { get; private set; }
        public Unit UnitOnTile { get; set; }
        public Faction OccupiedByFaction { get; set; }

        public Tile(TerrainTypes type)
        {
            OccupiedByFaction = Faction.Empty;
            Type = type;
            UnitOnTile = Unit.Empty;
        }

        //[0,0] from land to land, [0,1] from land to water...
        //Land, Water, City, Harbor, Capital
        private static int[,] movesRequired = new int[5, 5]{
                {1, int.MaxValue,1,1,1},
                {2,1,2,2,2},
                {1,int.MaxValue,1,1,1},
                {1,1,1,1,1},
                {1,int.MaxValue,1,1,1}
            };

        public static int MovesRequired(TerrainTypes from, TerrainTypes to)
        {
            return movesRequired[(int)from, (int)to];
        }

        private static bool[,] turnEndingInterface = new bool[5, 5]{
                {false, false,true,true,true},
                {true,false,true,true,true},
                {false,false,true,true,true},
                {false,false,true,true,true},
                {false,false,true,true,true}
            };

        public static bool IsInterfaceTurnEnding(TerrainTypes from, TerrainTypes to)
        {
            return turnEndingInterface[(int)from, (int)to];
        }



        /*
        /// <summary>
        /// Creates copy of this tile (including copy of all reference members).
        /// </summary>
        public Tile CreateCompleteCopy()
        {
            return new Tile() { Type = this.Type, UnitCount = this.UnitCount, UnitMorale = this.UnitMorale };
        }*/
    }

    public class Unit
    {
        public int UnitCount { get; set; }
        public int UnitMorale { get; set; }
        public bool UnitMovedThisTurn { get; set; }
        public int OwnedByFaction { get; set; }

        static Unit()
        {
            Empty = new Unit();
            Empty.OwnedByFaction = 0;
        }

        public readonly static Unit Empty;
    }

    public enum TerrainTypes { Land, Water, City, Harbor, Capital }

    /// <summary>
    /// Represents faction in game logic and corresponds to a player, either AI or human controlled.
    /// </summary>
    public class Faction
    {
        public int ID { get; private set; }

        private Faction(int ID)
        {
            this.ID = ID;
        }

        public static Faction[] CreateFactions(int numOfFactions)
        {
            Faction[] array = new Faction[numOfFactions];
            array[0] = Faction.Empty;
            for (int i = 1; i < numOfFactions + 1; i++)
            {
                array[i] = new Faction(i);
            }
            return array;
        }

        static Faction()
        {
            Empty = new Faction(0);
        }

        /// <summary>
        /// Creates copy of this faction (including copy of all reference members).
        /// </summary>
        public Faction CreateCompleteCopy()
        {
            return new Faction(this.ID);
        }

        public readonly static Faction Empty;

    }

}