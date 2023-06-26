using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoomArchitectEngine;

namespace RoomArchitectEngine
{

    public class RoofStyle : MonoBehaviour
    {
        public ROOFTYPE roofType;
        [Range(0, 4)] public float height = 1;
        [Range(0, 1)] public float flatTopAmount = 0.03f;
    }
}
