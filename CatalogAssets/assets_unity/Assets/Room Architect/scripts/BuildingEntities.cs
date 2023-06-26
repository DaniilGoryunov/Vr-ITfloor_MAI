using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RoomArchitectEngine;


namespace RoomArchitectEngine
{
    /// <summary>
    /// Represents a single room
    /// </summary>
    public class Room
    {
        public Dictionary<Position, WallPart> walls;
        public int floor
        {
            get
            {
                return BottomLeft.y;
            }
        }
        public Position BottomLeft { get; set; }
        public Position TopRight { get; set; }

        public Position BottomRight
        {
            get
            {
                return BottomLeft.offsetBy(x: Size.x);
            }
        }

        public Position TopLeft
        {
            get
            {
                return BottomLeft.offsetBy(z: Size.z);
            }
        }

        public Position Size
        {
            get
            {
                return TopRight - BottomLeft;
            }
        }

        public Room(Position p1, Position p2)
        {
            walls = new Dictionary<Position, WallPart>();
            Position.swapValuesByIncrementalOrder(ref p1, ref p2);
            BottomLeft = p1;
            TopRight = p2;
        }

        /// <summary>
        /// returns wether p is in the current room (inclusive)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Contains(Position p, bool excludeY = true)
        {
            if (p.isBetween(this.BottomLeft - new Position(1, 0, 1), this.TopRight, excludeY))
                return true;
            return false;
        }

        public bool Overlaps(Room r2)
        {
            Rect rect1 = this.toRect();
            Rect rect2 = r2.toRect();
            if (this.floor != r2.floor)
                return false;
            else if (rect1.Overlaps(rect2))
                return true;
            else
            return false;
        }

        public Rect toRect()
        {
            return new Rect(new Vector2(this.BottomLeft.x, this.BottomLeft.z), new Vector2(this.Size.x, this.Size.z));
        }

    }

    /// <summary>
    /// Internal class, end user shouldn't is not supposed to manipulate this
    /// </summary>
    public class Verge
    {
        public Position position;
        public bool horizontal;
        public int length;

        public Verge(Position position, bool isHorizontal)
        {
            this.position = position;
            this.horizontal = isHorizontal;
            length = 1;
        }

        public Position topLeft
        {
            get
            {
                if (horizontal)
                    return position;
                else
                    return position.offsetBy(0, 0, length);
            }
        }

        public Position bottomRight
        {
            get
            {
                if (horizontal)
                    return position.offsetBy(length, 0, 0);
                else
                    return position;
            }
        }

    }

    /// <summary>
    /// Represents a single rectangular roof
    /// </summary>
    public class Roof : Rectangle
    {
        public bool horizontal;
        public float vergeThickness = 0.3f;
        public RoofCornerExtensions roofCorners;
        public bool hasVerges = true;
        public float inclination = 0.5f;
        public float flatTopSize = 0;

        public Roof(Rectangle rectangle, bool horizontal = false) : base(rectangle)
        {
            this.horizontal = horizontal;
            roofCorners = new RoofCornerExtensions();
        }

        public Roof copy()
        {
            return new Roof(this, this.horizontal);
        }

        new public Roof swapXZ()
        {
            return new Roof(new Rectangle(position, size.swapXZ()), !horizontal);
        }
    }

    /// <summary>
    /// Internal Object, the end user shouldn't manipulate this.
    /// </summary>
    public class InternalRoof
    {
        public List<Roof> roofs;
        public List<Verge> verges;
        public Roof biggest;
        public InternalRoof()
        {
            biggest = new Roof(Rectangle.Zero);
            roofs = new List<Roof>();
            verges = new List<Verge>();
        }

        public void addNewRoof(Rectangle rect)
        {
            var newRoof = new Roof(rect, (rect.size.x > rect.size.z ? true : false));
            if (roofs.Count <= 0)
                biggest = newRoof;
            if (newRoof.isSquare)
                newRoof.horizontal = biggest.horizontal;
            roofs.Add(newRoof);
        }
    }

    /// <summary>
    /// Internal Object, the end user shouldn't manipulate this.
    /// </summary>
    public class FallToAdd
    {
        public Vector3 offset;
        public bool inverted = false;
        public Verge verge;

        public FallToAdd(Vector3 offset, bool inverted, Verge verge)
        {
            this.offset = offset;
            this.inverted = inverted;
            this.verge = verge;
        }
    }

    /// <summary>
    /// Internal Object, the end user shouldn't manipulate this.
    /// </summary>
    public class RoofCornerExtensions
    {
        public bool bottomLeft, bottomRight, topLeft, topRight;

        public RoofCornerExtensions()
        {
            bottomLeft = false;
            bottomRight = false;
            topLeft = false;
            topRight = false;
        }
    }

    /// <summary>
    /// Internal Object, the end user shouldn't manipulate this.
    /// </summary>
    public class FloorTile
    {
        public Position position;
        public bool disabled = false;

        public FloorTile(Position position)
        {
            this.position = position;
        }

        public void Set(Position p)
        {
            this.position = p;
        }
    }

    public enum STAIRSUPPORTTYPE
    {
        NONE,
        SLOPE,
        FULL
    }

    /// <summary>
    /// This object represents a double stair (Half way up one direction, and half way up the opposite direction)
    /// </summary>
    public class DoubleStair : SingleStair
    {
        public SingleStair st1;
        public SingleStair st2;
        public DoubleStair(Vector3 bottomPosition, Vector3 topPosition, int stepNumber, StairStyle style, bool isHorizontal = true) :
               base(bottomPosition, topPosition, stepNumber, style, isHorizontal)
        {
        }

        public void getHalfStairs(Directions orientation, float HScale, float thickness)
        {
            initOrientation(orientation);
            st1 = new SingleStair(bottomPosition, topPosition, stepNumber, style);
            st2 = new SingleStair(bottomPosition, topPosition, stepNumber, style);
            if (orientation.isEither(Directions.NORTH, Directions.SOUTH))
            {
                st1.initOrientation(orientation);
                st2.initOrientation(orientation.opposite());
            }
            else
            {
                st2.initOrientation(orientation);
                st1.initOrientation(orientation.opposite());
            }
            if (orientation == Directions.NORTH)
            {
                st1.bottomPosition = bottomPosition + new Vector3(0, 0, 0);
                st1.topPosition = bottomPosition + new Vector3(halfWidth, halfHeight, length - HScale);
                st2.bottomPosition = bottomPosition + new Vector3(halfWidth, halfHeight, 0);
                st2.topPosition = bottomPosition + new Vector3(width, height, length - HScale);
                st1.topPad.pos = new Vector3(length - HScale * 0.5f, halfHeight - thickness, 0);
                st1.topPad.size = new Vector3(HScale, thickness, width);
            }
            else if (orientation == Directions.SOUTH)
            {
                st1.bottomPosition = bottomPosition + new Vector3(0, 0, HScale);
                st1.topPosition = bottomPosition + new Vector3(halfWidth, halfHeight, length);
                st2.bottomPosition = bottomPosition + new Vector3(halfWidth, halfHeight, HScale);
                st2.topPosition = bottomPosition + new Vector3(width, height, length);
                st1.topPad.pos = new Vector3(length - HScale * 0.5f, halfHeight - thickness, halfWidth);
                st1.topPad.size = new Vector3(HScale, thickness, width);
            }
            // INVERT ST1 ST2 for EAST WEST
            else if (orientation == Directions.EAST)
            {
                st1.bottomPosition = bottomPosition + new Vector3(HScale, 0, 0);
                st1.topPosition = bottomPosition + new Vector3(length, halfHeight, halfWidth);
                st2.bottomPosition = bottomPosition + new Vector3(HScale, halfHeight, halfWidth);
                st2.topPosition = bottomPosition + new Vector3(length, height, width);
                st1.topPad.pos = new Vector3(length - HScale * 0.5f, halfHeight - thickness, 0);
                st1.topPad.size = new Vector3(HScale, thickness, width);
            }
            else
            {
                st1.bottomPosition = bottomPosition;
                st1.topPosition = bottomPosition + new Vector3(length - HScale, halfHeight, halfWidth);
                st2.bottomPosition = bottomPosition + new Vector3(0, halfHeight, halfWidth);
                st2.topPosition = bottomPosition + new Vector3(length - HScale, height, width);
                st1.topPad.pos = new Vector3(length - HScale * 0.5f, halfHeight - thickness, halfWidth);
                st1.topPad.size = new Vector3(HScale, thickness, width);
            }
            st1.style.buffer = -1;
            if (st2.style.fullBottom)
                st2.style.buffer = st1.height;
        }

        public float halfWidth
        {
            get 
            {
                return (width * 0.5f);
            }
        }

        public float halfLength
        {
            get
            {
                return (length * 0.5f);
            }
        }

        public float halfHeight
        {
            get
            {
                return (height * 0.5f);
            }
        }

    }

    /// <summary>
    /// This object represents a single stairway and its parameters
    /// </summary>
    public class SingleStair
    {
        public Vector3 bottomPosition, topPosition;
        public int stepNumber;
        public bool isHorizontal = true;
        public bool inverted = false;
        public StairStyle style;
        public v3Rect topPad;
        public List<v3Rect> rembs;

        public SingleStair(Vector3 bottomPosition, Vector3 topPosition, int stepNumber, StairStyle style, bool isHorizontal = true)
        {
            Vector3 newBottom = new Vector3((bottomPosition.x < topPosition.x ? bottomPosition.x : topPosition.x),
                                            (bottomPosition.y < topPosition.y ? bottomPosition.y : topPosition.y),
                                            (bottomPosition.z < topPosition.z ? bottomPosition.z : topPosition.z));
            Vector3 newTop = new Vector3((bottomPosition.x > topPosition.x ? bottomPosition.x : topPosition.x),
                                         (bottomPosition.y > topPosition.y ? bottomPosition.y : topPosition.y),
                                         (bottomPosition.z > topPosition.z ? bottomPosition.z : topPosition.z));
            topPad = new v3Rect();
            this.bottomPosition = newBottom;
            this.topPosition = newTop;
            this.stepNumber = stepNumber;
            this.isHorizontal = isHorizontal;
            this.style = style.copy();

        }

        public SingleStair(Vector3 bottomPosition, Vector3 topPosition, int stepNumber, StairStyle style, Directions orientation)
        {
            Vector3 newBottom = new Vector3((bottomPosition.x < topPosition.x ? bottomPosition.x : topPosition.x),
                                            (bottomPosition.y < topPosition.y ? bottomPosition.y : topPosition.y),
                                            (bottomPosition.z < topPosition.z ? bottomPosition.z : topPosition.z));
            Vector3 newTop = new Vector3((bottomPosition.x > topPosition.x ? bottomPosition.x : topPosition.x),
                                         (bottomPosition.y > topPosition.y ? bottomPosition.y : topPosition.y),
                                         (bottomPosition.z > topPosition.z ? bottomPosition.z : topPosition.z));
            this.bottomPosition = newBottom;
            this.topPosition = newTop;
            this.stepNumber = stepNumber;
            this.style = style;

        }

        public float length
        {
            get
            {
                if (isHorizontal)
                    return (topPosition.x - bottomPosition.x);
                return (topPosition.z - bottomPosition.z);
            }
        }

        public float width
        {
            get
            {
                if (isHorizontal)
                    return (topPosition.z - bottomPosition.z);
                return (topPosition.x - bottomPosition.x);
            }
        }

        public float height
        {
            get
            {
                return topPosition.y - bottomPosition.y;
            }
        }

        public Vector3 size
        {
            get
            {
                return new Vector3(length, height, width);
            }
        }

        public float steplength
        {
            get
            {
                return length / stepNumber;
            }
        }

        public float stepheight
        {
            get
            {
                return height / stepNumber;
            }
        }

        public void initOrientation(Directions orientation)
        {
            if (orientation == Directions.NORTH)
                isHorizontal = false;
            if (orientation == Directions.SOUTH)
            {
                inverted = true;
                isHorizontal = false;
            }
            if (orientation == Directions.WEST)
                inverted = true;
        }
    }

    public class v3Rect
    {
        public Vector3 pos, size;

        public v3Rect(Vector3 pos, Vector3 size)
        {
            this.pos = pos;
            this.size = size;
        }

        public v3Rect()
        {
            this.pos = new Vector3();
            this.size = new Vector3();
        }

    }

    public class StairStyle
    {
        public bool fullBottom = false;
        public RAILSTYLE railStyle = 0;
        public static StairStyle _instance = null;
        public float buffer = -1;

        public StairStyle()
        {

        }

        public StairStyle copy()
        {
            StairStyle s = new StairStyle();
            s.fullBottom = this.fullBottom;
            s.railStyle = this.railStyle;
            return s;
        }

        public static StairStyle Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StairStyle();
                return _instance;
            }

        }
    }

    public enum RAILSTYLE
    {   
        NONE,
        FULL
    }

    public class BoxTexture
    {
        public int[] textureID;

        public BoxTexture()
        {
            textureID = new int[6] { 0, 0, 0, 0, 0, 0 };
        }

        public BoxTexture(int north = -1, int south = -1, int east = -1, int west = -1, int center = -1, int top = -1, int rest = 0)
        {
            textureID = new int[6] { rest, rest, rest, rest, rest, rest };
            if (north != -1)
                textureID[Directions.NORTH] = north;
            if (south != -1)
                textureID[Directions.SOUTH] = south;
            if (east != -1)
                textureID[Directions.EAST] = east;
            if (west != -1)
                textureID[Directions.WEST] = west;
            if (top != -1)
                textureID[Directions.UP] = top;
            if (center != -1)
                textureID[Directions.CENTER] = center;
        }


        BoxTexture(BoxTexture copy)
        {
            textureID = new int[6] { copy.textureID[0],
                                 copy.textureID[1],
                                 copy.textureID[2],
                                 copy.textureID[3],
                                 copy.textureID[4],
                                 copy.textureID[5] };
        }

        public BoxTexture copy()
        {
            return new BoxTexture(this);
        }


        public void setTextureBySide(Directions direction, int id)
        {
            textureID[direction] = id;
        }

        public BoxTexture invertNorthSouth()
        {
            BoxTexture ret = new BoxTexture(this);
            int tmp = ret.textureID[Directions.NORTH];
            ret.textureID[Directions.NORTH] = ret.textureID[Directions.SOUTH];
            ret.textureID[Directions.SOUTH] = tmp;
            return ret;
        }

        public BoxTexture invertEastWest()
        {
            BoxTexture ret = new BoxTexture(this);
            int tmp = ret.textureID[Directions.EAST];
            ret.textureID[Directions.EAST] = ret.textureID[Directions.WEST];
            ret.textureID[Directions.WEST] = tmp;
            return ret;
        }
    }

    /// <summary>
    /// Data object representing a single fence line
    /// </summary>
    public class Fence
    {
        public Vector3 position;
        public float length;
        public Directions direction;
        public int inBetweenBarAmount;

        public Fence(Vector3 position, float length, Directions direction, int inBetweenBarAmount)
        {
            this.position = position;
            this.length = length;
            this.direction = direction;
            this.inBetweenBarAmount = inBetweenBarAmount;
        }
    }

    public class MeshPane
    {
        public Vector3 first, second, third, forth;
        public int textureID;
        public string separationID;
        public int floor;
        public float topHeight, bottomHeight;
        public bool invert;

        public MeshPane(Vector3 first, Vector3 second, Vector3 third, Vector3 forth, bool invert, int textureID, string separationID = "global")
        {
            this.first = first;
            this.second = second;
            this.third = third;
            this.forth = forth;
            bottomHeight = (first.y + forth.y) / 2f;
            topHeight = (second.y + third.y) / 2f;
            this.textureID = textureID;
            this.invert = invert;
            this.separationID = separationID;
        }

        Vector3 TMPcomp;

        public bool compare(MeshPane other)
        {
            if (this.invert != other.invert)
                return false;
            if (this.bottomHeight != other.bottomHeight)
                return false;
            if (this.topHeight != other.topHeight)
                return false;
            TMPcomp = this.third - other.second;
            float s = 0.001f;
            if (TMPcomp.sqrMagnitude > s || TMPcomp.sqrMagnitude < -s)
                return false;
            TMPcomp = this.forth - other.first;
            if (TMPcomp.sqrMagnitude > s || TMPcomp.sqrMagnitude < -s)
                return false;
            if (this.textureID != other.textureID)
                return false;
            if (this.separationID != other.separationID)
                return false;
            return true;
        }

        public MeshPane integrate(MeshPane other)
        {
            this.third = other.third;
            this.forth = other.forth;
            return this;
        }
    }
}


