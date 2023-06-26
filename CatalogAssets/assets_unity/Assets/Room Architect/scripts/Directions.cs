using UnityEngine;
using System.Collections;


/// <summary>
/// This objects represents a cardinal direction
/// Options are NORTH, SOUTH, EAST, WEST, NONE, and CENTER
/// NONE -> neither directions
/// CENTER -> Used to define the center poll of a wall block
/// </summary>
public struct Directions
{
    public static readonly int EAST = 0;
    public static readonly int NORTH = 1;
    public static readonly int WEST = 2;
    public static readonly int SOUTH = 3;
    public static readonly int UP = 4;
    public static readonly int CENTER = 5;
    public static readonly int NONE = 6;
    public static readonly int length = 7;
    public int value;

    public static Directions oppositeOf(Directions d)
    {
        if (d == Directions.EAST)
            return Directions.WEST;
        else if (d == Directions.WEST)
            return Directions.EAST;
        else if (d == Directions.NORTH)
            return Directions.SOUTH;
        else if (d == Directions.SOUTH)
            return Directions.NORTH;
        else
            return Directions.NONE;
    }

    public Directions(int v)
    {
        this.value = v;
    }


    public static bool operator ==(Directions a, Directions b)
    {
        if (a.value == b.value)
            return true;
        return false;
    }

    public static bool operator !=(Directions a, Directions b)
    {
        if (a.value != b.value)
            return true;
        return false;
    }

    public override bool Equals(object obj)
    {
        Directions pobj = (Directions)obj;
        if (pobj.value == this.value)
            return true;
        return false;
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public static implicit operator Directions(int other)
    {
        return new Directions(other);
    }

    public static implicit operator int(Directions other)
    {
        return other.value;
    }

    public bool isEither(params Directions[] directions)
    {
        foreach(Directions d in directions)
        {
            if (this.value == d.value)
                return true;
        }
        return false;
    }

    public Directions opposite()
    {
        if (value == NORTH)
            return SOUTH;
        else if (value == SOUTH)
            return NORTH;
        else if (value == WEST)
            return EAST;
        else if (value == EAST)
            return WEST;
        return NONE;
    }

    public override string ToString()
    {
        string ret = "";
        ret += (value == EAST ? "EAST" : "");
        ret += (value == NORTH ? "NORTH" : "");
        ret += (value == WEST ? "WEST" : "");
        ret += (value == SOUTH ? "SOUTH" : "");
        ret += (value == UP ? "UP" : "");
        ret += (value == CENTER ? "CENTER" : "");
        ret += (value == NONE ? "NONE" : "");
        return ret;
    }

    public Directions nextCounterClockWise()
    {
        if (value == NORTH)
            return WEST;
        else if (value == WEST)
            return SOUTH;
        else if (value == SOUTH)
            return EAST;
        else
            return NORTH;
    }

    public Directions nextClockWise()
    {
        if (value == NORTH)
            return EAST;
        else if (value == EAST)
            return SOUTH;
        else if (value == SOUTH)
            return WEST;
        else
            return NORTH;
    }

}

