using UnityEngine;
using System.Collections;

namespace RoomArchitectEngine
{
    public class ToolBox
    {
        public static string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static Vector3 newV(float x = 0, float y = 0, float z = 0, bool invert = false)
        {
            if (invert)
                return new Vector3(z, y, x);
            return new Vector3(x, y, z);
        }

        public static Vector3 divideVectors(Vector3 v1, Vector3 dividedBbyV2)
        {
            return new Vector3(v1.x / dividedBbyV2.x, v1.y / dividedBbyV2.y, v1.z / dividedBbyV2.z);
        }

        public static void swapV2(ref Vector2 v1, ref Vector2 v2)
        {
            Vector2 tmp = v2;
            v2 = v1;
            v1 = tmp;
        }

        public static string randomString(int len = 5)
        {
            string s = "";
            for (int i = 0; i < len; i++)
                s += charset[Random.Range(0, charset.Length - 1)];
            return s;
        }

    }
}
