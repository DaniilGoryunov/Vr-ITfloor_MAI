using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomArchitectEngine
{
    public class ObjectHolderManager
    {
        RoomArchitect holderHouse;
        Dictionary<int, GameObject> holders;

        public ObjectHolderManager(RoomArchitect house)
        {
            holderHouse = house;
            holders = new Dictionary<int, GameObject>();
        }

        public GameObject getHolder(int floor)
        {
            if (holders.ContainsKey(floor) == false)
            {
                holders.Add(floor, new GameObject("floor_" + floor.ToString()));
                holders[floor].transform.SetParent(holderHouse.objectHolder.transform, false);
            }
            return holders[floor];
        }

        public void setHolder(int floor, GameObject obj)
        {
            if (holders.ContainsKey(floor) == false)
                holders.Add(floor, obj);
        }

        public void combineAll(bool byFloor)
        {
            if (byFloor)
            {
                int i = 0;
                foreach (GameObject g in holders.Values)
                {
                    g.AddComponent<MeshCombiner>().combine(true, holderHouse.hitBoxManager.Get(i++));
                }
            }
            else
            {
                holderHouse.objectHolder.AddComponent<MeshCombiner>().combine(true, holderHouse.hitBoxManager.Get(0));
            }
        }
    }
}
