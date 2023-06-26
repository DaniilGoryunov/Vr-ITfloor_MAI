using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomArchitectEngine
{
    /// <summary>
    /// Okay, so what's happening here:
    /// 
    /// GlobalWallMesh is in charge of handling, giving and creating the procedural meshes (PO in short) holding the walls of the model
    /// The container is a dictionnary of floors, which is a dictionnary of procedural meshes classified by string unique IDs
    /// When a room is created the walls are assigned an UID to make a cut possible between each wall, so that the user can disable
    /// individual walls at will. The UID is given to the Get(), BUT
    /// 
    /// A procedural mesh can onverflow, it's limited in number of triangles it can contain. So if a PO with a specific UID is full,
    /// We must create a new PO. For that we add an index at the end of the provided UID, as a counter, called subID.
    /// 
    /// It's complicated, I can't think of a better solution, and I have to pay my bills so, onward to coding.
    /// </summary>
    public class GlobalWallMesh
    {
        public Dictionary<int, Dictionary<string, List<ProceduralObject>>> WallMeshes;
        public Material[] materials;
        public RoomArchitect house;
        string UID = "global";

        public GlobalWallMesh(RoomArchitect parent, Material[] materials)
        {
            this.house = parent;
            this.materials = materials;
            WallMeshes = new Dictionary<int, Dictionary<string, List<ProceduralObject>>>();
        }

        public void duplicateFloor(int from, int to)
        {
            if (WallMeshes.ContainsKey(to) == false)
                WallMeshes.Add(to, new Dictionary<string, List<ProceduralObject>>());
            else
                WallMeshes[to] = new Dictionary<string, List<ProceduralObject>>();
            foreach(var poList in WallMeshes[from])
            {
                WallMeshes[to].Add(poList.Key, new List<ProceduralObject>());
                for (int i = 0; i < poList.Value.Count; i++)
                {
                    ProceduralObject po = (new GameObject()).AddComponent<ProceduralObject>();
                    po.init(house, materials);
                    po.deepCopy(poList.Value[i]);
                    WallMeshes[to][poList.Key].Add(poList.Value[i]);
                }
            }
        }

        public void setUID(string UID)
        {
            this.UID = UID;
        }

        public void resetUID()
        {
            UID = "global";
        }

        public ProceduralObject Get(int floor = 0)
        {
            if (WallMeshes.ContainsKey(floor) == false)
                WallMeshes.Add(floor, new Dictionary<string, List<ProceduralObject>>());
            return getLatestPo(floor, (house.seperateEachWall == true ? UID : "global"));
        }

        public ProceduralObject getLatestPo(int floor, string UID)
        {
            if (WallMeshes[floor].ContainsKey(UID) == false)
                WallMeshes[floor].Add(UID, new List<ProceduralObject>());
            int lastMeshID = WallMeshes[floor][UID].Count - 1;
            if (WallMeshes[floor][UID].Count <= 0 || WallMeshes[floor][UID][lastMeshID].IsFull)
                return addNewSubmesh(floor, UID);
            return WallMeshes[floor][UID][lastMeshID];
        }

        ProceduralObject addNewSubmesh(int floor, string UID)
        {
            ProceduralObject newPO = (new GameObject()).AddComponent<ProceduralObject>();
            WallMeshes[floor][UID].Add(newPO);
            newPO.init(house, materials);
            newPO.optimizeWallMode = true;
            newPO.setTextureMode(TEXTUREMODE.BOX, house.RealDimensionsVector);
            newPO.transform.SetParent(getHolder(floor).transform, false);
            newPO.transform.localPosition = Vector3.zero;
            newPO.transform.localRotation = Quaternion.identity;
            newPO.gameObject.name = "subMesh_" + UID + "_" + (WallMeshes[floor][UID].Count - 1).ToString();
            return newPO;
        }

        public GameObject getHolder(int floor = 0)
        {
            GameObject holder;
            if (house.wallMeshHolder.ContainsKey(floor) == false)
            {
                holder = new GameObject("Floor_" + floor);
                holder.transform.SetParent(house.globalWallMeshHolder.transform);
                holder.transform.localPosition = Vector3.zero;
                house.wallMeshHolder.Add(floor, holder);
            }
            else
                holder = house.wallMeshHolder[floor];
            return holder;
        }
    }
}