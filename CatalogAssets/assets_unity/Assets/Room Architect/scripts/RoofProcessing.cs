using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        bool[,,] ceilingTilesBoard;
        InternalRoof roof;
        List<Position> ceilingTiles;
        //GameObject testTile;
        float recordFloorHeight = 0;
        int roofTileCount;

        void mapRoof()
        {
            roof = new InternalRoof();
            findTiles();
            findEdges();
            mergeTiles();
        }

        /// <summary>
        /// Maps every top surface eligible to have roof
        /// </summary>
        /// <returns>a list of singleRoof objects</returns>
        public List<Roof> mapRoofs()
        {
            mapRoof();
            return roof.roofs;
        }

        /// <summary>
        /// Finds single square tiles on the building that opens to the outside and needs to be covered by a roof
        /// </summary>
        void findTiles()
        {
            //testTile = GameObject.Find("TestTile");
            ceilingTilesBoard = new bool[bottomLeft.x + totalSize.x + 1,
                                         bottomLeft.y + totalSize.y + 2,
                                         bottomLeft.z + totalSize.z + 1];
            for (int i = 0; i < ceilingTilesBoard.GetLength(0); i++)
                for (int j = 0; j < ceilingTilesBoard.GetLength(1); j++)
                    for (int k = 0; k < ceilingTilesBoard.GetLength(2); k++)
                        ceilingTilesBoard[i, j, k] = false;
            ceilingTiles = new List<Position>();
            Position incr = bottomLeft.copy();
            roofTileCount = 0;
            while (incr.z < topRight.z)
            {
                incr.x = bottomLeft.x;
                while (incr.x < topRight.x)
                {
                    if (!ceilingTiles.Contains(incr))
                    {
                        Position p = findRoofLevel(incr);
                        if (p != null)
                        {
                            ceilingTiles.Add(p);
                            ceilingTilesBoard[incr.x, p.y, incr.z] = true;
                            roofTileCount++;
                        }
                    }
                    incr.x++;
                }
                incr.z++;
            }
        }

        void findEdges()
        {
            foreach (Position tile in ceilingTiles)
            {
                Position ntile = tile + new Position(0, 0, 1);

                if (!WallList.ContainsKey(tile.offsetBy(y: 0)) || WallList[tile.offsetBy(y: 0)].mods[Directions.EAST] == Mod.NONE)
                {
                    if (WallList.ContainsKey(tile.offsetBy(y: -1)) &&
                        WallList[tile.offsetBy(y: -1)].mods[Directions.EAST] != Mod.NONE)
                        tileEdgesHorizontal(tile);
                }
                if (!WallList.ContainsKey(tile.offsetBy(y: 0)) || WallList[tile.offsetBy(y: 0)].mods[Directions.NORTH] == Mod.NONE)
                {
                    //HERE
                    if (WallList.ContainsKey(tile.offsetBy(y: -1)) &&
                        WallList[tile.offsetBy(y: -1)].mods[Directions.NORTH] != Mod.NONE)
                        tileEdgesVertical(tile);
                }
                if (!WallList.ContainsKey(ntile.offsetBy(y: 0)) || WallList[ntile.offsetBy(y: 0)].mods[Directions.EAST] == Mod.NONE)
                {
                    if (WallList.ContainsKey(ntile.offsetBy(y: -1)) &&
                        WallList[ntile.offsetBy(y: -1)].mods[Directions.EAST] != Mod.NONE)
                        tileEdgesHorizontal(ntile);
                }
                ntile = tile + new Position(1, 0, 0);
                if (!WallList.ContainsKey(ntile.offsetBy(y: 0)) || WallList[ntile.offsetBy(y: 0)].mods[Directions.NORTH] == Mod.NONE)
                {
                    if (WallList.ContainsKey(ntile.offsetBy(y: -1)) &&
                        WallList[ntile.offsetBy(y: -1)].mods[Directions.NORTH] != Mod.NONE)
                        tileEdgesVertical(ntile);
                }
            }
            //GameObject testTile = GameObject.Find("testTile");
            //foreach (Verge e in roof.verges)
            //{
            //    GameObject go = GameObject.Instantiate(testTile) as GameObject;
            //    go.transform.SetParent(this.transform, false);
            //    go.transform.localPosition = e.position.toVector3(RealDimensionsVector);
            //    //go.transform.localScale = new Vector3(e.length * HorizontalScale, 1f, 1f);
            //    if (e.horizontal == false)
            //        go.transform.Rotate(Vector3.up, -90);

            //}
        }

        void tileEdgesHorizontal(Position tile)
        {
            for (int i = 0; i < roof.verges.Count; i++)
            {
                if (!roof.verges[i].horizontal || roof.verges[i].position.y != tile.y)
                    continue;
                if (isInside(tile) && isInside(tile.offsetBy(z: -1)))
                    return;
                if (tile == roof.verges[i].position.offsetBy(x: -1))
                {
                    roof.verges[i].position.x--;
                    roof.verges[i].length++;
                    return;
                }
                if (roof.verges[i].position.z == tile.z && roof.verges[i].position.x + roof.verges[i].length > tile.x)
                    return;
                if (roof.verges[i].position.z == tile.z && roof.verges[i].position.x + roof.verges[i].length == tile.x)
                {
                    roof.verges[i].length++;
                    return;
                }
            }
            Verge newEdge = new Verge(tile, true);
            roof.verges.Add(newEdge);
        }

        void tileEdgesVertical(Position tile)
        {
            for (int i = 0; i < roof.verges.Count; i++)
            {
                if (roof.verges[i].horizontal || roof.verges[i].position.y != tile.y)
                    continue;
                if (isInside(tile) && isInside(tile.offsetBy(x: -1)))
                    return;
                if (tile == roof.verges[i].position.offsetBy(z: -1))
                {
                    roof.verges[i].position.z--;
                    roof.verges[i].length++;
                    return;
                }
                if (roof.verges[i].position.x == tile.x && roof.verges[i].position.z + roof.verges[i].length > tile.z)
                    return;
                if (roof.verges[i].position.x == tile.x && roof.verges[i].position.z + roof.verges[i].length == tile.z)
                {
                    roof.verges[i].length++;
                    return;
                }
            }
            Verge newEdge = new Verge(tile, false);
            roof.verges.Add(newEdge);
        }

        /// <summary>
        /// merges tiles to find how to cover all the roof space with the least squares by floor
        /// </summary>
        /// <param name="floor"></param>
        /// <returns></returns>
        bool mergeTiles(int floor = 0)
        {
            Rectangle recordRect = Rectangle.Zero;
            bool foundNewMax = false;

            recordRect = findMaxRect(ref ceilingTilesBoard, floor, ref roofTileCount, ref foundNewMax);
            if (recordRect.size.x > 0 && recordRect.size.z > 0)
                roof.addNewRoof(recordRect);
            if (roofTileCount <= 0 || floor > 999)
                return false;
            if (!foundNewMax)
                mergeTiles(++floor);
            else
                mergeTiles(floor);
            return true;
        }


        /// <summary>
        /// Internal function, not to be used in the template description
        /// </summary>
        Rectangle recordRect;
        public Rectangle findMaxRect(ref bool[,,] matrix,
                                     int floor,
                                     ref int tileCount,
                                     ref bool foundNewMax)
        {
            recordRect = Rectangle.Zero;
            for (int z = bottomLeft.z; z < topRight.z; z++)
            {
                for (int x = bottomLeft.x; x < topRight.x; x++)
                {
                    int maxX = totalSize.x;
                    int subZ;
                    for (subZ = 0; (z + subZ) <= topRight.z; subZ++)
                    {
                        if (matrix[x, floor, z + subZ] == false)
                            break;
                        int totalZ = z + subZ;
                        for (int subX = 0; (x + subX) <= topRight.x; subX++)
                        {
                            int totalX = x + subX;
                            if (matrix[totalX, floor, totalZ] == false)
                            {
                                if (maxX > subX)
                                    maxX = subX;
                                break;
                            }
                        }
                    }
                    if (maxX * subZ > recordRect.size.XZArea)
                    {
                        recordRect.size = new Position(x: maxX, z: subZ);
                        recordRect.position = new Position(x, floor, z);
                        foundNewMax = true;
                    }
                }
            }
            for (int z = 0; z < recordRect.size.z; z++)
            {
                for (int x = 0; x < recordRect.size.x; x++)
                {
                    matrix[recordRect.position.x + x, floor, recordRect.position.z + z] = false;
                    tileCount--;
                }
            }
            return recordRect;
        }


        Position findRoofLevel(Position p)
        {
            int maxlevel = int.MinValue;
            foreach (Room room in rooms)
            {
                if (room.Contains(p) && room.floor > maxlevel)
                    maxlevel = room.floor;
                if (recordFloorHeight < room.floor)
                    recordFloorHeight = room.floor;
            }
            if (maxlevel == int.MinValue)
                return null;
            return new Position(p.x, maxlevel + 1, p.z);
        }
    }
}
