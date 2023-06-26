using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoomArchitectEngine;

public class Dungeon : RoomArchitect
{
    List<Room> DungeonRooms;
    public int length = 10;
    public int splitLimit = 10;

    public override void buildTemplate()
    {
        DungeonRooms = new List<Room>();
        Room lastRoom = null;
        lastRoom = newDungeonRoomNorth(lastRoom);
        DungeonRooms.Add(lastRoom);
        buildBranch(lastRoom, 0);
        addFloorEverywhere();
        //autoBuildRoof(ROOFTYPE.DEFAULT);
    }

    public void buildBranch(Room lastRoom, int splitCount)
    {
        if (splitCount > splitLimit)
            return;
        for (int i = 0; i < length; i++)
        {
            try
            {
                int rDirection = Random.Range(0, 100);
                if (Random.Range(0, 100) <= 5)
                    lastRoom = newDungeonRoomDown(lastRoom);
                if (rDirection < 25)
                    lastRoom = newDungeonRoomNorth(lastRoom);
                else if (rDirection < 50)
                    lastRoom = newDungeonRoomSouth(lastRoom);
                else if (rDirection < 75)
                    lastRoom = newDungeonRoomEast(lastRoom);
                else
                    lastRoom = newDungeonRoomWest(lastRoom);
            }
            catch (System.Exception e)
            {
                Debug.Log("Reaching lower limit: " + e.Message);
            }

        }
        buildBranch(lastRoom, ++splitCount);
        buildBranch(lastRoom, ++splitCount);
    }

    public Room newDungeonRoomDown(Room lastRoom)
    {
        Room newRoom = new Room(lastRoom.BottomLeft.offsetBy(y:-1), lastRoom.TopRight.offsetBy(y: -1));
        foreach (Room r in DungeonRooms)
        {
            if (newRoom.Overlaps(r))
                return lastRoom;
        }
        newRoom = addRoom(newRoom);
        DungeonRooms.Add(newRoom);
        addSingleStair(newRoom.BottomRight.offsetBy(x:-2, z: 1), lastRoom.TopRight.offsetBy(x: -4, z: -1), 10, Directions.NORTH);
        return newRoom;
    }

    public Room newDungeonRoomNorth(Room lastRoom)
    {
        Position entrance;
        if (lastRoom == null)
            entrance = entrance = new Position(100, 100, 100);
        else
            entrance = lastRoom.TopRight - new Position(lastRoom.Size.x / 2);
        Position size = new Position(Random.Range(5, 9), 0, Random.Range(5, 9));
        Position tmpBottomLeft = new Position(entrance.x - size.x / 2, entrance.y, entrance.z);
        Position tmpTopRight = tmpBottomLeft.offsetBy(size);
        Room newRoom = new Room(tmpBottomLeft, tmpTopRight);
        foreach (Room r in DungeonRooms)
        {
            if (newRoom.Overlaps(r))
                return lastRoom;
        }
        newRoom = addRoom(newRoom);
        /* 
         * NOTE: Room newRoom = new Room() does not produce a valid object to pass to mergeRooms(),
         * the method takes Room objects that have been initialized and returned by addRoom();
         */
        DungeonRooms.Add(newRoom);
        if (lastRoom != null)
            mergeRooms(lastRoom, newRoom);
        return newRoom;
    }

    public Room newDungeonRoomSouth(Room lastRoom)
    {
        Position entrance = lastRoom.BottomRight - new Position(lastRoom.Size.x / 2);
        Position size = new Position(Random.Range(5, 9), 0, Random.Range(5, 9));
        Position tmpTopRight = new Position(entrance.x + size.x / 2, entrance.y, entrance.z);
        Position tmpBottomLeft = tmpTopRight.offsetBy(-size);
        Room newRoom = new Room(tmpBottomLeft, tmpTopRight);
        foreach (Room r in DungeonRooms)
        {
            if (newRoom.Overlaps(r))
                return lastRoom;
        }
        newRoom = addRoom(newRoom);
        mergeRooms(lastRoom, newRoom);
        DungeonRooms.Add(newRoom);
        return newRoom;
    }

    public Room newDungeonRoomEast(Room lastRoom)
    {
        Position entrance = lastRoom.BottomRight + new Position(z: lastRoom.Size.z / 2);
        Position size = new Position(Random.Range(5, 9), 0, Random.Range(5, 9));
        Position tmpBottomLeft = new Position(entrance.x, entrance.y, entrance.z - size.z / 2);
        Position tmpTopRight = tmpBottomLeft.offsetBy(size);
        Room newRoom = new Room(tmpBottomLeft, tmpTopRight);
        foreach (Room r in DungeonRooms)
        {
            if (newRoom.Overlaps(r))
                return lastRoom;
        }
        newRoom = addRoom(newRoom);
        mergeRooms(lastRoom, newRoom);
        DungeonRooms.Add(newRoom);
        return newRoom;
    }

    public Room newDungeonRoomWest(Room lastRoom)
    {
        Position entrance = lastRoom.BottomLeft + new Position(z: lastRoom.Size.z / 2);
        Position size = new Position(Random.Range(5, 9), 0, Random.Range(5, 9));
        Position tmpTopRight = new Position(entrance.x, entrance.y, entrance.z + size.z / 2);
        Position tmpBottomLeft = tmpTopRight.offsetBy(-size);
        Room newRoom = new Room(tmpBottomLeft, tmpTopRight);
        foreach (Room r in DungeonRooms)
        {
            if (newRoom.Overlaps(r))
                return lastRoom;
        }
        newRoom = addRoom(newRoom);
        mergeRooms(lastRoom, newRoom);
        DungeonRooms.Add(newRoom);
        return newRoom;
    }

}
