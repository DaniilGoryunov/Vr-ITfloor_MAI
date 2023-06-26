using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        /// <summary>
        /// The height of a floor when substracted the window height, and the window height from the floor
        /// The value is in percentage from 0 to 1
        /// </summary>
        public float LeftOverHeight
        {
            get
            {
                return 1f - windowHeight - windowHeightFromFloor;
            }
        }

        void dequeueWallsToDrawingPipeline()
        {
            int i = 0;
            foreach (WallPart p in WallList.Values)
            {
                i++;
                putWall(p);
            }
        }

        public void depileMeshPane()
        {
            List<MeshPane> mpList = new List<MeshPane>();
            List<MeshPane> topperMPList = new List<MeshPane>();

            //double timer = UnityEditor.EditorApplication.timeSinceStartup;
            //measuredFunctionTimer += UnityEditor.EditorApplication.timeSinceStartup - timer;
            foreach (var floor in WallMesh.WallMeshes.Values)
            {
                foreach (var holder in floor.Values)
                {
                    foreach (ProceduralObject po in holder)
                    {
                        mpList.AddRange(po.MeshPaneList[(int)MESHPANELIST.NORTH]);
                        mpList.AddRange(po.MeshPaneList[(int)MESHPANELIST.SOUTH]);
                        mpList.AddRange(po.MeshPaneList[(int)MESHPANELIST.EAST]);
                        mpList.AddRange(po.MeshPaneList[(int)MESHPANELIST.WEST]);
                        topperMPList.AddRange(po.topperMeshPaneList);
                    }
                }
            }
            foreach (MeshPane mp in mpList)
            {
                int floor = findFloor(mp.first.y);
                WallMesh.setUID(mp.separationID);
                WallMesh.Get(floor).optimizeWallMode = false;
                WallMesh.Get(floor).addPane(mp.first, mp.second, mp.third, mp.forth, mp.separationID, mp.invert, mp.textureID);
            }
            foreach (MeshPane mp in topperMPList)
            {
                int floor = findFloor(mp.first.y - 0.05f);
                WallMesh.setUID(mp.separationID);
                WallMesh.Get(floor).optimizeWallMode = false;
                WallMesh.Get(floor).addPane(mp.first, mp.second, mp.third, mp.forth, mp.separationID, mp.invert, mp.textureID);
            }
            WallMesh.resetUID();
        }

        void duplicateFloors()
        {
            foreach(var v in floorsToDuplicate)
            {
                GameObject copiedWalls;
                GameObject copiedWallsObjects;
                GameObject copiedHitBoxes;
                if (wallMeshHolder.ContainsKey(v.Value))
                {
                    copiedWalls = wallMeshHolder[v.Value];
                    copiedWallsObjects = objectHolderManager.getHolder(v.Value);
                    copiedHitBoxes = hitBoxManager.Get(v.Value);
                }
                else
                {
                    Debug.LogError("Floor hard copy: The model does not have a floor at level " + v.Value);
                    continue;
                }
                GameObject newFloor = GameObject.Instantiate(copiedWalls, copiedWalls.transform.parent);
                GameObject newFloorWallObject = GameObject.Instantiate(copiedWallsObjects, copiedWallsObjects.transform.parent);
                GameObject newFloorHitBoxes = GameObject.Instantiate(copiedHitBoxes, copiedHitBoxes.transform.parent);
                newFloor.transform.localPosition = new Vector3(copiedWalls.transform.position.x, v.Key * RealDimensionsVector.y , copiedWalls.transform.position.z);
                newFloorWallObject.transform.localPosition = new Vector3(copiedWallsObjects.transform.position.x, v.Key * RealDimensionsVector.y, copiedWallsObjects.transform.position.z);
                newFloorHitBoxes.transform.localPosition = new Vector3(copiedHitBoxes.transform.position.x, v.Key * RealDimensionsVector.y, copiedHitBoxes.transform.position.z);
                objectHolderManager.setHolder(v.Key, newFloorWallObject);
                if (wallMeshHolder.ContainsKey(v.Key) == false)
                    wallMeshHolder.Add(v.Key, newFloor);
            }
        }

        void putWall(WallPart wallPart)
        {
            float tmpFrameWidth = (HorizontalScale * 0.5f) - (wallThickness * 0.5f) - FrameBufferWidth;
            Vector3 bufferPos = new Vector3(0.5f * wallThickness, 0, 0);
            Vector3 framePos = bufferPos + new Vector3(FrameBufferWidth, 0, 0);

            if (FrameBufferWidth <= 0)
                throw new System.Exception("Parameter error: The frame width is too wide in relation to the horizontal scale\n" +
                                           "(Hscale: " + HorizontalScale + " - frameW: " + FrameWidth + " - thickness: " + wallThickness + " - buffer width: " + FrameBufferWidth + ")");
            Vector3 position = wallPart.position.toVector3(RealDimensionsVector);
            if (wallPart.mods[Directions.CENTER] != Mod.NONE)
            {
                if (wallPart.mods[Directions.WEST] != Mod.NONE)
                    WallMesh.Get(wallPart.position.y).disableThoseOrientations(Directions.WEST);
                if (wallPart.mods[Directions.EAST] != Mod.NONE)
                    WallMesh.Get(wallPart.position.y).disableThoseOrientations(Directions.EAST);
                if (wallPart.mods[Directions.NORTH] != Mod.NONE)
                    WallMesh.Get(wallPart.position.y).disableThoseOrientations(Directions.NORTH);
                if (wallPart.mods[Directions.SOUTH] != Mod.NONE)
                    WallMesh.Get(wallPart.position.y).disableThoseOrientations(Directions.SOUTH);
                AddBoxDivided(wallPart,
                              position + Vector3.zero,
                              new Vector3(wallThickness,VerticalScale, wallThickness),
                              Directions.CENTER,
                              Directions.NONE,
                              wallPart.seperationID[Directions.CENTER]);
                WallMesh.Get(wallPart.position.y).resetDisabledPositions();
            }
            if (wallPart.mods[Directions.WEST] != Mod.NONE)
            {
                addWallDirectionPart(wallPart, Directions.WEST, position, -bufferPos, -framePos,
                                     new Vector3(FrameBufferWidth, VerticalScale, wallThickness),
                                     new Vector3(tmpFrameWidth, VerticalScale, wallThickness));
                WallMesh.Get(wallPart.position.y).resetDisabledPositions();
            }
            if (wallPart.mods[Directions.EAST] != Mod.NONE)
            {
                addWallDirectionPart(wallPart, Directions.EAST, position, bufferPos, framePos,
                                     new Vector3(FrameBufferWidth, VerticalScale, wallThickness),
                                     new Vector3(tmpFrameWidth, VerticalScale, wallThickness));
                WallMesh.Get(wallPart.position.y).resetDisabledPositions();
            }
            bufferPos = new Vector3(bufferPos.z, bufferPos.y, bufferPos.x);
            framePos = new Vector3(framePos.z, framePos.y, framePos.x);
            if (wallPart.mods[Directions.NORTH] != Mod.NONE)
            {
                addWallDirectionPart(wallPart, Directions.NORTH, position, bufferPos, framePos,
                         new Vector3(wallThickness, VerticalScale, FrameBufferWidth),
                         new Vector3(wallThickness, VerticalScale, tmpFrameWidth));
            }
            if (wallPart.mods[Directions.SOUTH] != Mod.NONE)
            {
                addWallDirectionPart(wallPart, Directions.SOUTH, position, -bufferPos, -framePos,
                         new Vector3(wallThickness, VerticalScale, FrameBufferWidth),
                         new Vector3(wallThickness, VerticalScale, tmpFrameWidth));
            }
        }

        public void addWallDirectionPart(WallPart wallPart, Directions direction, Vector3 position, Vector3 bufferPos, Vector3 framePos, Vector3 bufferSize, Vector3 FrameSize)
        {
            if (wallPart.mods[direction] != Mod.NONE)
            {
                WallMesh.Get(wallPart.position.y).disableThoseOrientations(Directions.oppositeOf(direction));
                if (wallPart.mods[direction] == Mod.FULL)
                    WallMesh.Get(wallPart.position.y).disableThoseOrientations(direction);
                AddBoxDivided(wallPart, position + bufferPos, bufferSize,
                              direction, Directions.oppositeOf(direction),
                              wallPart.seperationID[direction], true);
                AddBoxDivided(wallPart, position + framePos, FrameSize,
                              direction, Directions.oppositeOf(direction),
                              wallPart.seperationID[direction]);
                WallMesh.Get(wallPart.position.y).resetDisabledPositions();
            }
        }

        /// <summary>
        /// Add a pane of wall (A box) to a wall block 
        /// </summary>
        /// <param name="wallPart">The associated data</param>
        /// <param name="pos">The Spacial position of the box (NOT grid position)</param>
        /// <param name="size">The size of the box</param>
        /// <param name="direction">The direction of the part of the wall that it belongs to (Cardinal direction)</param>
        /// <param name="referenceCorner">The Reference point that correspond to where the drawing starts. For example, if it says "WEST", the box will be drawn at the LEFT of the parameter "pos"</param>
        /// <param name="cancelledPlanes">Parts of the drawn box can be cancelled to save memory</param>
        void AddBoxDivided(WallPart wallPart,
                                  Vector3 pos,
                                  Vector3 size,
                                  Directions direction,
                                  Directions referenceCorner,
                                  string separationID,
                                  bool isBuffer = false)
        {
            if (wallPart.hideBuffers && isBuffer)
                isBuffer = false;
            Vector3 newSize = new Vector3(size.x, size.y, size.z);
            Vector3 newPos = new Vector3(pos.x, pos.y, pos.z);
            newSize.y = windowHeightFromFloor * size.y;
            if (isBuffer || wallPart.mods[direction] == Mod.FULL || wallPart.mods[direction] == Mod.WINDOW)
            {
                if (wallPart.mods[direction] == Mod.FULL || wallPart.mods[direction] == Mod.DOOR || agressiveOptimization > 1)
                    WallMesh.Get(wallPart.position.y).disabledPanes[Directions.UP] = true;
                WallMesh.Get(wallPart.position.y).addBox(pos, newSize, wallPart.textures[direction], referenceCorner, separationID, true);
                WallMesh.Get(wallPart.position.y).disabledPanes[Directions.UP] = false;
            }
            newPos.y = pos.y + windowHeightFromFloor * size.y;
            newSize.y = windowHeight * size.y;
            if (isBuffer || (wallPart.mods[direction].isNot(Mod.WINDOW, Mod.DOOR, Mod.ONLYTOP)))
            {
                WallMesh.Get(wallPart.position.y).disabledPanes[Directions.UP] = true;
                WallMesh.Get(wallPart.position.y).addBox(newPos, newSize, wallPart.textures[direction], referenceCorner, separationID);
                WallMesh.Get(wallPart.position.y).disabledPanes[Directions.UP] = false;
            }
            newPos.y = pos.y + (1f - LeftOverHeight) * size.y;
            newSize.y = LeftOverHeight * size.y;
            if (isBuffer || wallPart.mods[direction] != Mod.NONE)
            {
                if (agressiveOptimization > 0)
                    WallMesh.Get(wallPart.position.y).disabledPanes[Directions.UP] = true;
                WallMesh.Get(wallPart.position.y).addBox(newPos, newSize, wallPart.textures[direction], referenceCorner, separationID, (agressiveOptimization > 1 ? false : true));
                WallMesh.Get(wallPart.position.y).disabledPanes[Directions.UP] = false;
            }
        }


        /// <summary>
        /// Generates a set of colliders for the current Building
        /// </summary>
        void generateColliders()
        {
            Position incr = bottomLeft.copy();

            while (incr.y <= topRight.y)
            {
                while (incr.z <= topRight.z)
                {
                    generateCollidersX(incr, windowHeightFromFloor, 0, new int[] { Mod.DOOR, Mod.ONLYTOP, Mod.NONE });
                    generateCollidersX(incr, windowHeight, windowHeightFromFloor, new int[] { Mod.DOOR, Mod.WINDOW, Mod.ONLYTOP, Mod.NONE });
                    generateCollidersX(incr, LeftOverHeight, windowHeight + windowHeightFromFloor, new int[] { Mod.NONE });
                    incr.z++;
                }
                incr.z = bottomLeft.z;
                while (incr.x <= topRight.x)
                {
                    generateCollidersZ(incr, windowHeightFromFloor, 0, new int[] { Mod.DOOR, Mod.ONLYTOP, Mod.NONE });
                    generateCollidersZ(incr, windowHeight, windowHeightFromFloor, new int[] { Mod.DOOR, Mod.WINDOW, Mod.ONLYTOP, Mod.NONE });
                    generateCollidersZ(incr, LeftOverHeight, windowHeight + windowHeightFromFloor, new int[] { Mod.NONE });
                    incr.x++;
                }
                incr.x = bottomLeft.x;
                incr.y++;
            }
        }

        /// <summary>
        /// Sub routine to the method generateColliders()
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="subScale"></param>
        /// <param name="subVerticalOffset"></param>
        /// <param name="comparators"></param>
        void generateCollidersX(Position iteration, float subScale, float subVerticalOffset, int[] comparators)
        {
            Vector3 boxSize = new Vector3(0, VerticalScale * subScale, wallThickness);
            Position incr = iteration.copy();
            bool isBuilding = false;
            Vector3 CreationStartingPoint = Vector3.zero;

            while (incr.x <= topRight.x)
            {
                if (!isBuilding && WallList.ContainsKey(incr) && WallList[incr].mods[Directions.CENTER].isNot(Mod.NONE, Mod.ONLYTOP))
                {
                    CreationStartingPoint = incr.toVector3(RealDimensionsVector) + new Vector3(-0.5f * wallThickness, VerticalScale * subVerticalOffset, -0.5f * wallThickness);
                    if (WallList[incr].mods[Directions.WEST] != Mod.NONE)
                    {
                        CreationStartingPoint.x -= FrameBufferWidth;
                        boxSize.x += FrameBufferWidth;
                    }
                    isBuilding = true;
                }
                if (isBuilding)
                {
                    if (WallList.ContainsKey(incr.offsetBy(x: 1)) == false ||
                        WallList[incr].mods[Directions.EAST].isEither(comparators))
                    {
                        if (WallList[incr].mods[Directions.EAST] != Mod.NONE)
                            boxSize.x += FrameBufferWidth;
                        addColliderX(ref boxSize, CreationStartingPoint, subScale, ref isBuilding);
                    }
                    else
                        boxSize.x += HorizontalScale;
                }
                incr.x++;
            }
        }

        /// <summary>
        /// Sub routine to the method generateColliders()
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="subScale"></param>
        /// <param name="isBuilding"></param>
        void addColliderX(ref Vector3 size, Vector3 position, float subScale, ref bool isBuilding)
        {
            if (size.x > 0)
            {
                size.x += wallThickness;
                position += size * 0.5f;
                var boxCollider = hitBoxManager.Get(findFloor(position.y)).AddComponent<BoxCollider>();
                boxCollider.center = position + new Vector3(0, 0, 0);
                boxCollider.size = size;
            }
            isBuilding = false;
            size = new Vector3(0, VerticalScale * subScale, wallThickness);
        }



        /// <summary>
        /// Sub routine to the method generateColliders()
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="subScale"></param>
        /// <param name="subVerticalOffset"></param>
        /// <param name="comparators"></param>
        void generateCollidersZ(Position iteration, float subScale, float subVerticalOffset, int[] comparators)
        {
            Vector3 boxSize = new Vector3(wallThickness, VerticalScale * subScale, 0);
            Position incr = iteration.copy();
            bool isBuilding = false;
            Vector3 CreationStartingPoint = Vector3.zero;

            while (incr.z <= topRight.z)
            {
                if (!isBuilding && WallList.ContainsKey(incr) && WallList[incr].mods[Directions.CENTER].isNot(Mod.NONE, Mod.ONLYTOP))
                {
                    CreationStartingPoint = incr.toVector3(RealDimensionsVector) + new Vector3(-0.5f * wallThickness, VerticalScale * subVerticalOffset, -0.5f * wallThickness);
                    if (WallList[incr].mods[Directions.SOUTH] != Mod.NONE)
                    {
                        CreationStartingPoint.z -= FrameBufferWidth;
                        boxSize.z += FrameBufferWidth;
                    }
                    isBuilding = true;
                }
                if (isBuilding)
                {
                    if (WallList.ContainsKey(incr.offsetBy(z: 1)) == false ||
                        WallList[incr].mods[Directions.NORTH].isEither(comparators))
                    {
                        if (WallList[incr].mods[Directions.NORTH] != Mod.NONE)
                            boxSize.z += FrameBufferWidth;
                        addColliderZ(ref boxSize, CreationStartingPoint, subScale, ref isBuilding);
                    }
                    else
                        boxSize.z += HorizontalScale;
                }
                incr.z++;
            }
        }

        /// <summary>
        /// Sub routine to the method generateColliders()
        /// </summary>
        /// <param name="size"></param>
        /// <param name="position"></param>
        /// <param name="subScale"></param>
        /// <param name="isBuilding"></param>
        public void addColliderZ(ref Vector3 size, Vector3 position, float subScale, ref bool isBuilding)
        {
            if (size.z > 0)
            {
                size.z += wallThickness;
                position += size * 0.5f;
                var boxCollider = hitBoxManager.Get(findFloor(position.y)).AddComponent<BoxCollider>();
                boxCollider.center = position + new Vector3(0, 0, 0);
                boxCollider.size = size;
            }
            isBuilding = false;
            size = new Vector3(wallThickness, VerticalScale * subScale, 0);
        }
    }
}
