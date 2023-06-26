using UnityEngine;
using System.Collections;


namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        /// <summary>
        /// Places a door through a wall junction
        /// Example: if a door is added at [O;O;O] with direction north, the door will be placed between
        /// [0;0;0] and [0;0;1]
        /// </summary>
        /// <param name="position">position of the door</param>
        /// <param name="direction">Direction to which the door is parallel to</param>
        /// <param name="inverted">If true, this parameters allows flipping the door 180° by its center</param>
        /// <param name="doorID">ID corresponding to doors in the door's list on the unity interface</param>
        public void addDoor(Position position, Directions direction, bool inverted = false, int doorID = DEFAULT)
        {
            if (doorModels.Length <= 0)
                throw new System.Exception("Trying to add a door but the the door list is empty");
            placeWallObject(position, direction, Mod.DOOR, ref doorModels, 0, inverted, doorID);
        }

        /// <summary>
        /// Places a door through a wall junction
        /// Example: if a door is added between [O;O;O] and [0;0;1], it will be placed in the walls between those points,
        /// parralel to the north direction.
        /// </summary>
        /// <param name="wall1">first wall</param>
        /// <param name="wall2">second wall</param>
        /// <param name="inverted">If true, this parameters allows flipping the door 180° by its center</param>
        /// <param name="doorID">ID corresponding to doors in the door's list on the unity interface</param>
        public void addDoor(WallPart wall1, WallPart wall2, bool inverted = false, int doorID = DEFAULT)
        {
            if (doorModels.Length <= 0)
                throw new System.Exception("Trying to add a door but the the door list is empty");
            placeWallObject(wall1, wall2, Mod.DOOR, ref doorModels, 0, inverted, doorID);
        }

        /// <summary>
        /// Places a window through a wall junction
        /// Example: if a window is added at [O;O;O] with direction north, the window will be placed between
        /// [0;0;0] and [0;0;1]
        /// </summary>
        /// <param name="position">position of the door</param>
        /// <param name="direction">Direction to which the door is parallel to</param>
        /// <param name="inverted">If true, this parameters allows flipping the window 180° by its center</param>
        /// <param name="doorID">ID corresponding to windows in the window's list on the unity interface</param>
        public void addWindow(Position position, Directions direction, float customHeightBuffer = 0, bool inverted = false, int windowID = DEFAULT)
        {
            if (WindowModels.Length <= 0)
                throw new System.Exception("Trying to add a window but the the window list is empty");
            placeWallObject(position, direction, Mod.WINDOW, ref WindowModels, customHeightBuffer, inverted, windowID);
        }

        /// <summary>
        /// Places a window through a wall junction
        /// Example: if a window is added between [O;O;O] and [0;0;1], it will be placed in the walls between those points,
        /// parralel to the north direction.
        /// </summary>
        /// <param name="wall1">first wall</param>
        /// <param name="wall2">second wall</param>
        /// <param name="inverted">If true, this parameters allows flipping the window 180° by its center</param>
        /// <param name="doorID">ID corresponding to windows in the window's list on the unity interface</param>
        public void addWindow(WallPart wall1, WallPart wall2, float customHeightBuffer, bool inverted = false, int windowID = DEFAULT)
        {
            if (WindowModels.Length <= 0)
                throw new System.Exception("Trying to add a window but the the window list is empty");
            placeWallObject(wall1, wall2, Mod.WINDOW, ref WindowModels, customHeightBuffer, inverted, windowID);
        }

        /// <summary>
        /// Places a wide opening through several walls
        /// Example: if an entrance is added between [O;O;O] and [0;0;7], the function will create the equivalent of
        /// a wide door opening (like a garage door for example) into all the wall junctions present between the two points
        /// the GameObject (being optionnal) will be placed at the bottom center of the opening
        /// </summary>
        /// <param name="wall1">first wall</param>
        /// <param name="wall2">last wall</param>
        /// <param name="door">Optional GameObject to place into the entrance, at the bottom center</param>
        /// <param name="inverted">If true, this parameters allows flipping the window 180° by its center</param>
        public void addWideEntrance(Position wall1, Position wall2, GameObject door = null, bool inverted = false)
        {
            try
            {
                placeWideEntranceHole(wall1, wall2);
                if (door != null)
                {
                    Vector3 distance = (wall2 - wall1).toAbsoluteValue().toVector3(RealDimensionsVector);
                    GameObject tmp = GameObject.Instantiate(door) as GameObject;
                    tmp.transform.SetParent(objectHolderManager.getHolder(wall1.y).transform, false);
                    tmp.transform.localPosition = wall1.toVector3(RealDimensionsVector) + (0.5f * distance);
                    if (distance.z != 0)
                    {
                        tmp.transform.Rotate(Vector3.up, 90);
                    }
                    if (inverted)
                        tmp.transform.Rotate(Vector3.up, 180);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error placing the door for wide entrance between " + wall1.ToString() + " - " + wall2.ToString() + " - " + e.Message);
            }
        }

        void placeWallObject(Position position, Directions direction, Mod mod, ref GameObject[] objectList, float customHeightBuffer = 0, bool inverted = false, int windowID = DEFAULT)
        {
            try
            {
                if (direction == Directions.NORTH)
                    placeHole(position, position.offsetBy(z: 1), mod);
                else if (direction == Directions.SOUTH)
                    placeHole(position, position.offsetBy(z: -1), mod);
                else if (direction == Directions.EAST)
                    placeHole(position, position.offsetBy(x: 1), mod);
                else if (direction == Directions.WEST)
                    placeHole(position, position.offsetBy(x: -1), mod);
                instantiateWAllObject(position, direction, mod, ref objectList, customHeightBuffer, inverted, windowID);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Placing window error (pos:" + position.ToString() + " - direction:" + direction.ToString() + " - " + e.Message);
            }
        }

        void placeWallObject(WallPart wall1, WallPart wall2, Mod mod, ref GameObject[] objectList, float customHeightBuffer = 0, bool inverted = false, int windowID = DEFAULT)
        {
            if (wall1.position == wall2.position.offsetBy(z: 1))
                placeWallObject(wall1.position, Directions.NORTH, mod, ref objectList, customHeightBuffer, inverted, windowID);
            else if (wall1.position == wall2.position.offsetBy(z: -1))
                placeWallObject(wall1.position, Directions.SOUTH, mod, ref objectList, customHeightBuffer, inverted, windowID);
            else if (wall1.position == wall2.position.offsetBy(x: 1))
                placeWallObject(wall1.position, Directions.EAST, mod, ref objectList, customHeightBuffer, inverted, windowID);
            else if (wall1.position == wall2.position.offsetBy(x: -1))
                placeWallObject(wall1.position, Directions.WEST, mod, ref objectList, customHeightBuffer, inverted, windowID);
            else
                Debug.LogWarning("Placing window error (pos:" + wall1.position.ToString() + " - Walls are not adjacent");
        }

        void instantiateWAllObject(Position position, Directions direction, Mod mod, ref GameObject[] objectList, float customHeightBuffer = 0, bool inverted = false, int objectID = DEFAULT)
        {
            GameObject window = GameObject.Instantiate(objectList[objectID]) as GameObject;
            window.transform.SetParent(objectHolderManager.getHolder(position.y).transform, false);
            Vector3 posShift = Vector3.zero;
            float height = (mod == Mod.WINDOW ? WindowHeightFromFloor : 0);
            posShift += (direction == Directions.NORTH ? new Vector3(0, height, HorizontalScale * 0.5f) : Vector3.zero);
            posShift += (direction == Directions.SOUTH ? new Vector3(0, height, -HorizontalScale * 0.5f) : Vector3.zero);
            posShift += (direction == Directions.EAST ? new Vector3(HorizontalScale * 0.5f, height, 0) : Vector3.zero);
            posShift += (direction == Directions.WEST ? new Vector3(-HorizontalScale * 0.5f, height, 0) : Vector3.zero);
            window.transform.localPosition = position.toVector3(RealDimensionsVector) + posShift;
            if (direction == Directions.NORTH)
                window.transform.Rotate(Vector3.up, 90);
            if (direction == Directions.WEST)
                window.transform.Rotate(Vector3.up, 180);
            if (direction == Directions.SOUTH)
                window.transform.Rotate(Vector3.up, 270);
            if (customHeightBuffer > 0)
            {
                BoxTexture boxTex = new BoxTexture(north: WallList[position].textures[direction].textureID[direction.nextCounterClockWise()],
                                                   south: WallList[position].textures[direction].textureID[direction.nextClockWise()],
                                                   rest:currentEdgeTex);
                ProceduralObject buffer = new GameObject().AddComponent<ProceduralObject>();
                buffer.init(this, materials);
                buffer.setTextureMode(TEXTUREMODE.BOX, RealDimensionsVector, new Vector2(0.5f, windowHeightFromFloor));
                buffer.transform.SetParent(globalWallMeshHolder.transform, false);
                buffer.transform.Translate(window.transform.localPosition);
                buffer.name = "window buffer";
                if (direction == Directions.NORTH)
                    buffer.transform.Rotate(Vector3.up, 90);
                if (direction == Directions.WEST)
                    buffer.transform.Rotate(Vector3.up, 180);
                if (direction == Directions.SOUTH)
                    buffer.transform.Rotate(Vector3.up, 270);
                buffer.addBox(Vector3.zero, new Vector3(FrameWidth, WindowHeight - customHeightBuffer, wallThickness), boxTex, "");
                buffer.createMesh();
                window.transform.Translate(0, customHeightBuffer, 0);
                MeshCollider collider = buffer.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
            }


        }

        /// <summary>
        /// Places a hole into a wall
        /// Conditions:
        ///  - The positions wall1 and wall2 MUST be at a position of an existing wall
        ///  - The positions wall1 and wall2 MUST either be on the same X, or the same Z
        ///  - The positions wall1 and wall2 MUST be adjacent
        /// </summary>
        /// <param name="wall1"></param>
        /// <param name="wall2"></param>
        /// <param name="modType"></param>
        void placeHole(Position wall1, Position wall2, Mod modType)
        {
            if (!WallList.ContainsKey(wall1))
                throw new System.Exception("Placing hole: The wall position passed as 'wall1' parameter does not exist");
            else if (!WallList.ContainsKey(wall2))
                throw new System.Exception("Placing hole: The wall position passed as 'wall2' parameter does not exist");
            else if ((wall2 - wall1).toAbsoluteValue().XZSum() > 1 || wall1 == wall2)
                throw new System.Exception("Placing hole: The walls are not adjacents");
            else if (wall1.y != wall2.y)
                throw new System.Exception("Placing hole: The walls are not on the same floor");
            if (wall1.x > wall2.x)
                Position.swap(ref wall1, ref wall2);
            else if (wall1.z > wall2.z)
                Position.swap(ref wall1, ref wall2);
            if (wall1.x == wall2.x)
            {
                WallList[wall1].mods[Directions.NORTH] = modType;
                WallList[wall2].mods[Directions.SOUTH] = modType;
            }
            else if (wall1.z == wall2.z)
            {
                WallList[wall1].mods[Directions.EAST] = modType;
                WallList[wall2].mods[Directions.WEST] = modType;
            }
            else
            {
                Debug.LogWarning("Uknown error during the placing of a hole");
            }
        }


        /// <summary>
        /// Places a wide Entrance into a wall, for garage entrances for example
        /// Conditions:
        ///  - The positions wall1 and wall2 MUST be at a position of an existing wall
        ///  - The positions wall1 and wall2 MUST either be on the same X, or the same Z
        ///  - The positions wall1 and wall2 may not be directly adjacent
        /// </summary>
        /// <param name="wall1"></param>
        /// <param name="wall2"></param>
        /// <param name="modType"></param>
        void placeWideEntranceHole(Position wall1, Position wall2)
        {
            if (!WallList.ContainsKey(wall1))
                throw new System.Exception("Placing hole: The wall position passed as 'wall1' parameter does not exist");
            else if (!WallList.ContainsKey(wall2))
                throw new System.Exception("Placing hole: The wall position passed as 'wall2' parameter does not exist");
            else if (wall1.y != wall2.y)
                throw new System.Exception("Placing hole: The walls are not on the same floor");
            if (wall1.x > wall2.x)
                Position.swap(ref wall1, ref wall2);
            else if (wall1.z > wall2.z)
                Position.swap(ref wall1, ref wall2);
            if (wall1.x == wall2.x)
            {
                try
                {
                    Position incr = wall1.offsetBy(z: 1);
                    WallList[wall1].mods[Directions.NORTH] = Mod.DOOR;
                    while (incr != wall2 && incr.z < 250)
                    {
                        WallList[incr].makeArchThroughZ();
                        incr.z++;
                    }
                    WallList[wall2].mods[Directions.SOUTH] = Mod.DOOR;
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Placing wide Entrance: Wall might not be continuous: " + e.Message);
                }
            }
            else if (wall1.z == wall2.z)
            {
                try
                {
                    Position incr = wall1.offsetBy(x: 1);
                    WallList[wall1].mods[Directions.EAST] = Mod.DOOR;
                    while (incr != wall2 && incr.x < 250)
                    {
                        WallList[incr].makeArchThroughX();
                        incr.x++;
                    }
                    WallList[wall2].mods[Directions.WEST] = Mod.DOOR;
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Placing wide Entrance: Wall might not be continuous: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("Uknown error during the placing of a hole");
            }
        }

    }
}
