using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomArchitectEngine
{
    public enum ROOFTYPE
    {
        NONE,
        DEFAULT,
        FLAT,
        BALCONYFLAT
    }

    public partial class RoomArchitect : MonoBehaviour
    {
        /// <summary>
        /// Automatically generates a roof for every part that would need to be capped with a roof
        /// </summary>
        public void autoBuildRoof(ROOFTYPE roofType = ROOFTYPE.DEFAULT)
        {
            unchecked
            {
                mapRoof();
                foreach (Roof r in roof.roofs)
                {
                    buildRoof(r);
                }
            }
        }

        /// <summary>
        /// Builds a single roof
        /// </summary>
        /// <param name="roof"></param>
        public void buildRoof(Roof roof)
        {
            ROOFTYPE t = roofcurrStyle;
            RoofStyle rs = this.gameObject.GetComponent<RoofStyle>();
            if (rs != null)
                t = rs.roofType;
            if (t == ROOFTYPE.DEFAULT)
                buildSingleDefaultRoof(roof);
            else if (t == ROOFTYPE.FLAT)
                addFlatRoof(roof, false);
            else if (t == ROOFTYPE.BALCONYFLAT)
                addFlatRoof(roof, true);
        }


        void buildSingleDefaultRoof(Roof r)
        {
            r.inclination = 0.8f;
            r.flatTopSize = 0.05f;
            List<Verge> vergeSubList = new List<Verge>(roof.verges);
            roofGenerationTemporaryData dt = new roofGenerationTemporaryData();
            dt.roofTexture = new BoxTexture();
            dt.roofTexture.textureID[Directions.UP] = currentRoofTex;
            dt.roofTexture.textureID[Directions.CENTER] = currentOutdoorTex;
            dt.roofTexture.textureID[Directions.NORTH] = currentCeilingTex;
            dt.newRoof = (new GameObject()).AddComponent<ProceduralObject>();
            dt.newRoof.transform.SetParent(roofHolder.transform, false);
            dt.newRoof.name = "roof";
            dt.newRoof.setTextureMode(TEXTUREMODE.ROOF, Vector3.one);
            addRoof(ref dt, vergeSubList, r);
            Transform meshTr = dt.newRoof.transform;
            dt.newRoof = (new GameObject()).AddComponent<ProceduralObject>();
            dt.collisionMesh = true;
            addRoof(ref dt, vergeSubList, r);
            dt.newRoof.transform.SetParent(meshTr);
            dt.newRoof.transform.localPosition = Vector3.zero;
            dt.newRoof.name = "roof collider";
        }

        void addFlatRoof(Roof r, bool balcony)
        {
            roofGenerationTemporaryData dt = new roofGenerationTemporaryData();

            addFlatRoof(ref dt, r, balcony);
            dt.newRoof.createMesh();
            dt.newRoof.transform.Translate(0, -wallThickness * 0.5f, 0);
            dt.newRoof.gameObject.AddComponent<BoxCollider>();
            dt.newRoof.gameObject.name = "roof";
            GameObject go = new GameObject();
            go.name = "roff";
            go.transform.SetParent(dt.newRoof.transform.parent, false);
            dt.newRoof.transform.SetParent(go.transform, true);
            go.AddComponent<MeshCombiner>().combine(true);
        }

        void addFlatRoof(ref roofGenerationTemporaryData dt, Roof r, bool balcony)
        {
            dt.roofTexture = new BoxTexture();
            for (int i = 0; i < dt.roofTexture.textureID.Length; i++)
                dt.roofTexture.textureID[i] = currentOutdoorTex;
            dt.roofTexture.textureID[Directions.UP] = currentRoofTex;
            dt.roofTexture.textureID[Directions.CENTER] = currentCeilingTex;
            dt.newRoof = (new GameObject()).AddComponent<ProceduralObject>();
            dt.newRoof.transform.SetParent(roofHolder.transform, false);
            dt.newRoof.init(this, materials);
            dt.newRoof.setTextureMode(TEXTUREMODE.BOX, RealDimensionsVector, new Vector2(-wallThickness * 0.5f, 0));
            dt.bottomLeft = r.BottomLeft.toVector3(RealDimensionsVector);
            dt.topRight = r.TopRight.toVector3(RealDimensionsVector);
            dt.bottomLeft -= ToolBox.newV(x: 1, z: 1) * wallThickness * 0.5f;
            dt.topRight += ToolBox.newV(x: 1, z: 1) * wallThickness * 0.5f;
            dt.size = dt.topRight - dt.bottomLeft;
            GameObject roofBorders = new GameObject();
            if (balcony)
            {
                Roof swappedRoof = r;
                roofBorders.transform.SetParent(roofHolder.transform);
                roofBorders.transform.localPosition = dt.bottomLeft;
                foreach (Verge v in roof.verges)
                {
                    if (v.horizontal && v.length == r.size.x)
                    {
                        if (v.position == r.position)
                            addFlatRoofVerge(r, new Vector3(dt.size.x * 0.5f, 0, wallThickness * 0.5f),
                                             new Vector3(dt.size.x, 1.30f, wallThickness), dt, roofBorders.transform, "south");
                        else if (v.position == r.TopLeft)
                            addFlatRoofVerge(r, new Vector3(dt.size.x * 0.5f, 0, dt.size.z - wallThickness * 0.5f),
                                             new Vector3(dt.size.x, 1.30f, wallThickness), dt, roofBorders.transform, "north");

                    }
                    else if (v.length == r.size.z)
                    {
                        if (v.position == r.position)
                            addFlatRoofVerge(r, new Vector3(wallThickness * 0.5f, 0, dt.size.z * 0.5f),
                                             new Vector3(wallThickness, 1.30f, dt.size.z), dt, roofBorders.transform, "west");
                        else if (v.position == r.BottomRight)
                            addFlatRoofVerge(r, new Vector3(dt.size.x - wallThickness * 0.5f, 0, dt.size.z * 0.5f),
                                             new Vector3(wallThickness, 1.30f, dt.size.z), dt, roofBorders.transform, "east");
                    }
                }
            }
            if (r.horizontal)
            {
                dt.size = new Vector3(dt.size.z, dt.size.y, dt.size.x);
            }
            dt.size.y = wallThickness;
            dt.newRoof.addBox(dt.size / 2f, new Vector3(dt.size.x, 0.01f, dt.size.z), dt.roofTexture, "", true);
            placeRoof(ref dt, r);
            roofBorders.transform.SetParent(dt.newRoof.transform, true);
            //roofBorders.gameObject.AddComponent<MeshCombiner>().combine(true);
        }

        void addFlatRoofVerge(Roof roof, Vector3 pos, Vector3 size, roofGenerationTemporaryData dt, Transform parnt, string partName)
        {
            ProceduralObject po = new GameObject().AddComponent<ProceduralObject>();
            po.init(this, materials);
            po.addBox(Vector3.zero, size, dt.roofTexture, "", true);
            po.createMesh();
            po.transform.SetParent(parnt);
            po.transform.localPosition = pos;
            po.gameObject.name = "verge " + partName;
        }

        /// <summary>
        /// sub routine of "buildRoof"
        /// Mesh contruction of a single piece of roof
        /// </summary>
        /// <param name="bottomLeft"></param>
        /// <param name="topRight"></param>
        /// <param name="inclination"></param>
        /// <param name="horizontal"></param>
        /// <param name="flatTopCap"></param>
        void addRoof(ref roofGenerationTemporaryData dt, List<Verge> vergeSubList, Roof roof)
        {
            roof.flatTopSize = roofFlatSize;
            roof.inclination = roofHeight;
            RoofStyle rs = this.gameObject.GetComponent<RoofStyle>();
            if (rs != null)
            {
                roof.flatTopSize = rs.flatTopAmount;
                roof.inclination = rs.height;
            }
            if (roof.flatTopSize >= 1)
                roof.flatTopSize = 0.999f;
            dt.inclination = roof.inclination / roofHeightMultiplier;
            dt.bottomLeft = roof.BottomLeft.toVector3(RealDimensionsVector);
            dt.topRight = roof.TopRight.toVector3(RealDimensionsVector);
            dt.bottomLeft -= ToolBox.newV(x: 1, z: 1) * wallThickness * 0.5f;
            dt.topRight += ToolBox.newV(x: 1, z: 1) * wallThickness * 0.5f;
            dt.size = dt.topRight - dt.bottomLeft;
            if (roof.horizontal)
                dt.size = new Vector3(dt.size.z, dt.size.y, dt.size.x);
            dt.LengthBy2 = dt.size.x * 0.5f;
            dt.yLength = Mathf.Sin(dt.inclination) * dt.LengthBy2 * roofHeightMultiplier;
            dt.flatTopCap = roof.flatTopSize * dt.LengthBy2;

            dt.newRoof.init(this, materials);
            addSlope(ref dt, roof, null, Vector3.zero, dt.size.z);
            if (roof.horizontal)
                addRoofVergesH(ref dt, roof, ref vergeSubList);
            else
                addRoofVergesV(ref dt, roof, ref vergeSubList);
            dt.newRoof.setTextureMode(TEXTUREMODE.BOX, RealDimensionsVector, new Vector2(-wallThickness * 0.5f, 0));
            // BOT LEFT TRI
            dt.newRoof.addTri(ToolBox.newV(0, 0, 0),
                            ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0 + dt.yLength, 0),
                            ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0, 0), false, dt.roofTexture.textureID[Directions.CENTER]);
            dt.newRoof.addTri(ToolBox.newV(0, 0, 0),
                            ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0 + dt.yLength, 0),
                            ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0, 0), true, dt.roofTexture.textureID[Directions.CENTER]);
            // BOT RIGHT TRI
            dt.newRoof.addTriDoubleSide(ToolBox.newV(dt.size.x - dt.LengthBy2 + dt.flatTopCap, 0, 0),
                            ToolBox.newV(dt.size.x - dt.LengthBy2 + dt.flatTopCap, 0 + dt.yLength, 0),
                            ToolBox.newV(dt.size.x, 0, 0), dt.roofTexture.textureID[Directions.CENTER]);
            // TOP LEFT TRI
            dt.newRoof.addTriDoubleSide(ToolBox.newV(0, 0, dt.size.z),
                            ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0, dt.size.z),
                            ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0 + dt.yLength, dt.size.z), dt.roofTexture.textureID[Directions.CENTER]);
            // TOP RIGHT TRI
            dt.newRoof.addTriDoubleSide(ToolBox.newV(dt.size.x - dt.LengthBy2 + dt.flatTopCap, 0, dt.size.z),
                            ToolBox.newV(dt.size.x, 0, dt.size.z),
                            ToolBox.newV(dt.size.x - dt.LengthBy2 + dt.flatTopCap, 0 + dt.yLength, dt.size.z), dt.roofTexture.textureID[Directions.CENTER]);
            // FRONT VERT SQUARE
            dt.newRoof.addPaneDoubleSide(ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0, 0),
                             ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0 + dt.yLength, 0),
                             ToolBox.newV(0 + dt.LengthBy2 + dt.flatTopCap, 0 + dt.yLength, 0),
                             ToolBox.newV(0 + dt.LengthBy2 + dt.flatTopCap, 0, 0), dt.roofTexture.textureID[Directions.CENTER]);
            // BACK VERTICAL SQUARE
            dt.newRoof.addPaneDoubleSide(ToolBox.newV(0 + dt.LengthBy2 + dt.flatTopCap, 0, dt.size.z),
                             ToolBox.newV(0 + dt.LengthBy2 + dt.flatTopCap, 0 + dt.yLength, dt.size.z),
                             ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0 + dt.yLength, dt.size.z),
                             ToolBox.newV(0 + dt.LengthBy2 - dt.flatTopCap, 0, dt.size.z), dt.roofTexture.textureID[Directions.CENTER]);
            if (dt.collisionMesh)
            {
                dt.newRoof.gameObject.AddComponent<MeshCollider>().convex = true;
                dt.newRoof.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            // CEILING
            dt.newRoof.addPane(ToolBox.newV(0, 0, 0),
                             ToolBox.newV(dt.size.x, 0, 0),
                             ToolBox.newV(dt.size.x, 0, dt.size.z),
                             ToolBox.newV(0, 0, dt.size.z), "", false, dt.roofTexture.textureID[Directions.NORTH]);

            dt.newRoof.createMesh();
            placeRoof(ref dt, roof);
        }

        void placeRoof(ref roofGenerationTemporaryData dt, Roof roof)
        {
            dt.newRoof.transform.localPosition = new Vector3(dt.bottomLeft.x,
                                                             dt.bottomLeft.y,
                                                             dt.bottomLeft.z);
            if (roof.horizontal)
            {
                dt.newRoof.transform.Rotate(Vector3.up, 90);
                dt.newRoof.transform.Translate(-dt.size.x, 0, 0);
            }
            dt.newRoof.transform.localScale *= 0.999f;
            float disp = (1f - 0.999f) * 0.5f;
            dt.newRoof.transform.Translate(dt.size * disp);
        }


        void addRoofVergesV(ref roofGenerationTemporaryData dt, Roof roof, ref List<Verge> vergeSubList)
        {
            if (dt.collisionMesh)
                return;
            List<Verge> toRemove = new List<Verge>();
            List<FallToAdd> fallToAdd = new List<FallToAdd>();

            foreach (Verge v in vergeSubList)
            {
                if (v.position.y != roof.position.y)
                    continue;
                if (v.horizontal && roof.position.x >= v.position.x && roof.BottomRight.x <= v.position.offsetBy(x: v.length).x)
                {
                    if (roof.position.z == v.position.z)
                        addSlope(ref dt, roof, v, new Vector3(0, 0, -roof.vergeThickness), roof.vergeThickness);
                    else if (roof.TopLeft.z == v.position.z)
                        addSlope(ref dt, roof, v, new Vector3(0, 0, wallThickness + (v.position - roof.position).z * HorizontalScale), roof.vergeThickness);
                }
                else if (!v.horizontal && v.position.z >= roof.BottomLeft.z && v.position.z < roof.TopLeft.z)
                {
                    if (v.position.x == roof.position.x)
                        fallToAdd.Add(new FallToAdd(Vector3.zero, false, v));
                    if (v.position.x == roof.position.offsetBy(roof.size.shortestXZ).x)
                        fallToAdd.Add(new FallToAdd(Vector3.zero, true, v));
                }
            }
            foreach (FallToAdd f in fallToAdd)
            {
                addFall(ref dt, roof, f.verge, f.inverted, f.offset);
            }
            foreach (Verge v in toRemove)
                vergeSubList.Remove(v);
        }

        void addRoofVergesH(ref roofGenerationTemporaryData dt, Roof roof, ref List<Verge> vergeSubList)
        {
            if (dt.collisionMesh)
                return;
            List<Verge> toRemove = new List<Verge>();
            Roof swappedRoof = roof.swapXZ();
            List<FallToAdd> fallToAdd = new List<FallToAdd>();

            foreach (Verge v in vergeSubList)
            {
                if (v.position.y != roof.position.y)
                    continue;
                if (!v.horizontal && roof.position.z >= v.position.z && roof.TopLeft.z <= v.position.offsetBy(z: roof.size.z).z)
                {
                    if (v.position.x == roof.position.x)
                        addSlope(ref dt, roof, v, new Vector3(0, 0, -roof.vergeThickness), roof.vergeThickness);
                    else if (v.position.x == roof.BottomRight.x)
                        addSlope(ref dt, roof, v, new Vector3(0, 0, wallThickness + swappedRoof.size.z * HorizontalScale), roof.vergeThickness);
                }
                else if (v.horizontal && v.position.x >= roof.BottomLeft.x && v.position.x < roof.BottomRight.x)
                {
                    if (v.position.z == roof.position.offsetBy(z: roof.size.shortestXZ).z)
                    {
                        //addFall(ref dt, roof, v false, new Vector3(0, 0, -roof.size.shortestXZ * HorizontalScale));
                        fallToAdd.Add(new FallToAdd(new Vector3(0, 0, -roof.size.shortestXZ * HorizontalScale), false, v));
                    }
                    if (v.position.z == roof.position.z)
                    {
                        //addFall(ref dt, roof, v, true, new Vector3(roof.size.shortestXZ * HorizontalScale, 0, 0));
                        fallToAdd.Add(new FallToAdd(new Vector3(roof.size.shortestXZ * HorizontalScale, 0, 0), true, v));
                    }
                }
            }
            foreach (FallToAdd f in fallToAdd)
            {
                addFall(ref dt, roof, f.verge, f.inverted, f.offset);
            }
            foreach (Verge v in toRemove)
                vergeSubList.Remove(v);
        }


        void addSlope(ref roofGenerationTemporaryData dt, Roof roof, Verge verge, Vector3 offset, float len)
        {
            if (verge != null)
            {
                if (verge.position == roof.position)
                    roof.roofCorners.bottomLeft = true;
                if (verge.position == roof.BottomRight ||
                    (verge.horizontal && verge.position.offsetBy(verge.length) == roof.BottomRight))
                    roof.roofCorners.bottomRight = true;
                if (verge.position == roof.TopLeft ||
                    (!verge.horizontal && verge.position.offsetBy(z: verge.length) == roof.TopLeft))
                    roof.roofCorners.topLeft = true;
                if (verge.position.offsetBy(x: verge.length) == roof.TopRight ||
                    verge.position.offsetBy(z: verge.length) == roof.TopRight)
                    roof.roofCorners.topRight = true;
            }
            dt.newRoof.pivotTexture90 = true;
            dt.newRoof.addThickPane(offset + ToolBox.newV(0, 0, 0),
                             offset + ToolBox.newV(0, 0, len),
                             offset + ToolBox.newV(dt.LengthBy2 - dt.flatTopCap, dt.yLength, len),
                             offset + ToolBox.newV(dt.LengthBy2 - dt.flatTopCap, dt.yLength, 0), 0.08f, dt.roofTexture.textureID[Directions.UP]);
            // TOP CAP
            dt.newRoof.addThickPane(offset + ToolBox.newV(dt.LengthBy2 - dt.flatTopCap, dt.yLength, 0),
                             offset + ToolBox.newV(dt.LengthBy2 - dt.flatTopCap, dt.yLength, len),
                             offset + ToolBox.newV(dt.LengthBy2 + dt.flatTopCap, dt.yLength, len),
                             offset + ToolBox.newV(dt.LengthBy2 + dt.flatTopCap, dt.yLength, 0), 0.08f, dt.roofTexture.textureID[Directions.UP]);
            dt.newRoof.pivotTextureI90 = true;
            // RIGHT SLOPE
            dt.newRoof.addThickPane(offset + ToolBox.newV(dt.size.x, 0, len),
                             offset + ToolBox.newV(dt.size.x, 0, 0),
                             offset + ToolBox.newV(dt.LengthBy2 + dt.flatTopCap, dt.yLength, 0),
                             offset + ToolBox.newV(dt.LengthBy2 + dt.flatTopCap, dt.yLength, len), 0.08f, dt.roofTexture.textureID[Directions.UP]);
            dt.newRoof.pivotTexture90 = false;
            dt.newRoof.pivotTextureI90 = false;
            // LEFT SLOPE
            //dt.newRoof.addThickPane(offset + ToolBox.newV(0, 0, 0),
            //                 offset + ToolBox.newV(0, 0, len),
            //                 offset + ToolBox.newV(dt.LengthBy2 - dt.flatTopCap, dt.yLength, len),
            //                 offset + ToolBox.newV(dt.LengthBy2 - dt.flatTopCap, dt.yLength, 0), 0.08f, dt.roofTexture.textureID[Directions.UP]);
        }


        void addFall(
            ref roofGenerationTemporaryData dt,
            Roof roof,
            Verge verge,
            bool invertedSlope,
            Vector3 offset)
        {
            dt.newRoof.pivotTexture90 = true;
            if (invertedSlope)
                dt.newRoof.pivotTextureI90 = true;
            // Look at your notes, ylen * (0.1 / xlen)
            float height = dt.yLength * (roof.vergeThickness / dt.LengthBy2);
            float yOffset = 0;
            Vector3 position = offset + (verge.position - roof.position).toVector3(RealDimensionsVector);
            float fallLength = verge.length * HorizontalScale + wallThickness;
            if (invertedSlope)
            {
                height = -height;
                yOffset = height;
                position.x += wallThickness + roof.vergeThickness;
            }
            if ((roof.roofCorners.bottomLeft && !invertedSlope && verge.position.XZEquals(roof.position)) ||
                (roof.roofCorners.bottomRight && invertedSlope && verge.position.XZEquals(roof.BottomRight)) ||
                (roof.roofCorners.bottomLeft && invertedSlope && verge.position.XZEquals(roof.position)) ||
                (roof.roofCorners.topLeft && !invertedSlope && verge.position.XZEquals(roof.TopLeft)))
            {
                position.z -= roof.vergeThickness;
                fallLength += roof.vergeThickness;
            }
            if ((roof.roofCorners.topLeft && !invertedSlope && verge.position.offsetBy(z: verge.length).XZEquals(roof.TopLeft)) ||
                (roof.roofCorners.topRight && invertedSlope && verge.position.offsetBy(z: verge.length).XZEquals(roof.TopRight)) ||
                (roof.roofCorners.topRight && !invertedSlope && verge.position.offsetBy(x: roof.size.x).XZEquals(roof.TopRight)) ||
                (roof.roofCorners.bottomRight && invertedSlope && verge.position.offsetBy(x: roof.size.x).XZEquals(roof.BottomRight)))
                fallLength += roof.vergeThickness;
            dt.newRoof.addThickPane(position + ToolBox.newV(-roof.vergeThickness, yOffset - height, 0),
                                    position + ToolBox.newV(-roof.vergeThickness, yOffset - height, fallLength),
                                    position + ToolBox.newV(0, yOffset, fallLength),
                                    position + ToolBox.newV(0, yOffset, 0), 0.08f, dt.roofTexture.textureID[Directions.UP]);
            dt.newRoof.pivotTexture90 = false;
            dt.newRoof.pivotTextureI90 = false;
        }
    }

    public class roofGenerationTemporaryData
    {
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public List<Verge> vergeSubList;
        public float inclination;
        public float flatTopCap;
        public Vector3 size;
        public float LengthBy2;
        public float yLength;
        public ProceduralObject newRoof;
        public bool collisionMesh = false;
        public BoxTexture roofTexture;
    }
}