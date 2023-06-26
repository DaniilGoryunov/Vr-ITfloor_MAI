using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomArchitectEngine
{
    public enum TEXTUREMODE
    {
        DEFAULT,
        BOX,
        ROOF,
        CUSTOM
    }

    public enum MESHPANELIST
    {
        NORTH,
        SOUTH,
        EAST,
        WEST,
        TOPPER,
        NEITHER
    }

    /// <summary>
    /// Class used to hold basic procedural mesh generation methods
    /// Do not use this class 
    /// </summary>
    [ExecuteInEditMode]
    public class ProceduralObject : MonoBehaviour
    {
        MeshFilter filter;
        Renderer myRenderer;
        List<Vector3> vertices;
        List<int> triangles;
        List<List<int>> subTri;
        List<Vector3> normals;
        List<Vector2> uv;
        List<int> tmpTris = new List<int>();
        TEXTUREMODE textureMode = TEXTUREMODE.DEFAULT;
        public bool pivotTexture90 = false;
        public bool pivotTextureI90 = false;
        public bool optimizeWallMode = false;
        [HideInInspector] public MESHPANELIST meshPanelListSelector = MESHPANELIST.NEITHER;
        public Dictionary<int, List<MeshPane>> MeshPaneList;
        public List<MeshPane> topperMeshPaneList;
        public Vector3 textureStretchBox = Vector3.one;
        public Vector2 textureOffset = Vector2.zero;
        public Material[] tex;
        public RoomArchitect house;

        public void deepCopy(ProceduralObject other)
        {
            vertices.AddRange(other.vertices);
            triangles.AddRange(other.triangles);
            subTri.AddRange(other.subTri);
            normals.AddRange(other.normals);
            uv.AddRange(other.uv);
            tmpTris.AddRange(other.tmpTris);
            pivotTexture90 = other.pivotTexture90;
            pivotTextureI90 = other.pivotTextureI90;
            optimizeWallMode = other.optimizeWallMode;
            topperMeshPaneList.AddRange(other.topperMeshPaneList);
            textureStretchBox = other.textureStretchBox;
            textureOffset = other.textureOffset;
            tex = new Material[other.tex.Length];
            for (int i = 0; i < tex.Length; i++)
                tex[i] = other.tex[i];
            house = other.house;
        }

        public bool IsFull
        {
            get
            {
                if (vertices.Count >= 60000)
                    return true;
                return false;
            }
        }

        public bool[] disabledPanes = new bool[Directions.length];

        public void disableThoseOrientations(params Directions[] disabled)
        {
            foreach (Directions d in disabled)
                disabledPanes[d] = true;
        }

        public void resetDisabledPositions()
        {
            for (int i = 0; i < Directions.length; i++)
            {
                disabledPanes[i] = false;
            }
        }

        void Awake()
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            subTri = new List<List<int>>();
            normals = new List<Vector3>();
            uv = new List<Vector2>();
            MeshPaneList = new Dictionary<int, List<MeshPane>>();
            MeshPaneList.Add((int)MESHPANELIST.NORTH, new List<MeshPane>());
            MeshPaneList.Add((int)MESHPANELIST.SOUTH, new List<MeshPane>());
            MeshPaneList.Add((int)MESHPANELIST.EAST, new List<MeshPane>());
            MeshPaneList.Add((int)MESHPANELIST.WEST, new List<MeshPane>());
            topperMeshPaneList = new List<MeshPane>();
            if (gameObject.GetComponent<MeshFilter>() == null)
                filter = gameObject.AddComponent<MeshFilter>();
            if (gameObject.GetComponent<MeshRenderer>() == null)
                myRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        public void init(RoomArchitect house, Material[] tex)
        {
            this.house = house;
            try
            {
                GameObject.DestroyImmediate(gameObject.GetComponent<MeshFilter>());
            }
            catch
            {
            }
            filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = new Mesh();
            vertices = new List<Vector3>();
            subTri = new List<List<int>>();
            this.tex = tex;
            for (int i = 0; i < tex.Length; i++)
                subTri.Add(new List<int>());
            normals.Clear();
            uv.Clear();
            for (int i = 0; i < tex.Length; i++)
                addPane(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, "", false, i);
        }


        /// <summary>
        /// Adds a triangle to the mesh. The order of the points must be arranged clockwise
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        public void addTri(Vector3 first, Vector3 second, Vector3 third, bool invert = false, int textureID = 0)
        {

            /*Vector3 v1 = new Vector3(second.x - first.x, second.y - first.y, second.z - first.z);
            Vector3 v2 = new Vector3(third.x - first.x, third.y - first.y, third.z - first.z);*/
            //norm = Vector3.Cross(v2, v1);
            int lastCount = vertices.Count;
            vertices.Add(first);
            vertices.Add(second);
            vertices.Add(third);

            if (!invert)
                tmpTris = new List<int>() { lastCount + 0, lastCount + 1, lastCount + 2 };
            else
                tmpTris = new List<int>() { lastCount + 2, lastCount + 1, lastCount + 0 };
            triangles.AddRange(tmpTris);
            subTri[textureID].AddRange(tmpTris);
            Vector3 norm = Vector3.Cross(second - first, third - first).normalized;
            if (invert)
                norm = Vector3.Cross(second - third, first - third).normalized;
            normals.Add(norm);
            normals.Add(norm);
            normals.Add(norm);

            if (textureMode == TEXTUREMODE.DEFAULT)
            {
                if (norm == -Vector3.forward || norm == Vector3.forward)
                {
                    uv.Add(new Vector2(first.x, first.y));
                    uv.Add(new Vector2(first.x, second.y));
                    uv.Add(new Vector2(third.x, second.y));
                }
                else if (norm == Vector3.left || norm == Vector3.right)
                {
                    uv.Add(new Vector2(first.z, first.y));
                    uv.Add(new Vector2(first.z, second.y));
                    uv.Add(new Vector2(third.z, second.y));
                }
                else
                {
                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(1, 1));
                }
            }
            else if (textureMode == TEXTUREMODE.BOX || (textureMode == TEXTUREMODE.ROOF && normalIsStraight(norm)))
            {
                if (textureMode == TEXTUREMODE.ROOF)
                {
                    Vector3 add = new Vector3(house.wallThickness * 0.5f, 0, 0);
                    first -= add;
                    second -= add;
                    third -= add;
                }
                boxUVMapping(first, second, third, Vector3.zero, true);
            }
            else
            {
                uv.Add(new Vector2(0, 0));
                uv.Add(new Vector2(1, 0));
                uv.Add(new Vector2(1, 1));
            }
        }

        public void addTriDoubleSide(Vector3 first, Vector3 second, Vector3 third, int textureID)
        {
            addTri(first, second, third, true, textureID);
            addTri(first, second, third, false, textureID);
        }

        /// <summary>
        /// Adds a rectangle to the mesh. The order of the points must be arranged clockwise
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <param name="forth"></param>
        /// <param name="invert"></param>
        public void addPane(Vector3 first, Vector3 second, Vector3 third, Vector3 forth, string separationID, bool invert = false, int textureID = 0)
        {
            if (optimizeWallMode == true && house.optimizeMesh && meshPanelListSelector != MESHPANELIST.TOPPER && meshPanelListSelector != MESHPANELIST.NEITHER)
            {
                optimizeMeshPane(new MeshPane(first, second, third, forth, invert, textureID, separationID));
                return;
            }
            if (optimizeWallMode == true && house.optimizeMesh && meshPanelListSelector == MESHPANELIST.TOPPER)
            {
                topperMeshPaneList.Add(new MeshPane(first, second, third, forth, invert, textureID, separationID));
                return;
            }
            List<Vector3> tmpPoints = new List<Vector3>() { first, second, third, forth };
            if (pivotTexture90)
                tmpPoints = new List<Vector3>() { second, third, forth, first };
            else if (pivotTextureI90)
                tmpPoints = new List<Vector3>() { forth, first, second, third };

            int lastCount = vertices.Count;
            vertices.Add(tmpPoints[0]);
            vertices.Add(tmpPoints[1]);
            vertices.Add(tmpPoints[2]);
            vertices.Add(tmpPoints[3]);

            if (!invert)
                tmpTris = new List<int>() { lastCount + 0, lastCount + 1, lastCount + 2,
                                        lastCount + 0, lastCount + 2, lastCount + 3};
            else
                tmpTris = new List<int>() { lastCount + 3, lastCount + 2, lastCount + 1,
                                        lastCount + 3, lastCount + 1, lastCount + 0};

            triangles.AddRange(tmpTris);
            subTri[textureID].AddRange(tmpTris);
            Vector3 norm = Vector3.Cross(tmpPoints[1] - tmpPoints[0], tmpPoints[2] - tmpPoints[0]).normalized;
            if (invert)
                norm = Vector3.Cross(tmpPoints[2] - tmpPoints[3], tmpPoints[1] - tmpPoints[3]).normalized;
            normals.Add(norm);
            normals.Add(norm);
            normals.Add(norm);
            normals.Add(norm);

            if (textureMode == TEXTUREMODE.DEFAULT)
            {
                if (norm == -Vector3.forward || norm == Vector3.forward)
                {
                    uv.Add(new Vector2(tmpPoints[0].x, tmpPoints[0].y));
                    uv.Add(new Vector2(tmpPoints[0].x, tmpPoints[1].y));
                    uv.Add(new Vector2(tmpPoints[2].x, tmpPoints[1].y));
                    uv.Add(new Vector2(tmpPoints[2].x, tmpPoints[0].y));
                }
                else if (norm == Vector3.left || norm == Vector3.right)
                {
                    uv.Add(new Vector2(tmpPoints[0].z, tmpPoints[0].y));
                    uv.Add(new Vector2(tmpPoints[0].z, tmpPoints[1].y));
                    uv.Add(new Vector2(tmpPoints[2].z, tmpPoints[1].y));
                    uv.Add(new Vector2(tmpPoints[2].z, tmpPoints[0].y));
                }
                else
                {
                    uv.Add(new Vector2(0, 0));
                    uv.Add(new Vector2(1, 0));
                    uv.Add(new Vector2(1, 1));
                    uv.Add(new Vector2(0, 1));
                }
            }
            else if (textureMode == TEXTUREMODE.BOX || (textureMode == TEXTUREMODE.ROOF))
            {
                boxUVMapping(tmpPoints[0], tmpPoints[1], tmpPoints[2], tmpPoints[3]);
            }
            else
            {
                uv.Add(new Vector2(0, 0));
                uv.Add(new Vector2(1, 0));
                uv.Add(new Vector2(1, 1));
                uv.Add(new Vector2(0, 1));
            }
        }

        List<MeshPane> TMPList;
        MeshPane TMPIntegrator, TMPIntegrated;
        public void optimizeMeshPane(MeshPane pane)
        {
            TMPList = MeshPaneList[(int)meshPanelListSelector];
            TMPIntegrator = null;
            TMPIntegrated = null;
            for (int i = 0; i < TMPList.Count; i++)
            {
                if (pane.compare(TMPList[i]))
                    TMPIntegrated = TMPList[i];
                if (TMPList[i].compare(pane))
                    TMPIntegrator = TMPList[i];
                if (TMPIntegrator != null && TMPIntegrated != null)
                    break;
            }
            if (TMPIntegrator == null && TMPIntegrated == null)
                TMPList.Insert(0, pane);
            else if (TMPIntegrator != null && TMPIntegrated == null)
                TMPIntegrator.integrate(pane);
            else if (TMPIntegrator == null && TMPIntegrated != null)
            {
                TMPList.Insert(0, pane.integrate(TMPIntegrated));
                TMPList.Remove(TMPIntegrated);
            }
            else if (TMPIntegrator != null && TMPIntegrated != null)
            {
                pane.integrate(TMPIntegrated);
                TMPList.Remove(TMPIntegrated);
                TMPIntegrator.integrate(pane);
            }
        }

        void boxUVMapping(Vector3 first, Vector3 second, Vector3 third, Vector3 forth, bool isTri = false)
        {
            Vector3 correctedUVFirst = ToolBox.divideVectors(first, house.RealDimensionsVector);
            Vector3 correctedUVSecond = ToolBox.divideVectors(second, house.RealDimensionsVector);
            Vector3 correctedUVThird = ToolBox.divideVectors(third, house.RealDimensionsVector);
            Vector3 correctedUVForth = ToolBox.divideVectors(forth, house.RealDimensionsVector);
            correctedUVFirst = ToolBox.divideVectors(first, textureStretchBox);
            correctedUVSecond = ToolBox.divideVectors(second, textureStretchBox);
            correctedUVThird = ToolBox.divideVectors(third, textureStretchBox);
            correctedUVForth = ToolBox.divideVectors(forth, textureStretchBox);
            Vector2 bottomLeft = Vector2.zero;
            Vector2 topLeft = Vector2.zero;
            Vector2 topRight = Vector2.zero;
            Vector2 bottomRight = Vector2.zero;
            if (first.z == third.z)
            {
                bottomLeft = new Vector2(correctedUVFirst.x, correctedUVFirst.y);
                topLeft = new Vector2(correctedUVSecond.x, correctedUVSecond.y);
                topRight = new Vector2(correctedUVThird.x, correctedUVThird.y);
                bottomRight = new Vector2(correctedUVForth.x, correctedUVForth.y);
            }
            else if (first.x == third.x)
            {
                bottomLeft = new Vector2(correctedUVFirst.z, correctedUVFirst.y);
                topLeft = new Vector2(correctedUVSecond.z, correctedUVSecond.y);
                topRight = new Vector2(correctedUVThird.z, correctedUVThird.y);
                bottomRight = new Vector2(correctedUVForth.z, correctedUVForth.y);
            }
            else
            {
                bottomLeft = new Vector2(correctedUVFirst.x, correctedUVFirst.z);
                topLeft = new Vector2(correctedUVSecond.x, correctedUVSecond.z);
                topRight = new Vector2(correctedUVThird.x, correctedUVThird.z);
                bottomRight = new Vector2(correctedUVForth.x, correctedUVForth.z);
            }
            if (pivotTexture90)
            {
                correctedUVFirst = ToolBox.divideVectors(first, house.RealDimensionsVector);
                bottomLeft = new Vector3(bottomLeft.y, bottomLeft.x);
                topLeft = new Vector3(topLeft.y, topLeft.x);
                topRight = new Vector3(topRight.y, topRight.x);
                bottomRight = new Vector3(bottomRight.y, bottomRight.x);
                if (pivotTextureI90)
                {
                    ToolBox.swapV2(ref bottomLeft, ref topLeft);
                    ToolBox.swapV2(ref bottomRight, ref topRight);
                }
            }
            while (textureOffset.y < 0)
                textureOffset.y += 10;
            bottomLeft += textureOffset;
            topLeft += textureOffset;
            topRight += textureOffset;
            bottomRight += textureOffset;
            uv.Add(bottomLeft);
            uv.Add(topLeft);
            uv.Add(topRight);
            if (!isTri)
                uv.Add(bottomRight);
        }

        bool normalIsStraight(Vector3 norm)
        {
            if (norm == Vector3.up || norm == Vector3.down ||
                norm == Vector3.left || norm == Vector3.right ||
                norm == Vector3.forward || norm == Vector3.back)
                return true;
            return false;
        }

        public void setTextureMode(TEXTUREMODE mode)
        {
            setTextureMode(mode, Vector3.one, Vector2.zero);
        }

        public void setTextureMode(TEXTUREMODE mode, Vector3 stretchBox)
        {
            setTextureMode(mode, stretchBox, Vector2.zero);
        }

        //public void orderPanePointForBoxUV(Vector3 first, Vector3 second, Vector3 third, Vector3 forth,
        //                                   out Vector3 adjustedFirst, out Vector3 adjustedSecond, out Vector3 adjustedThird, out Vector3 adjustedForth)
        //{
        //    adjustedFirst = (first.magnitude)
        //}

        public void setTextureMode(TEXTUREMODE mode, Vector3 stretchBox, Vector2 textureOffset)
        {
            textureStretchBox = stretchBox;
            textureMode = mode;
            this.textureOffset = textureOffset;
        }

        public void addPaneDoubleSide(Vector3 first, Vector3 second, Vector3 third, Vector3 forth, int textureID = 0)
        {
            addPane(first, second, third, forth, "", false, textureID);
            addPane(first, second, third, forth, "", true, textureID);
        }

        public void addThickPane(Vector3 first, Vector3 second, Vector3 third, Vector3 forth, float thickness, int textureID = 0)
        {
            float halfThick = thickness * 0.5f;
            addPane(first, second, third, forth, "", true, textureID);
            first.y += thickness;
            second.y += thickness;
            third.y += thickness;
            forth.y += thickness;
            addPane(first, second, third, forth, "", false, textureID);
            first.y -= halfThick;
            second.y -= halfThick;
            third.y -= halfThick;
            forth.y -= halfThick;
            addPane(new Vector3(first.x, first.y - halfThick, first.z),
                    new Vector3(first.x, first.y + halfThick, first.z),
                    new Vector3(forth.x, third.y + halfThick, forth.z),
                    new Vector3(forth.x, third.y - halfThick, forth.z), "", false, textureID);
            addPane(new Vector3(third.x, third.y - halfThick, third.z),
                    new Vector3(third.x, third.y + halfThick, third.z),
                    new Vector3(second.x, second.y + halfThick, second.z),
                    new Vector3(second.x, second.y - halfThick, second.z), "", false, textureID);
            addPane(new Vector3(first.x, first.y - halfThick, first.z),
                    new Vector3(second.x, second.y - halfThick, second.z),
                    new Vector3(second.x, second.y + halfThick, second.z),
                    new Vector3(first.x, first.y + halfThick, first.z), "", false, textureID);
            addPane(new Vector3(forth.x, forth.y - halfThick, forth.z),
                     new Vector3(forth.x, forth.y + halfThick, forth.z),
                     new Vector3(forth.x, forth.y + halfThick, third.z),
                     new Vector3(forth.x, forth.y - halfThick, third.z), "", false, textureID);
        }

        public void addSlab(Vector3 bottomPosition, Vector3 topPosition, float thickness, int textureID, int slabTextureID, bool fullBottom)
        {
            Vector3 first = new Vector3(bottomPosition.x, bottomPosition.y, topPosition.z);
            Vector3 second = new Vector3(topPosition.x, topPosition.y, topPosition.z);
            Vector3 third = new Vector3(topPosition.x, topPosition.y, bottomPosition.z);
            Vector3 forth = new Vector3(bottomPosition.x, bottomPosition.y, bottomPosition.z);
            addSlab(first, second, third, forth, 0.8f, textureID);
            bottomPosition += new Vector3(thickness, 0, 0);
            if (!fullBottom)
                topPosition += new Vector3(thickness, 0, 0);
            Vector3 botFirst = new Vector3(bottomPosition.x, bottomPosition.y, topPosition.z);
            Vector3 botSecond = new Vector3(topPosition.x, topPosition.y, topPosition.z);
            if (fullBottom)
                botSecond = new Vector3(topPosition.x, bottomPosition.y, topPosition.z);
            Vector3 botThird = new Vector3(topPosition.x, topPosition.y, bottomPosition.z);
            if (fullBottom)
                botThird = new Vector3(topPosition.x, bottomPosition.y, bottomPosition.z);
            Vector3 botForth = new Vector3(bottomPosition.x, bottomPosition.y, bottomPosition.z);
            addSlab(botFirst, botSecond, botThird, botForth, 0.8f, textureID, true);
            addPane(botForth, forth, third, botThird, "", false, slabTextureID);
            addPane(first, second, botSecond, botFirst, "", true, slabTextureID);
            addPane(second, botSecond, botThird, third, "", false, slabTextureID);
        }

        public void addSingleFence(Vector3 Position, float length, Directions direction, int inBetweenBarAmount, int textureID = 0)
        {
            if (direction == Directions.WEST)
            {
                Position.x -= length;
                direction = Directions.EAST;
            }
            if (direction == Directions.SOUTH)
            {
                Position.z -= length;
                direction = Directions.NORTH;
            }
            BoxTexture bt = new BoxTexture(rest: textureID);
            float insideWidth = 0.5f;
            float pillarWidth = 0.05f;
            Vector3 pillarSize = new Vector3(pillarWidth, house.WindowHeightFromFloor, pillarWidth);
            Vector3 dir = new Vector3(1, 0, 0);
            if (direction.isEither(Directions.NORTH, Directions.SOUTH))
                dir = new Vector3(0, 0, 1);
            addBox(Position, pillarSize, bt, "");
            addBox(Position + new Vector3(length * dir.x, 0, length * dir.z), pillarSize, bt, "");
            addBox(Position + new Vector3(-pillarWidth * dir.x * 0.5f, house.WindowHeightFromFloor, -pillarWidth * dir.z * 0.5f),
                new Vector3(length * dir.x + pillarWidth, pillarWidth, length * dir.z + pillarWidth), bt, direction.opposite(), "", true);
            addBox(Position + new Vector3(-pillarWidth * dir.x * 0.5f, house.WindowHeightFromFloor * 0.25f, -pillarWidth * dir.z * 0.5f),
                new Vector3(length * dir.x + pillarWidth * insideWidth, pillarWidth * insideWidth, length * dir.z + pillarWidth * insideWidth), bt, direction.opposite(), "", true);
            Vector3 incr = new Vector3(Position.x, Position.y, Position.z);
            float stepSize = length / inBetweenBarAmount;
            incr += new Vector3(stepSize * dir.x, 0, stepSize * dir.z);
            while (incr.z < Position.z + length && incr.x < Position.x + length)
            {
                addBox(incr, new Vector3(pillarWidth * insideWidth, house.WindowHeightFromFloor, pillarWidth * insideWidth), bt, "");
                incr += new Vector3(stepSize * dir.x, 0, stepSize * dir.z);
            }
        }

        public void addRail(Vector3 bottomPosition, Vector3 topPosition, int textureID, float thickness)
        {
            topPosition.z = bottomPosition.z + thickness;
            Vector3 first = new Vector3(bottomPosition.x, bottomPosition.y, bottomPosition.z);
            Vector3 second = new Vector3(bottomPosition.x, bottomPosition.y, topPosition.z);
            Vector3 third = new Vector3(topPosition.x, topPosition.y, topPosition.z);
            Vector3 forth = new Vector3(topPosition.x, topPosition.y, bottomPosition.z);
            addThickPane(first, second, third, forth, 1f, textureID);
        }

        void addSlab(Vector3 first, Vector3 second, Vector3 third, Vector3 forth, float thickness, int textureID, bool invert = false)
        {
            addPane(new Vector3(first.x, first.y, first.z),
                    new Vector3(second.x, second.y, second.z),
                    new Vector3(third.x, third.y, third.z),
                    new Vector3(forth.x, forth.y, forth.z), "", invert, textureID);
        }

        /// <summary>
        /// adds a box to the mesh
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void addBox(Vector3 pos, Vector3 size, BoxTexture tex, string sperationID, bool isTop = false)
        {
            addBox(pos, size, tex, Directions.NONE, sperationID, isTop);
        }


        /// <summary>
        /// Adds a box to the mes
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="referenceCorner"></param>
        /// <param name="isTop"></param>
        public void addBox(Vector3 pos, Vector3 size, BoxTexture tex, Directions referenceCorner, string separationID, bool isTop = false)
        {
            if (referenceCorner == Directions.EAST)
                pos.x -= 0.5f * size.x;
            else if (referenceCorner == Directions.WEST)
                pos.x += 0.5f * size.x;
            else if (referenceCorner == Directions.NORTH)
                pos.z -= 0.5f * size.z;
            else if (referenceCorner == Directions.SOUTH)
                pos.z += 0.5f * size.z;
            float sxBy2 = size.x / 2f;
            float szBy2 = size.z / 2f;
            // SOUTH
            meshPanelListSelector = MESHPANELIST.SOUTH;
            if (!disabledPanes[Directions.SOUTH])
                addPane(ToolBox.newV(pos.x - sxBy2, pos.y + 0, pos.z - szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + size.y, pos.z - szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + size.y, pos.z - szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + 0, pos.z - szBy2), separationID, textureID: tex.textureID[Directions.SOUTH]);
            // NORTH
            meshPanelListSelector = MESHPANELIST.NORTH;
            if (!disabledPanes[Directions.NORTH])
                addPane(ToolBox.newV(pos.x + sxBy2, pos.y + 0, pos.z + szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + size.y, pos.z + szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + size.y, pos.z + szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + 0, pos.z + szBy2), separationID, textureID: tex.textureID[Directions.NORTH]);
            // WEST
            meshPanelListSelector = MESHPANELIST.WEST;
            if (!disabledPanes[Directions.WEST])
                addPane(ToolBox.newV(pos.x - sxBy2, pos.y + 0, pos.z + szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + size.y, pos.z + szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + size.y, pos.z - szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + 0, pos.z - szBy2), separationID, textureID: tex.textureID[Directions.WEST]);
            // EAST
            meshPanelListSelector = MESHPANELIST.EAST;
            if (!disabledPanes[Directions.EAST])
                addPane(ToolBox.newV(pos.x + sxBy2, pos.y + 0, pos.z - szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + size.y, pos.z - szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + size.y, pos.z + szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + 0, pos.z + szBy2), separationID, textureID: tex.textureID[Directions.EAST]);
            meshPanelListSelector = MESHPANELIST.TOPPER;
            // TOP
            if (!disabledPanes[Directions.UP])
                addPane(ToolBox.newV(pos.x - sxBy2, pos.y + size.y, pos.z - szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + size.y, pos.z + szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + size.y, pos.z + szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + size.y, pos.z - szBy2), separationID, textureID: tex.textureID[Directions.UP]);
            // BOTTOM
            meshPanelListSelector = MESHPANELIST.NEITHER;
            if (isTop)
            {
                addPane(ToolBox.newV(pos.x - sxBy2, pos.y + 0, pos.z - szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + 0, pos.z - szBy2),
                        ToolBox.newV(pos.x + sxBy2, pos.y + 0, pos.z + szBy2),
                        ToolBox.newV(pos.x - sxBy2, pos.y + 0, pos.z + szBy2), separationID, textureID: tex.textureID[Directions.CENTER]);
            }
        }

        /// <summary>
        /// Method called after the draw queuing methods have all been called
        /// </summary>
        public void createMesh()
        {
            if (filter.sharedMesh == null)
                return;
            filter.sharedMesh.vertices = vertices.ToArray();
            filter.sharedMesh.normals = normals.ToArray();
            filter.sharedMesh.uv = uv.ToArray();
            filter.sharedMesh.subMeshCount = subTri.Count;
            for (int i = 0; i < subTri.Count; i++)
                filter.sharedMesh.SetTriangles(subTri[i], i);
            myRenderer.material = tex[0];
            myRenderer.materials = tex;
            filter.sharedMesh.RecalculateBounds();
            filter.sharedMesh.RecalculateNormals();
            var o_250_8_636329647675811721 = filter.sharedMesh;
        }

    }
}
