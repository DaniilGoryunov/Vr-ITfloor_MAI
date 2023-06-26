using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        /// <summary>
        /// Adds a room to the building queue.
        /// </summary>
        /// <param name="position">BottomLeft Corner of the room</param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="wallThickness"></param>
        /// <returns></returns>
        public Room addRoom(Position position, int width, int depth)
        {
            return addRoom(position, position.offsetBy(x: width, z: depth));
        }

        /// <summary>
        /// Adds a room to the building queue.
        /// </summary>
        /// <param name="bottomLeft">BottomLeft Corner of the room</param>
        /// <param name="topRight">topRight Corner of the room</param>
        /// <param name="wallThickness"></param>
        /// <returns></returns>
        public Room addRoom(Position bottomLeft, Position topRight)
        {
            Position.swapValuesByIncrementalOrder(ref bottomLeft, ref topRight);
            Room room = new Room(bottomLeft, topRight);
            BoxTexture vBox, hBox;
            createTextureBoxes(out hBox, out vBox);
            BoxTexture hBoxInvert = hBox.invertNorthSouth();
            BoxTexture vBoxInvert = vBox.invertEastWest();
            string[] uids = { "EAST" + ToolBox.randomString(5),
                              "NORTH" + ToolBox.randomString(5),
                              "WEST" + ToolBox.randomString(5), 
                              "SOUTH" + ToolBox.randomString(5)};
            //if (seperateEachWall == false)
            //    uids = new string[] { "global", "global", "global", "global" };
            //string[] uids = { ToolBox.randomString(), ToolBox.randomString(), ToolBox.randomString(), ToolBox.randomString() };
            for (int i = 0; i < room.Size.x; i++)
            {
                if (!isInside(room.BottomLeft.offsetBy(x: i)))
                { }
                WallPart newWall1 = queueWall(new WallPart(room.BottomLeft.offsetBy(x: i), wallThickness), ref room);
                newWall1.setMod(Directions.EAST, Mod.FULL, uids[Directions.SOUTH], hBox, false, owner:room);
                newWall1.setSep(Directions.EAST, uids[Directions.SOUTH]);
                newWall1.setSep(Directions.CENTER, uids[Directions.SOUTH]);
                WallPart newWall2 = queueWall(new WallPart(room.BottomLeft.offsetBy(x: i, z: room.Size.z), wallThickness), ref room);
                newWall2.setMod(Directions.EAST, Mod.FULL, uids[Directions.NORTH], hBoxInvert, true, owner: room);
                newWall2.setSep(Directions.EAST, uids[Directions.NORTH]);
                newWall2.setSep(Directions.CENTER, uids[Directions.NORTH]);
                if (i > 0)
                {
                    newWall1.setMod(Directions.WEST, Mod.FULL, uids[Directions.SOUTH], hBox, false, owner: room);
                    newWall1.setSep(Directions.WEST, uids[Directions.SOUTH]);
                    newWall2.setMod(Directions.WEST, Mod.FULL, uids[Directions.NORTH], hBoxInvert, true, owner: room);
                    newWall2.setSep(Directions.WEST, uids[Directions.NORTH]);
                }
                else
                {
                    newWall2.setMod(Directions.SOUTH, Mod.FULL, uids[Directions.WEST], vBox, false, owner: room);
                    newWall2.setSep(Directions.SOUTH, uids[Directions.WEST]);
                }
            }
            for (int i = 0; i < room.Size.z; i++)
            {
                WallPart newWall1 = queueWall(new WallPart(room.BottomLeft.offsetBy(z: i), wallThickness), ref room);
                newWall1.setMod(Directions.NORTH, Mod.FULL, uids[Directions.WEST], vBox, false, owner: room);
                newWall1.setSep(Directions.NORTH, uids[Directions.WEST]);
                newWall1.setSep(Directions.CENTER, uids[Directions.WEST]);
                WallPart newWall2 = queueWall(new WallPart(room.BottomLeft.offsetBy(x: room.Size.x, z: i), wallThickness), ref room);
                newWall2.setMod(Directions.NORTH, Mod.FULL, uids[Directions.EAST], vBoxInvert, true, owner: room);
                newWall2.setSep(Directions.NORTH, uids[Directions.EAST]);
                newWall2.setSep(Directions.CENTER, uids[Directions.EAST]);

                if (i > 0)
                {
                    newWall1.setMod(Directions.SOUTH, Mod.FULL, uids[Directions.WEST], vBox, false, owner: room);
                    newWall1.setSep(Directions.SOUTH, uids[Directions.WEST]);
                    newWall2.setMod(Directions.SOUTH, Mod.FULL, uids[Directions.EAST], vBoxInvert, true, owner: room);
                    newWall2.setSep(Directions.SOUTH, uids[Directions.EAST]);
                }
                else
                {
                    newWall2.setMod(Directions.WEST, Mod.FULL, uids[Directions.SOUTH], hBox, false, owner: room);
                    newWall2.setSep(Directions.WEST, uids[Directions.SOUTH]);
                }
            }
            WallPart finalWall = queueWall(new WallPart(topRight, wallThickness), ref room);
            finalWall.setMod(Directions.WEST, Mod.FULL, uids[Directions.NORTH], hBoxInvert, true, owner: room);
            finalWall.setMod(Directions.SOUTH, Mod.FULL, uids[Directions.EAST], vBoxInvert, true, owner: room);
            finalWall.setSep(Directions.WEST, uids[Directions.NORTH]);
            finalWall.setSep(Directions.SOUTH, uids[Directions.EAST]);
            rooms.Add(room);
            return room;
        }

        public Room addRoom(Room room)
        {
            return addRoom(room.BottomLeft, room.BottomLeft.offsetBy(room.Size));
        }

        /// <summary>
        /// Adds a set of walls between p1 and p2. Positions must be vertically or horizontally aligned
        /// </summary>
        public void addWall(Position p1, Position p2)
        {
            if (!p1.isAlignedXorZ(p2))
                throw (new System.Exception("Error adding wall: the 2 provided positions are not vertically or horizontally aligned"));
            Position.swapValuesByIncrementalOrder(ref p1, ref p2);
            int length = (p2.x - p1.x) + (p2.z - p1.z);
            Directions dir = Directions.NORTH;
            if (p2.x - p1.x != 0)
                dir = Directions.EAST;
            addWall(p1, dir, length);
        }

        /// <summary>
        /// removes a set of walls. Positions must be vertically or horizontally aligned
        /// </summary>
        public void addWall(Position position, Directions direction, int length)
        {
            string sepID = ToolBox.randomString();
            if (direction.isEither(Directions.SOUTH, Directions.EAST))
            {
                position = position.offsetBy(direction, length);
                direction = Directions.oppositeOf(direction);
            }
            addWallPart(position, sepID, direction, true);
            position = position.offsetBy(direction, 1);
            int i;
            for (i = 1; i < length; i++)
            {
                addWallPart(position, sepID, direction);
                position = position.offsetBy(direction, 1);
            }
            addWallPart(position, sepID, direction, false, true);
        }

        /// <summary>
        /// Removes a set of walls between p1 and p2. Positions must be vertically or horizontally aligned
        /// </summary>
        public void removeWall(Position p1, Position p2)
        {
            if (!p1.isAlignedXorZ(p2))
                throw (new System.Exception("Error adding wall: the 2 provided positions are not vertically or horizontally aligned"));
            Position.swapValuesByIncrementalOrder(ref p1, ref p2);
            int length = (p2.x - p1.x) + (p2.z - p1.z);
            Directions dir = Directions.NORTH;
            if (p2.x - p1.x != 0)
                dir = Directions.EAST;
            Debug.Log(p1.ToString() + " - " + dir.ToString() + " - " + length);
            addWall(p1, dir, length);
        }

        /// <summary>
        /// Removes a set of walls. Positions must be vertically or horizontally aligned
        /// </summary>
        public void removeWall(Position position, Directions direction, int length)
        {
            if (direction.isEither(Directions.SOUTH, Directions.EAST))
            {
                position = position.offsetBy(direction, length);
                direction = Directions.oppositeOf(direction);
            }
            removeWallPart(position, direction, true);
            position = position.offsetBy(direction, 1);
            int i;
            for (i = 1; i < length; i++)
            {
                removeWallPart(position, direction);
                position = position.offsetBy(direction, 1);
            }
            removeWallPart(position, direction, false, true);
        }

        void addWallPart(Position position, string sepID, Directions direction, bool first = false, bool last = false)
        {
            Directions d1 = (direction.isEither(Directions.NORTH, Directions.SOUTH) ? Directions.NORTH : Directions.WEST);
            Directions d2 = (direction.isEither(Directions.NORTH, Directions.SOUTH) ? Directions.SOUTH : Directions.EAST);
            WallPart w = null;
            if (WallList.ContainsKey(position))
                w = WallList[position];
            else
                w = new WallPart(position, wallThickness);
            if (!last)
                w.setMod(d1, Mod.FULL, sepID, new BoxTexture(), false);
            if (!first)
                w.setMod(d2, Mod.FULL, sepID, new BoxTexture(), false);
            WallPart finalW = queueWall(w);
        }

        void removeWallPart(Position position, Directions direction, bool first = false, bool last = false)
        {
            Directions d1 = (direction.isEither(Directions.NORTH, Directions.SOUTH) ? Directions.NORTH : Directions.WEST);
            Directions d2 = (direction.isEither(Directions.NORTH, Directions.SOUTH) ? Directions.SOUTH : Directions.EAST);
            WallPart w = null;
            if (WallList.ContainsKey(position))
                w = WallList[position];
            else
                return;
            if (first)
                w.setMod(d1, Mod.NONE, "_IGNORE_", new BoxTexture(), false);
            else if (last)
                w.setMod(d2, Mod.NONE, "_IGNORE_", new BoxTexture(), false);
            else
                unqueueWall(w);
        }



        /// <summary>
        /// Merges two rooms by deleting any crossing walls
        /// </summary>
        /// <param name="room1"></param>
        /// <param name="room2"></param>
        public void mergeRooms(Room room1, Room room2)
        {
            if (!room1.Overlaps(room2))
            {
                nonOverlappingMerge(room1, room2);
                return;
            }
            List<WallPart> toRemove = new List<WallPart>();
            List<WallPart> joints = new List<WallPart>();
            foreach (WallPart w in room1.walls.Values)
            {
                if (room2.walls.ContainsKey(w.position))
                {
                    joints.Add(w);
                    continue;
                }
                if (isInsideRoom(w.position, room2, 0))
                {
                    WallList.Remove(w.position);
                    if (!toRemove.Contains(w))
                        toRemove.Add(w);
                }
            }
            foreach (WallPart w in room2.walls.Values)
            {
                if (room1.walls.ContainsKey(w.position))
                {
                    joints.Add(w);
                    continue;
                }
                if (isInsideRoom(w.position, room1, 0))
                {
                    WallList.Remove(w.position);
                    if (!toRemove.Contains(w))
                        toRemove.Add(w);
                }
            }
            foreach (WallPart p in toRemove)
            {
                if (room1.walls.ContainsKey(p.position))
                    room1.walls.Remove(p.position);
                if (room2.walls.ContainsKey(p.position))
                    room2.walls.Remove(p.position);
            }
            foreach (WallPart p in joints)
            {
                connectRoomJoints(p.position);
            }
        }

        void connectRoomJoints(Position p)
        {

            WallList[p].mods[Directions.NORTH] = Mod.NONE;
            WallList[p].mods[Directions.SOUTH] = Mod.NONE;
            WallList[p].mods[Directions.EAST] = Mod.NONE;
            WallList[p].mods[Directions.WEST] = Mod.NONE;
            if (WallList.ContainsKey(WallList[p].position.offsetBy(z:1)))
                WallList[p].mods[Directions.NORTH] = Mod.FULL;
            if (WallList.ContainsKey(WallList[p].position.offsetBy(z:-1)))
                WallList[p].mods[Directions.SOUTH] = Mod.FULL;
            if (WallList.ContainsKey(WallList[p].position.offsetBy(1)))
                WallList[p].mods[Directions.EAST] = Mod.FULL;
            if (WallList.ContainsKey(WallList[p].position.offsetBy(-1)))
                WallList[p].mods[Directions.WEST] = Mod.FULL;

        }

        void nonOverlappingMerge(Room room1, Room room2)
        {
            foreach (WallPart r in room2.walls.Values)
            {
                for (int i = 0; i < r.mods.Length; i++)
                {
                    if (r.mods[i].ownerRooms.Contains(room1) && r.mods[i].ownerRooms.Contains(room2))
                        r.setMod(i, Mod.NONE, "_IGNORE_", new BoxTexture(), false, false, null);
                }
                if (r.onlyCenterIsActive())
                    r.setMod(Directions.CENTER, Mod.NONE, "_IGNORE_", new BoxTexture(), false);
            }
        }

        /// <summary>
        /// Sets textures for every generated meshes in the building.
        /// You can set a particular texture by using the following: setTextures(floorTexture:4)
        /// See generation examples for specific uses
        /// </summary>
        /// <param name="indoorTexture"></param>
        /// <param name="outdoorTexture"></param>
        /// <param name="floorTexture"></param>
        /// <param name="stairSteptexture"></param>
        /// <param name="stairRailTexture"></param>
        /// <param name="roofTexture"></param>
        /// <param name="edgeTexture"></param>
        /// <param name="ceilingTexture"></param>
        public void setTextures(int indoorTexture = -1,
                            int outdoorTexture = -1,
                            int floorTexture = -1,
                            int stairSteptexture = -1,
                            int stairRailTexture = -1,
                            int roofTexture = -1,
                            int edgeTexture = -1,
                            int ceilingTexture = -1,
                            int foundationTexture = -1,
                            bool applyToChildren = false)
        {
            int max = Mathf.Max(indoorTexture, outdoorTexture, floorTexture,
                                stairSteptexture, stairRailTexture, roofTexture,
                                roofTexture, edgeTexture, ceilingTexture, foundationTexture);
            if (max >= 0 && max > materials.Length)
                throw new System.Exception("setTextures(): one of the material id is over the length of the material list (" + max + ")");
            if (indoorTexture != -1)
                currentIndoorTex = indoorTexture;
            if (outdoorTexture != -1)
                currentOutdoorTex = outdoorTexture;
            if (floorTexture != -1)
                currentFloorTex = floorTexture;
            if (stairSteptexture != -1)
                currentStairStepTex = stairSteptexture;
            if (stairRailTexture != -1)
                currentRailTex = stairRailTexture;
            if (roofTexture != -1)
                currentRoofTex = roofTexture;
            if (edgeTexture != -1)
                currentEdgeTex = edgeTexture;
            if (ceilingTexture != -1)
                currentCeilingTex = ceilingTexture;
            if (foundationTexture != -1)
                currentFoundation = foundationTexture;
            if (applyToChildren)
            {
                RoomArchitect[] chil = transform.GetComponentsInChildren<RoomArchitect>();
                foreach (RoomArchitect c in chil)
                {
                    if (c != this)
                        c.setTextures(indoorTexture, outdoorTexture, floorTexture, stairSteptexture,
                                      stairRailTexture, roofTexture, edgeTexture, ceilingTexture);
                }

            }
        }

        public void applyMaterialsToChildren()
        {
            RoomArchitect[] chil = transform.GetComponentsInChildren<RoomArchitect>();
            foreach (RoomArchitect c in chil)
            {
                if (c != this)
                    c.materials = this.materials;
            }
        }

        /// <summary>
        /// Adds a fence to the model
        /// </summary>
        /// <param name="position">Starting point of the fence in Unity units</param>
        /// <param name="length">total length in Unity units</param>
        /// <param name="direction">Cardinal direction</param>
        /// <param name="inBetweenBarAmount">Number of bars inbetween the extremeties</param>
        public void addFence(Vector3 position, float length, Directions direction, int inBetweenBarAmount)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(objectHolderManager.getHolder(findFloor(position.y)).transform, false);
            go.transform.localPosition = position;
            ProceduralObject po = go.AddComponent<ProceduralObject>();
            po.init(this, materials);
            po.addSingleFence(Vector3.zero, length, direction, inBetweenBarAmount, currentRailTex);
            po.createMesh();
            go.AddComponent<BoxCollider>();
            fences.Add(new Fence(position, length, direction, inBetweenBarAmount));
            go.name = "fence";
        }

        /// <summary>
        /// Adds a fence to the model
        /// </summary>
        /// <param name="position">Starting point of the fence in cell units</param>
        /// <param name="length">total length in Unity units</param>
        /// <param name="direction">Cardinal direction</param>
        /// <param name="inBetweenBarAmount">Number of bars inbetween the extremeties</param>
        public void addFence(Position position, float length, Directions direction, int inBetweenBarAmount)
        {
            addFence(position.toVector3(RealDimensionsVector), length * HorizontalScale, direction, inBetweenBarAmount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <param name="direction"></param>
        /// <param name="inBetweenBarAmount"></param>
        public void addFence(Position position, int length, Directions direction, int inBetweenBarAmount)
        {
            addFence(position.toVector3(RealDimensionsVector), length * HorizontalScale, direction, inBetweenBarAmount);
        }

        /// <summary>
        /// adds a piece of floor to the building. The rectangles will be placed according to it's lower left corner
        /// </summary>
        /// <param name="rect"></param>
        public void addFloor(Rectangle rect)
        {
            setFloorTexture();
            addFloorBigTileMesh(rect);
        }

        /// <summary>
        /// adds a piece of floor to the building. The rectangles will be placed according to it's lower left corner
        /// </summary>
        /// <param name="rect"></param>
        public void addFloor(Position bottomLeft, Position topRight)
        {
            Position.swapValuesByIncrementalOrder(ref bottomLeft, ref topRight);
            setFloorTexture();
            addFloorBigTileMesh(new Rectangle(bottomLeft, topRight - bottomLeft));
        }

        /// <summary>
        /// This method creates a buffer beneath the house and elevate the whole model
        /// </summary>
        /// <param name="elevation"></param>
        public void setElevation(float elevation)
        {
            this.elevation = elevation;
        }

        /// <summary>
        /// Stairs can have different styles based on the enumerator RAILSTYLE.
        /// You can also specify if the stair must be connected to the ground through all its length with the boolean "fullBottom"
        /// </summary>
        /// <param name="railStyle"></param>
        /// <param name="fullBottom"></param>
        public void setStairStyle(RAILSTYLE railStyle = RAILSTYLE.FULL, bool fullBottom = false)
        {
            StairStyle.Instance.fullBottom = fullBottom;
            StairStyle.Instance.railStyle = railStyle;
        }

        /// <summary>
        /// Sets a style for every following roof after the call. Note that "Height" and "flatTopeSize" parameters will
        /// be considered only for "default" roof type
        /// </summary>
        /// <param name="roofType">Type of the roof. ROOFTYPE.NONE will cancel the generation</param>
        /// <param name="height">height of the roof based on its width. height = 1 will generate a 45� angled roof</param>
        /// <param name="flatTopSize">Percentage of flat space between the 2 slopes.</param>
        public void setRoofStyle(ROOFTYPE roofType, float height = 0.8f, float flatTopSize = 0.03f)
        {
            roofHeight = height;
            this.roofFlatSize = flatTopSize;
            this.roofcurrStyle = roofType;
        }

        public void duplicateEntireFloor(int floorToDuplicate, int floor)
        {
            if (!floorsToDuplicate.ContainsKey(floor))
                floorsToDuplicate.Add(floor, floorToDuplicate);
        }
    }
}
