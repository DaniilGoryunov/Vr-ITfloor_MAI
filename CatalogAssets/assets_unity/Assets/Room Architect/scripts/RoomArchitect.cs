using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        public bool generateChildren = true;
        public bool optimizeMesh = true;
        public bool seperateByFloor = true;
        public bool seperateEachWall = false;
        public bool mergeWallObjects = false;
        [Range(0, 2)] public int agressiveOptimization = 0;
        public float HorizontalScale = 1;
        public float VerticalScale = 2.2f;
        public float FrameWidth = 0.9f;
        public float WindowHeightFromFloor = 1f;
        public float WindowHeight = 1f;
        public GameObject[] WindowModels;
        public GameObject[] doorModels;
        [HideInInspector] public List<Room> rooms;
        [HideInInspector] public List<Fence> fences;
        [HideInInspector] public List<FloorTile> floorTiles;
        Dictionary<Position, WallPart> WallList;
        public Position bottomLeft, topRight;
        public float wallThickness = 0.08f, floorThickness = 0.08f;
        //List<ProceduralObject> WallMeshes;
        public GlobalWallMesh WallMesh;
        public const int DEFAULT = 0;
        public Material[] materials;
        int currentIndoorTex = 0;
        int currentOutdoorTex = 0;
        int currentStairStepTex = 0;
        int currentRailTex = 0;
        int currentRoofTex = 0;
        int currentEdgeTex = 0;
        int currentFloorTex = 0;
        int currentCeilingTex = 0;
        int currentFoundation = 0;
        public double measuredFunctionTimer = 0;
        static float roofHeightMultiplier = 4;
        BoxTexture currentTexturingScheme;
        [HideInInspector] public GameObject globalWallMeshHolder;
        public Dictionary<int, GameObject> wallMeshHolder;
        [HideInInspector] public GameObject objectHolder;
        [HideInInspector] public ObjectHolderManager objectHolderManager;
        GameObject roofHolder;
        public GameObject hitboxHolder;
        public HitBoxManager hitBoxManager;
        public float elevation = 0;
        float roofHeight = 0.8f;
        float roofFlatSize = 0.03f;
        bool dontBuild = false;
        ROOFTYPE roofcurrStyle = ROOFTYPE.DEFAULT;
        Dictionary<int, int> floorsToDuplicate;

        /// <summary>
        /// This method must be overriden, it represents the blue print of the building 
        /// </summary>
        public virtual void buildTemplate()
        {

        }

        float windowHeightFromFloor
        {
            get
            {
                if (WindowHeightFromFloor + WindowHeight > VerticalScale)
                    throw new System.Exception("(WindowHeightFromFloor + WindowHeight) is greater than the vertical scale");
                float leftOver = VerticalScale - WindowHeight - WindowHeightFromFloor;
                float res = (VerticalScale - WindowHeight - leftOver) / VerticalScale;
                return res;
            }
        }

        float windowHeight
        {
            get
            {
                if (WindowHeightFromFloor + WindowHeight > VerticalScale)
                    throw new System.Exception("(WindowHeightFromFloor + WindowHeight) is greater than the vertical scale");
                float leftOver = VerticalScale - WindowHeight - WindowHeightFromFloor;
                float res = (VerticalScale - WindowHeightFromFloor - leftOver) / VerticalScale;
                return res;
            }
        }

        float FrameBufferWidth
        {
            get
            {
                return (HorizontalScale - FrameWidth - wallThickness) * 0.5f;

            }
        }

        /// <summary>
        /// return a new Vector3 as following : new Vector3(HorizontalScale, VerticalScale, HorizontalScale)
        /// </summary>
        public Vector3 RealDimensionsVector
        {
            get
            {
                return new Vector3(HorizontalScale, VerticalScale, HorizontalScale);
            }
        }

        /*ProceduralObject WallMesh
        {
            get
            {
                if (WallMeshes == null)
                    WallMeshes = new List<ProceduralObject>();
                if (WallMeshes.Count <= 0)
                    addNewSubmesh();
                else if (WallMeshes[WallMeshes.Count - 1] == null)
                {
                    WallMeshes[WallMeshes.Count - 1] = (new GameObject()).AddComponent<ProceduralObject>();
                    ProceduralObject newPO = WallMeshes[WallMeshes.Count - 1];
                    newPO.init(this, materials);
                    newPO.optimizeWallMode = true;
                    newPO.setTextureMode(TEXTUREMODE.BOX, RealDimensionsVector);
                    newPO.transform.SetParent(wallMeshHolder.transform, false);
                    newPO.gameObject.name = "subMesh_" + (WallMeshes.Count - 1).ToString();
                }
                if (WallMeshes[WallMeshes.Count - 1].IsFull)
                    addNewSubmesh();
                return WallMeshes[WallMeshes.Count - 1];
            }
        }*/

        /*void addNewSubmesh()
        {
            WallMeshes.Add((new GameObject()).AddComponent<ProceduralObject>());
            ProceduralObject newPO = WallMeshes[WallMeshes.Count - 1];
            newPO.init(this, materials);
            newPO.optimizeWallMode = true;
            newPO.setTextureMode(TEXTUREMODE.BOX, RealDimensionsVector);
            newPO.transform.SetParent(wallMeshHolder.transform, false);
            newPO.gameObject.name = "subMesh_" + (WallMeshes.Count - 1).ToString();
        }*/

        /// <summary>
        /// Calculated total size of the generated building (topRight(x;y;z) - bottomLeft(x;y;z))
        /// </summary>
        Position totalSize
        {
            get
            {
                return topRight - bottomLeft;
            }
        }

        public void Awake()
        {
        }


        /// <summary>
        /// To call before starting building
        /// </summary>
        public void init()
        {
            clear();
            wallMeshHolder = new Dictionary<int, GameObject>();
            WallMesh = new GlobalWallMesh(this, materials);
            floorsToDuplicate = new Dictionary<int, int>();
            createHolder(ref globalWallMeshHolder, "wall meshes");
            createHolder(ref objectHolder, "wall objects");
            createHolder(ref roofHolder, "roofs");
            createHolder(ref hitboxHolder, "hitboxes");
            hitBoxManager = new HitBoxManager(this);
            objectHolderManager = new ObjectHolderManager(this);
            WallList.Clear();
            rooms.Clear();
            roof = new InternalRoof();
            ceilingTiles = new List<Position>();
            recordFloorHeight = 0;
            roofTileCount = 0;
            bottomLeft = Position.One * int.MaxValue;
            topRight = Position.One * int.MinValue;
        }

        public void clear()
        {
            floorTileCount = 0;
            groundSurface = new List<Rectangle>();
            rooms = new List<Room>();
            fences = new List<Fence>();
            floorTiles = new List<FloorTile>();
            mergedFloorTiles = new List<Rectangle>();
            floorsToDuplicate = new Dictionary<int, int>();
            WallList = new Dictionary<Position, WallPart>();
            if (stairs != null)
                stairs.Clear();
            if (doubleStairs!= null)
                doubleStairs.Clear();
            BoxCollider[] colliders = GetComponents<BoxCollider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                GameObject.DestroyImmediate(colliders[i]);
            }
            try
            {
                foreach (GameObject g in wallMeshHolder.Values)
                    GameObject.DestroyImmediate(g);
                GameObject.DestroyImmediate(globalWallMeshHolder);
            }
            catch { }
            try
            {
                GameObject.DestroyImmediate(objectHolder);
            }
            catch { }
            try
            {
                GameObject.DestroyImmediate(roofHolder);
            }
            catch { }
            try
            {
                GameObject.DestroyImmediate(hitboxHolder);
            }
            catch { }
            hitBoxManager = new HitBoxManager(this);
            Transform toRemove = null;
            while ((toRemove = transform.Find("wall meshes")) != null)
                GameObject.DestroyImmediate(toRemove.gameObject);
            while ((toRemove = transform.Find("wall objects")) != null)
                GameObject.DestroyImmediate(toRemove.gameObject);
            while ((toRemove = transform.Find("roofs")) != null)
                GameObject.DestroyImmediate(toRemove.gameObject);
            while ((toRemove = transform.Find("hitboxes")) != null)
                GameObject.DestroyImmediate(toRemove.gameObject);
            if (generateChildren)
            {
                RoomArchitect[] chil = transform.GetComponentsInChildren<RoomArchitect>();
                foreach (RoomArchitect c in chil)
                {
                    if (c != this)
                        c.clear();
                }
            }
            WallMesh = null;
            objectHolderManager = null;
        }

        public void createHolder(ref GameObject holder, string name)
        {
            try
            {
                GameObject.DestroyImmediate(holder);
            }
            catch { }
            holder = new GameObject(name);
            holder.transform.SetParent(this.transform, false);
        }

        /// <summary>
        /// To call after all made into HouseBuilderTemplate.
        /// Refer to documentation section "Building Process" for more information
        /// WARNING: This method should not be called inside buildTemplate();
        /// </summary>
        public void Build()
        {
            StartCoroutine(asyncBuild());
            if (generateChildren)
            {
                RoomArchitect[] child = transform.GetComponentsInChildren<RoomArchitect>();
                foreach (RoomArchitect c in child)
                {
                    if (c != this)
                        c.Build();
                }
            }
            measuredFunctionTimer = 0;
        }

        public void DontBuild()
        {
            dontBuild = true;
        }

        Vector3 SavePos;
        Quaternion saveRot;
        Transform saveParent;
        public IEnumerator asyncBuild()
        {
            SavePos = transform.localPosition;
            saveRot = transform.localRotation;
            saveParent = transform.parent;
            transform.SetParent(null);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            float t1 = Time.realtimeSinceStartup;
            if (materials == null || materials.Length <= 0)
                throw new System.Exception("No materials assigned to the Building. Please add at least one material to the inspector list");
            init();
            if (Application.isPlaying)
                yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0);
            dontBuild = false;
            buildTemplate();
            if (dontBuild)
            {
                resetOldPos();
                yield break;
            }
            if (Application.isPlaying)
            {
                // hard copy pasting dequeueWallsToDrawingPipeline() here as an async task for performance reasons
                // Basically it is strongly discouraged to nest async methods. So there.
                int i = 0;
                foreach (WallPart p in WallList.Values)
                {
                    i++;
                    putWall(p); 
                    if (i > 500)
                    {
                        i = 0;
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
            else
                dequeueWallsToDrawingPipeline();
            depileMeshPane();
            if (Application.isPlaying)
                yield return new WaitForEndOfFrame();
            ceilingTilesBoard = new bool[bottomLeft.x + totalSize.x + 1, bottomLeft.y + totalSize.y + 2, bottomLeft.z + totalSize.z + 1];
            try {
                buildFloors();
            }
            catch (System.Exception e){
                Debug.LogError("Error during buildFloors(); " + e.Message);
                yield break;
            }
            if (Application.isPlaying)
                yield return new WaitForEndOfFrame();
            try {
                buildFoundation();
            }
            catch (System.Exception e) {
                Debug.LogError("Error during buildFoundation(); " + e.Message);
                yield break;
            }
            if (Application.isPlaying)
                yield return new WaitForEndOfFrame();
            try {
                buildStairs();
            }
            catch (System.Exception e) {
                Debug.LogError("Error during buildStairs(); " + e.Message);
                yield break;
            }
            if (Application.isPlaying)
                yield return new WaitForEndOfFrame();

            foreach (var floor in WallMesh.WallMeshes.Values)
            {
                foreach (var holder in floor.Values)
                {
                    foreach (ProceduralObject po in holder)
                    {
                        if (po != null)
                        {
                            if (Application.isPlaying)
                                yield return new WaitForEndOfFrame();
                            try
                            {
                                po.createMesh();
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError("Error during createMesh(); " + e.Message);
                                yield break;
                            }
                        }
                    }
                }
            }
            if (Application.isPlaying)
                yield return new WaitForEndOfFrame();
            try
            {
                generateColliders();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during generateColliders(); " + e.Message);
                yield break;
            }
            duplicateFloors();
            if (mergeWallObjects)
                objectHolderManager.combineAll(seperateByFloor);
            if (!seperateByFloor)
                globalWallMeshHolder.AddComponent<MeshCombiner>().combine(true, hitBoxManager.Get(0));
            globalWallMeshHolder.transform.Translate(Vector3.up * elevation);
            objectHolder.transform.Translate(Vector3.up * elevation);
            roofHolder.transform.Translate(Vector3.up * elevation);
            hitboxHolder.transform.Translate(Vector3.up * elevation);
            Debug.Log("House generated in " + (Time.realtimeSinceStartup - t1));
            resetOldPos();
            yield return 0;
        }

        void resetOldPos()
        {
            transform.SetParent(saveParent);
            transform.localPosition = SavePos;
            transform.localRotation = saveRot;
        }

        void createTextureBoxes(out BoxTexture hBox, out BoxTexture vBox)
        {
            hBox = new BoxTexture();
            hBox.setTextureBySide(Directions.NORTH, currentIndoorTex);
            hBox.setTextureBySide(Directions.SOUTH, currentOutdoorTex);
            hBox.setTextureBySide(Directions.EAST, currentEdgeTex);
            hBox.setTextureBySide(Directions.WEST, currentEdgeTex);
            hBox.setTextureBySide(Directions.UP, currentEdgeTex);
            hBox.setTextureBySide(Directions.CENTER, currentEdgeTex);
            vBox = new BoxTexture();
            vBox.setTextureBySide(Directions.NORTH, currentEdgeTex);
            vBox.setTextureBySide(Directions.SOUTH, currentEdgeTex);
            vBox.setTextureBySide(Directions.EAST, currentIndoorTex);
            vBox.setTextureBySide(Directions.WEST, currentOutdoorTex);
            vBox.setTextureBySide(Directions.UP, currentEdgeTex);
            hBox.setTextureBySide(Directions.CENTER, currentEdgeTex);

        }

        WallPart queueWall(WallPart wall, ref Room room)
        {
            if (wall.position.x < 0 || wall.position.y < 0 || wall.position.z < 0)
                throw new System.Exception("the generation can not occur at X and Z position under zero (A wall can not be placed at " + wall.position.ToString() + ")");
            if (!WallList.ContainsKey(wall.position))
            {
                room.walls.Add(wall.position, wall);
                WallList.Add(wall.position, wall);
                bottomLeft = Position.getMinValues(ref wall.position, ref bottomLeft);
                topRight = Position.getMaxValues(ref wall.position, ref topRight);
            }
            else
            {
                if (!room.walls.ContainsKey(wall.position))
                    room.walls.Add(wall.position, WallList[wall.position]);
                return WallList[wall.position];
            }
            return wall;
        }

        WallPart queueWall(WallPart wall)
        {
            if (wall.position.x < 0 || wall.position.y < 0 || wall.position.z < 0)
                throw new System.Exception("the generation can not occur at position under zero (A wall can not be placed at " + wall.position.ToString() + ")");
            if (!WallList.ContainsKey(wall.position))
            {
                WallList.Add(wall.position, wall);
                bottomLeft = Position.getMinValues(ref wall.position, ref bottomLeft);
                topRight = Position.getMaxValues(ref wall.position, ref topRight);
            }
            else
                return WallList[wall.position];
            return wall;
        }

        void unqueueWall(WallPart wall)
        {
            if (wall.position.x < 0 || wall.position.y < 0 || wall.position.z < 0)
                throw new System.Exception("the generation can not occur at position under zero (A wall can not be placed at " + wall.position.ToString() + ")");
            if (!WallList.ContainsKey(wall.position))
                Debug.LogWarning("Position " + wall.position.ToString() + " does not exists");
            else
                WallList.Remove(wall.position);
            return;
        }


        /// <summary>
        /// returns wether or not the tile is inside any room
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public bool isInside(Position tile, int floorOffset = 1)
        {
            foreach (Room r in rooms)
            {
                if (r.floor + floorOffset != tile.y)
                    continue;
                if (r.Contains(tile, true))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// checks wether or not the 'tile' parameter indoors
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="floorOffset"></param>
        /// <returns></returns>
        public bool isInside(Vector3 tile, int floorOffset = 1)
        {
            tile = ToolBox.divideVectors(tile, RealDimensionsVector);
            Position pTile = new Position((int)tile.x, (int)tile.y, (int)tile.z);

            foreach (Room r in rooms)
            {
                if (r.floor + floorOffset != pTile.y)
                    continue;
                if (r.Contains(pTile, true))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// checks wether or not the 'tile' parameter is inside 'room'
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="room"></param>
        /// <param name="floorOffset"></param>
        /// <returns></returns>
        public bool isInsideRoom(Position tile, Room room, int floorOffset = 1)
        {
            if (room.floor + floorOffset != tile.y)
                return false;
            if (room.Contains(tile, true))
                return true;
            return false;
        }

        public int findFloor(float height)
        {
            return (int)((height + 0.02f) / VerticalScale);
        }
}

}
