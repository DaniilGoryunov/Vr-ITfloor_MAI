using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        List<Rectangle> mergedFloorTiles;
        List<Rectangle> groundSurface;
        bool[,,] floorTilesBoard;
        int floorTileCount = 0;
        BoxTexture floorTex;

        void addFloorBigTileMesh(Rectangle r)
        {
            bool prevState = WallMesh.Get(r.position.y).optimizeWallMode;
            WallMesh.Get(r.position.y).optimizeWallMode = false;
            Vector3 pos = r.position.toVector3(RealDimensionsVector) + r.size.toVector3(RealDimensionsVector) * 0.5f - new Vector3(0, floorThickness - 0.001f, 0);
            Vector3 size = r.size.toVector3(RealDimensionsVector) + new Vector3(wallThickness * 0.999f, floorThickness, wallThickness * 0.999f);
            WallMesh.Get(r.position.y).addBox(pos, size, floorTex, Directions.NONE, "floors", true);
            BoxCollider b = hitBoxManager.Get(r.position.y).AddComponent<BoxCollider>();
            b.center = pos + new Vector3(0, floorThickness * 0.5f + elevation);
            b.size = size;
            if (groundSurface == null)
                groundSurface = new List<Rectangle>();
            if (r.position.y == 0)
                groundSurface.Add(r);
            WallMesh.Get(r.position.y).optimizeWallMode = prevState;
        }

        void addFoundation(Rectangle r)
        {
            Vector3 pos = r.position.toVector3(RealDimensionsVector) + r.size.toVector3(RealDimensionsVector) * 0.5f - new Vector3(0.001f, floorThickness + elevation, 0.001f);
            Vector3 size = r.size.toVector3(RealDimensionsVector) + new Vector3(wallThickness * 1.009f + 0.002f, floorThickness + elevation, wallThickness * 1.009f + 0.002f);
            bool prevState = WallMesh.Get(r.position.y).optimizeWallMode;
            WallMesh.Get(r.position.y).optimizeWallMode = false;
            WallMesh.Get(r.position.y).addBox(pos, size, new BoxTexture(rest:currentFoundation), Directions.NONE, "floors", true);
            BoxCollider b = hitBoxManager.Get(r.position.y).AddComponent<BoxCollider>();
            b.center = pos + new Vector3(0, floorThickness * 0.5f + elevation * 1.5f);
            b.size = size;
            WallMesh.Get(r.position.y).optimizeWallMode = prevState;

        }

        void setFloorTexture()
        {
            floorTex = new BoxTexture();
            for (int i = 0; i < floorTex.textureID.Length; i++)
                floorTex.textureID[i] = currentEdgeTex;
            floorTex.textureID[Directions.UP] = currentFloorTex;
        }

        void buildFloors()
        {
            floorTilesBoard = new bool[bottomLeft.x + totalSize.x + 1, bottomLeft.y + totalSize.y + 3, bottomLeft.z + totalSize.z + 1];
            for (int i = 0; i < floorTiles.Count; i++)
            {
                if (floorTiles[i].disabled)
                    continue;
                floorTilesBoard[floorTiles[i].position.x, floorTiles[i].position.y, floorTiles[i].position.z] = true;
                floorTileCount++;
            }
            mergedFloorTiles = new List<Rectangle>();
            mergeFloor();

            setFloorTexture();
            foreach (Rectangle r in mergedFloorTiles)
            {
                addFloorBigTileMesh(r);
            }
        }

        void buildFoundation()
        {
            if (groundSurface == null)
                return;
            foreach (Rectangle r in groundSurface)
            {
                addFoundation(r);
            }
        }


        /// <summary>
        /// Add floors for every room created
        /// </summary>
        public void addFloorEverywhere()
        {
            FloorTile f = new FloorTile(null);
            foreach (Room r in rooms)
            {
                for (int z = 0; z < r.Size.z; z++)
                {
                    for (int x = 0; x < r.Size.x; x++)
                    {
                        f.Set(r.BottomLeft + new Position(x, 0, z));
                        bool collision = false;
                        if (stairs != null)
                        {
                            for (int i = 0; i < stairs.Count; i++)
                            {
                                if (tileStairCollision(f, stairs[i]))
                                {
                                    collision = true;
                                    break;
                                }
                            }

                        }
                        if (doubleStairs != null)
                        {
                            for (int i = 0; i < doubleStairs.Count; i++)
                            {
                                if (collision)
                                    break;
                                if (tileStairCollision(f, doubleStairs[i]))
                                {
                                    collision = true;
                                    break;
                                }
                            }
                        }
                        if (collision)
                            continue;
                        floorTiles.Add(new FloorTile(f.position));
                    }
                }
            }
        }

        // attention: Trying to merge the floor tiles into bigger rectangles, using the recursive method from the ceiling tiles merging.
        // The process is half set, remaining is:
        // replacing the current tiles
        // Generating colliders from them

        bool mergeFloor(int floor = 0)
        {
            Rectangle recordRect = Rectangle.Zero;
            bool foundNewMax = false;

            recordRect = findMaxRect(ref floorTilesBoard, floor, ref floorTileCount, ref foundNewMax);
            if (recordRect.size.x > 0 && recordRect.size.z > 0)
            {
                mergedFloorTiles.Add(recordRect);
            }
            if (floorTileCount <= 0 || floor > 999 || floor >= floorTilesBoard.GetLength(1) - 1)
            {
                return false;
            }
            if (!foundNewMax)
                mergeFloor(++floor);
            else
                mergeFloor(floor);
            return true;
        }
    }
}