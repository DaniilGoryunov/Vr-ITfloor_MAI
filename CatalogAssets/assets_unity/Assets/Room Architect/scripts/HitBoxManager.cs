using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomArchitectEngine
{
    public class HitBoxManager
    {
        public Dictionary<int, GameObject> floors;
        RoomArchitect house;

        public HitBoxManager(RoomArchitect house)
        {
            this.house = house;
            floors = new Dictionary<int, GameObject>();
        }

        public GameObject Get(int floor)
        {
            if (floors.ContainsKey(floor) == false)
            {
                GameObject newHolder = new GameObject("floor_" + floor);
                newHolder.transform.SetParent(house.hitboxHolder.transform, false);
                newHolder.transform.localPosition = new Vector3(0, 0, 0);
                newHolder.transform.rotation = Quaternion.identity;
                floors.Add(floor, newHolder);
            }
            return floors[floor];
        }
    }
}

