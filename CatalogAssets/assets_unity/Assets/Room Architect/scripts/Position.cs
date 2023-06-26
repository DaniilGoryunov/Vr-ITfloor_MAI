using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This object represents a position in a 3D grid. Units in this object are integrals, and
/// do not define any scale. Position objects should not be used directly to place objects into
/// the scene. See toVector3() if you are looking for conversion
/// </summary>
public class Position : IEquatable<Position>
{
    public int x, y, z;

    /// <summary>
    /// returns the area  X * Z
    /// </summary>
    public int XZArea
    {
        get
        {
            return x * z;
        }
    }

    public Position()
    {
        init(0, 0, 0);
    }

    public Position(int x = 0, int y = 0, int z = 0)
    {
        init(x, y, z);
    }

    void init(int x = 0, int y = 0, int z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;

    }

    public static Position operator + (Position a, Position b)
    {
        return new Position(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static bool operator == (Position a, Position b)
    {
        if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
            return true;
        if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
            return false;
        Position res = a - b;
        if (res.x == 0 && res.y == 0 && res.z == 0)
            return true;
        return false;
    }

    public static bool operator !=(Position a, Position b)
    {
        if (a == b)
            return false;
        return true;
    }

    public override bool Equals(object obj)
    {
        Position p2 = obj as Position;
        if (p2 == this)
            return true;
        return false;
    }

    public static Position operator -(Position a, Position b)
    {
        return new Position(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Position operator -(Position a)
    {
        return new Position(-a.x, -a.y, -a.z);
    }

    public static Position operator *(Position a, int n)
    {
        return new Position(a.x * n, a.y * n, a.z * n);
    }


    /// <summary>
    /// the Euclidian Length of the Position object
    /// </summary>
    public float DistanceToZero
    {
        get
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
    }


    /// <summary>
    /// adds X and Z of the incoming Position to the source object, and ignores Y parameter
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public Position addXZ(Position b)
    {
        return new Position(this.x + b.x, this.y, this.z + b.z);
    }

    /// <summary>
    /// returns a offset copy of the source Position
    /// Example A Position [10,0,5].shiftedBy(2,0,0) returns [12,0,5]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public Position offsetBy(int x = 0, int y = 0, int z = 0)
    {
        return this.copy() + new Position(x, y, z);
    }

    public Position offsetBy(Position p)
    {
        return offsetBy(p.x, p.y, p.z);
    }


    /// <summary>
    /// returns a offset copy of the source Position
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="value"></param>
    public Position offsetBy(Directions direction, int value)
    {
        Position newPos = this.copy();

        if (direction == Directions.NORTH)
            newPos.z += value;
        else if (direction == Directions.SOUTH)
            newPos.z -= value;
        else if (direction == Directions.EAST)
            newPos.x += value;
        else if (direction == Directions.WEST)
            newPos.x -= value;
        return newPos;
    }

    /// <summary>
    /// the same Position with y incremented by one
    /// </summary>
    /// <returns></returns>
    public Position upOneFloor()
    {
        return this.copy() + new Position(0, 1, 0);
    }

    /// <summary>
    /// Returns a copy of the Position with X and Z swapped
    /// </summary>
    public Position swapXZ()
    {
        return new Position(z, y, x);
    }

    /// <summary>
    /// Compares X and Z, ignoring Y
    /// </summary>
    /// <param name="p"></param>
    public bool XZEquals(Position p)
    {
        if (this.x == p.x && this.z == p.z)
            return true;
        return false;
    }

    /// <summary>
    /// Returns the bigger value between X and Z
    /// </summary>
    public int longestXZ
    {
        get
        {
            return (x > z ? x : z);
        }
    }

    /// <summary>
    /// Returns the smaller value between X and Z
    /// </summary>
    public int shortestXZ
    {
        get
        {
            return (x < z ? x : z);
        }
    }

    /// <summary>
    /// Returns a copy of the Position object with Y set to 0
    /// </summary>
    /// <returns></returns>
    public Position YToZero()
    {
        return new Position(x, 0, z);
    }

    /// <summary>
    /// return the Position with absolute positive values
    /// </summary>
    /// <returns></returns>
    public Position toAbsoluteValue()
    {
        return new Position(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
    }

    /// <summary>
    /// returns x + z;
    /// </summary>
    /// <returns></returns>
    public float XZSum()
    {
        return x + z;
    }

    /// <summary>
    /// swaps p1 and p2 references
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    public static void swap(ref Position p1, ref Position p2)
    {
        Position tmpp = p1;
        p1 = p2;
        p2 = tmpp;
    }

    /// <summary>
    /// return a copy of the current object
    /// </summary>
    /// <returns></returns>
    public Position copy()
    {
        return new Position(x, y, z);
    }

    /// <summary>
    /// This method stores the minimal values of any of the two Position into p1, and the maximals in p2
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    public static void swapValuesByIncrementalOrder(ref Position p1, ref Position p2)
    {
        Position newP1 = new Position((p1.x < p2.x ? p1.x : p2.x),
                                      (p1.y < p2.y ? p1.y : p2.y),
                                      (p1.z < p2.z ? p1.z : p2.z));
        Position newP2 = new Position((p1.x > p2.x ? p1.x : p2.x),
                                      (p1.y > p2.y ? p1.y : p2.y),
                                      (p1.z > p2.z ? p1.z : p2.z));
        p1 = newP1;
        p2 = newP2;
    }

    /// <summary>
    /// returns a new Position contaning the smallest values of p1 and p2
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static Position getMinValues(ref Position p1, ref Position p2)
    {
        return new Position((p1.x < p2.x ? p1.x : p2.x),
                            (p1.y < p2.y ? p1.y : p2.y),
                            (p1.z < p2.z ? p1.z : p2.z));
    }

    /// <summary>
    /// returns a new Position contaning the biggest values of p1 and p2
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static Position getMaxValues(ref Position p1, ref Position p2)
    {
        return new Position((p1.x > p2.x ? p1.x : p2.x),
                            (p1.y > p2.y ? p1.y : p2.y),
                            (p1.z > p2.z ? p1.z : p2.z));
    }

    /// <summary>
    /// returns wether the current Object x,y,z values are in between min and max (exclusive on x and z, and inclusive on y)
    /// </summary>
    /// <param name="min">the minimal x,y,z values</param>
    /// <param name="max">the maximal x,y,z values</param>
    /// <param name="excludeY"></param>
    /// <returns></returns>
    public bool isBetween(Position min, Position max, bool excludeY = false)
    {
        if (this.x <= min.x || this.x >= max.x)
            return false;
        if (!excludeY && (this.y < min.y || this.y > max.y))
            return false;
        if (this.z <= min.z || this.z >= max.z)
            return false;
        return true;
    }

    public bool isAlignedXorZ(Position to)
    {
        if (this.x == to.x || this.z == to.z)
            return true;
        return false;
    }


    public bool Equals(Position other)
    {
        if (this.x == other.x && this.y == other.y && this.z == other.z)
            return true;
        return false;
    }

    public Vector3 toVector3(Vector3 scale)
    {
        return new Vector3(this.x * scale.x, this.y * scale.y, this.z * scale.z);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString()
    {
        return "[" + x.ToString() + ";" + y.ToString() + ";" + z.ToString() + "]";
    }

    /// <summary>
    /// return new Position(0, 0, 0);
    /// </summary>
    public static Position Zero
    {
        get
        {
            return new Position(0, 0, 0);
        }
    }

    /// <summary>
    /// return new Position(1, 1, 1);
    /// </summary>
    public static Position One
    {
        get
        {
            return new Position(1, 1, 1);
        }
    }

    /// <summary>
    /// return new Position(-1, 0, 0);
    /// </summary>
    public static Position Left
    {
        get
        {
            return new Position(-1, 0, 0);
        }
    }

    /// <summary>
    /// return new Position(1, 0, 0);
    /// </summary>
    public static Position Right
    {
        get
        {
            return new Position(1, 0, 0);
        }
    }

    /// <summary>
    /// return new Position(0, 0, 1);
    /// </summary>
    public static Position Forward
    {
        get
        {
            return new Position(0, 0, 1);
        }
    }

    /// <summary>
    /// return new Position(0, 0, -1);
    /// </summary>
    public static Position Backward
    {
        get
        {
            return new Position(0, 0, -1);
        }
    }

}

/// <summary>
/// Object containing a position and a size.
/// Note: the position representsx the bottom left corner of the Rectangle
/// </summary>
public class Rectangle
{
    public Position position, size;

    public Position BottomLeft
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
        }
    }

    public Position BottomRight
    {
        get
        {
            return position.offsetBy(size.x);
        }
    }

    public Position TopRight
    {
        get
        {
            return position + size;
        }
    }

    public Position TopLeft
    {
        get
        {
            return position + new Position(0, 0, size.z);
        }
    }

    public static Rectangle Zero
    {
        get
        {
            return new Rectangle(Position.Zero, Position.Zero);
        }
    }

    public Rectangle(Position position, Position size)
    {
        init(position, size);
    }

    public Rectangle(Rectangle r)
    {
        init(r.position, r.size);
    }


    void init(Position position, Position size)
    {
        this.position = position;
        this.size = size;
    }

    public override string ToString()
    {
        return "[pos: " + this.position.ToString() + " - size: " + this.size.ToString() + "]";
    }

    public static bool operator ==(Rectangle a, Rectangle b)
    {
        if (a.position == b.position && a.size == b.size)
            return true;
        return false;
    }

    public static bool operator !=(Rectangle a, Rectangle b)
    {
        if (!(a == b))
            return true;
        return false;
    }

    public override bool Equals(object obj)
    {
        Rectangle pobj = obj as Rectangle;
        if (pobj.position == this.position && pobj.size == this.size)
            return true;
        return false;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public int Area
    {
        get
        {
            return size.x * size.z;
        }
    }

    public bool isSquare
    {
        get
        {
            return (size.x == size.z ? true : false);
        }
    }

    /// <summary>
    /// returns a copy of the Rectangle with the size's X and Z component swapped
    /// </summary>
    /// <returns></returns>
    public Rectangle swapXZ()
    {
        return new Rectangle(position, size.swapXZ());
    }

}
