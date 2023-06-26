using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace RoomArchitectEngine
{


    /// <summary>
    /// A 'Mod' object represents the current modification of a subpiece within a wall block
    /// So, a subpiece set to "DOOR" will represent one of the two parts of wallpiece holding a door
    /// This object is internal and should not concern final users
    /// </summary>
    public class Mod
    {
        public readonly static int NONE = 0;
        public readonly static int FULL = 1;
        public readonly static int ONLYTOP = 2;
        public readonly static int DOOR = 3;
        public readonly static int WINDOW = 4;
        public int value;
        public List<Room> ownerRooms;

        public Mod(int v)
        {
            ownerRooms = new List<Room>();
            this.value = v;
        }

        public void addOwner(Room r)
        {
            try
            {
                ownerRooms.Add(r);
            } catch { }
        }

        public static bool operator ==(Mod a, Mod b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            if (a.value == b.value)
                return true;
            return false;
        }

        public static bool operator !=(Mod a, Mod b)
        {
            if (a.value == b.value)
                return false;
            return true;
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

        public static implicit operator Mod(int other)
        {
            return new Mod(other);
        }

        public void Set(Mod other)
        {
            this.value = other.value;
        }

        public void Set(int other)
        {
            this.value = other;
        }

        public bool isEither(params int[] list)
        {
            foreach (int v in list)
            {
                if (value == v)
                    return true;
            }
            return false;
        }

        public bool isNot(params int[] list)
        {
            foreach (int v in list)
            {
                if (value == v)
                    return false;
            }
            return true;
        }


        public override string ToString()
        {
            if (value == Mod.NONE)
                return "NONE";
            if (value == Mod.FULL)
                return "FULL";
            if (value == Mod.ONLYTOP)
                return "ONLY TOP";
            if (value == Mod.DOOR)
                return "DOOR";
            if (value == Mod.WINDOW)
                return "WINDOW";
            return "UNDEFINED";
        }
    }
}
