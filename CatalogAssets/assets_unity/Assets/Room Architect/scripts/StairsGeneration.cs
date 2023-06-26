using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomArchitectEngine
{
    public partial class RoomArchitect : MonoBehaviour
    {
        List<SingleStair> stairs;
        List<DoubleStair> doubleStairs;

        /// <summary>
        /// Add a single stairway to the building. This method use real Unity units, and has a cell unit equivalent
        /// </summary>
        /// <param name="bottomPosition">Bottom left position of the stair</param>
        /// <param name="topPosition">top right position of the stair</param>
        /// <param name="stepNumber"> number of steps in the stairway</param>
        /// <param name="orientation"> Cardinal orientation. If the steps points north, the lowest point will be south, and the highest point north</param>
        /// <returns></returns>
        public SingleStair addSingleStair(Vector3 bottomPosition, Vector3 topPosition, int stepNumber, Directions orientation)
        {
            if (stairs == null)
                stairs = new List<SingleStair>();
            SingleStair newStair = new SingleStair(bottomPosition, topPosition, stepNumber, StairStyle.Instance.copy());
            if (orientation == Directions.NORTH)
                newStair.isHorizontal = false;
            newStair.initOrientation(orientation);
            stairs.Add(newStair);
            removeTilesOnStairs(newStair);
            return newStair;
        }

        /// <summary>
        /// Add a double stairway to the building. This method use real Unity units, and has a cell unit equivalent
        /// </summary>
        /// <param name="bottomPosition">Bottom left position of the stair</param>
        /// <param name="topPosition">top right position of the stair</param>
        /// <param name="stepNumber"> number of steps in the stairway</param>
        /// <param name="orientation"> Cardinal orientation. If the steps points north, the lowest point will be south, and the highest point north</param>
        /// <returns></returns>
        public DoubleStair addDoubleStair(Vector3 bottomPosition, Vector3 topPosition, int stepNumber, Directions orientation)
        {
            if (doubleStairs == null)
                doubleStairs = new List<DoubleStair>();
            DoubleStair newStair = new DoubleStair(bottomPosition, topPosition, stepNumber, StairStyle.Instance.copy());
            newStair.getHalfStairs(orientation, HorizontalScale, floorThickness);
            //WallMesh.addPane(bottomPosition,
            //                 bottomPosition + new Vector3(0, 0, topPosition.z - bottomPosition.z),
            //                 bottomPosition + new Vector3(topPosition.x - bottomPosition.x, 0, topPosition.z - bottomPosition.z),
            //                 bottomPosition + new Vector3(topPosition.x - bottomPosition.x, 0, 0));
            removeTilesOnStairs(newStair);
            doubleStairs.Add(newStair);
            return newStair;
        }

        /// <summary>
        /// Add a double stairway to the building. This method use cell units, and has a real Unity unit equivalent
        /// </summary>
        /// <param name="bottomPosition">Bottom left position of the stair</param>
        /// <param name="topPosition">top right position of the stair</param>
        /// <param name="stepNumber"> number of steps in the stairway</param>
        /// <param name="orientation"> Cardinal orientation. If the steps points north, the lowest point will be south, and the highest point north</param>
        /// <returns></returns>
        public DoubleStair addDoubleStair(Position bottomPosition, Position topPosition, int stepNumber, Directions orientation)
        {
            return addDoubleStair(bottomPosition.toVector3(RealDimensionsVector), topPosition.toVector3(RealDimensionsVector), stepNumber, orientation);
        }

        /// <summary>
        /// Add a single stairway to the building. This method use cell units, and has a real Unity unit equivalent
        /// </summary>
        /// <param name="bottomPosition">Bottom left position of the stair</param>
        /// <param name="topPosition">top right position of the stair</param>
        /// <param name="stepNumber"> number of steps in the stairway</param>
        /// <param name="orientation"> Cardinal orientation. If the steps points north, the lowest point will be south, and the highest point north</param>
        /// <returns></returns>
        public SingleStair addSingleStair(Position bottomPosition, Position topPosition, int stepNumber, Directions orientation)
        {
            return addSingleStair(bottomPosition.toVector3(RealDimensionsVector), topPosition.toVector3(RealDimensionsVector), stepNumber, orientation);
        }

        void buildStairs()
        {
            if (stairs == null)
                return;
            if (stairs != null)
            {
                foreach (SingleStair s in stairs)
                {
                    generateStair(s);
                }
            }

            if (doubleStairs != null)
            {
                foreach (DoubleStair s in doubleStairs)
                {
                    generateStair(s.st1);
                    generateStair(s.st2);
                }
            }

        }

        void generateStair(SingleStair stair)
        {
            BoxTexture tmpTexture = new BoxTexture();
            for (int j = 0; j < tmpTexture.textureID.Length; j++)
                tmpTexture.textureID[j] = currentStairStepTex;

            ProceduralObject newStair = (new GameObject()).AddComponent<ProceduralObject>();
            newStair.setTextureMode(TEXTUREMODE.BOX, new Vector3(1, 1, 1));
            GameObject stairParent = new GameObject("StairHolder");
            stairParent.transform.SetParent(objectHolderManager.getHolder(findFloor(stair.bottomPosition.y)).transform);
            stairParent.transform.localPosition = stair.bottomPosition + (stair.topPosition - stair.bottomPosition) * 0.5f;
            newStair.name = "stair";
            newStair.transform.SetParent(this.transform, false);
            newStair.transform.localPosition = stair.bottomPosition;
            newStair.transform.SetParent(stairParent.transform, true);
            newStair.init(this, materials);
            Vector3 size = new Vector3(stair.length, stair.height, stair.width);
            newStair.addSlab(Position.Zero.toVector3(RealDimensionsVector), size, stair.steplength, currentStairStepTex, currentRailTex, stair.style.fullBottom);
            if (stair.style.railStyle == RAILSTYLE.FULL)
            {
                newStair.addRail(Position.Zero.toVector3(RealDimensionsVector), size, currentRailTex, 0.08f);
                newStair.addRail(Position.Zero.toVector3(RealDimensionsVector) + new Vector3(0, 0, stair.width - 0.08f), size, currentRailTex, 0.08f);
            }
            if (stair.style.buffer > 0)
            {
                Vector3 bufferSize = new Vector3(stair.size.x, stair.style.buffer, stair.size.z);
                newStair.addBox(new Vector3(bufferSize.x * 0.5f, -bufferSize.y, bufferSize.z * 0.5f), bufferSize, tmpTexture.copy(), "");
            }
            if (stair.topPad.pos != null)
            {
                newStair.addBox(stair.topPad.pos, stair.topPad.size, tmpTexture.copy(), "", true);
                BoxCollider b = newStair.gameObject.AddComponent<BoxCollider>();
                b.center = stair.topPad.pos + new Vector3(0, floorThickness * 0.5f, 0);
                b.size = stair.topPad.size;
            }
            for (int i = 0; i < stair.stepNumber; i++)
            {
                Vector3 stepSize = new Vector3(stair.steplength, stair.stepheight, stair.width - 0.001f);
                Vector3 stepPos = new Vector3(i * stair.steplength + stair.steplength * 0.5f, i * stair.stepheight, stair.width * 0.5f);
                BoxTexture tex = new BoxTexture();
                for (int j = 0; j < tex.textureID.Length; j++)
                    tex.textureID[j] = currentStairStepTex;
                newStair.addBox(stepPos, stepSize, tex, "", (i == 0 ? true : false));
                BoxCollider b = newStair.gameObject.AddComponent<BoxCollider>();
                b.center = stepPos + new Vector3(0, stepSize.y, 0) * 0.5f;
                b.size = stepSize;
            }
            newStair.createMesh();
            if (!stair.isHorizontal)
            {
                newStair.transform.Rotate(Vector3.up, -90);
                newStair.transform.Translate(0, 0, -size.z);
            }
            if (stair.inverted)
            {
                stairParent.transform.Rotate(Vector3.up, 180);
            }
        }

        void removeTilesOnStairs(SingleStair s)
        {
            List<FloorTile> toRemove = new List<FloorTile>();
            foreach (FloorTile t in floorTiles)
            {
                if (tileStairCollision(t, s))
                {
                    toRemove.Add(t);    
                }
            }
            foreach (FloorTile t in toRemove)
                floorTiles.Remove(t);
        }

        Vector3 TMPfPos;
        bool tileStairCollision(FloorTile f, SingleStair s)
        {
            TMPfPos = f.position.toVector3(RealDimensionsVector);
            if (TMPfPos.x >= s.bottomPosition.x  - 0.01f &&
                TMPfPos.z >= s.bottomPosition.z - 0.01f &&
                TMPfPos.x + HorizontalScale <= s.topPosition.x + 0.01f &&
                TMPfPos.z + HorizontalScale <= s.topPosition.z + 0.01f &&
                TMPfPos.y > s.bottomPosition.y + 0.05f &&
                TMPfPos.y <= s.topPosition.y + 0.01f)
                return true;
            return false;
        }
    }
}
