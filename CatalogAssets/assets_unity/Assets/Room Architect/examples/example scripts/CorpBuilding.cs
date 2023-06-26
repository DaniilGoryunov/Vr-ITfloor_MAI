using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoomArchitectEngine;

public class CorpBuilding : RoomArchitect
{
    int floorAmount = 0;

    public override void buildTemplate()
    {
        setTextures(3, 4, 5);
        reception();
        floorAmount = Random.Range(30, 30);
        for (int i = 1; i < floorAmount; i++)
            floorTemplate(i, (i == (floorAmount - 1) ? true : false));
        addFloorEverywhere();
        autoBuildRoof(ROOFTYPE.FLAT);
    }

    public void Start()
    {
        //Build();
    }

    public void reception()
    {
        Room floor = addRoom(new Position(2, 0, 2), 13, 13);
        int frontGroundLength = 4;
        addFloor(floor.BottomLeft.offsetBy(z: -4), floor.BottomRight);
        setStairStyle(RAILSTYLE.FULL, true);
        addSingleStair(floor.BottomLeft.offsetBy(z: -frontGroundLength - 1).toVector3(RealDimensionsVector),
                       floor.BottomRight.offsetBy(z: -frontGroundLength).toVector3(RealDimensionsVector) - new Vector3(0, elevation, 0),
                       5, Directions.NORTH);
        Rectangle stairRect = new Rectangle(bottomLeft + new Position(0, 0, 9), new Position(4, 1, 2));
        addDoubleStair(stairRect.position,
                       stairRect.TopRight,
                       10, Directions.EAST);
        addWall(stairRect.position, Directions.EAST, 4);
        addWall(stairRect.TopRight.offsetBy(y:-1), Directions.SOUTH, 1);
        addRoom(floor.TopLeft, stairRect.TopRight.offsetBy(y: -1));
        addRoom(floor.TopRight, floor.TopRight.offsetBy(-5, 0, -4));
        addWideEntrance(floor.BottomLeft.offsetBy(6), floor.BottomRight, WindowModels[2]);
        addWideEntrance(floor.TopLeft.offsetBy(4), floor.TopLeft.offsetBy(8), WindowModels[4]);
        addDoor(floor.TopLeft.offsetBy(4), Directions.SOUTH, false, 2);
        addDoor(floor.TopLeft.offsetBy(8), Directions.SOUTH, true, 2);
        addFence(floor.BottomLeft.offsetBy(z: -frontGroundLength), frontGroundLength, Directions.NORTH, 10 * frontGroundLength);
        addFence(floor.BottomRight.offsetBy(z: -frontGroundLength), frontGroundLength, Directions.NORTH, 10 * frontGroundLength);
        addWideEntrance(floor.BottomLeft.offsetBy(3), floor.BottomLeft.offsetBy(5), doorModels[0]);

    }

    public void floorTemplate(int floor,bool last = false)
    {

        Room globalRoom = addRoom(new Position(2, floor, 2), 13, 13);
        setTextures(outdoorTexture:3);
        addRoom(globalRoom.BottomLeft, 3, 4);
        addRoom(globalRoom.BottomLeft.offsetBy(3), 3, 4);
        Room office3 = addRoom(globalRoom.BottomLeft.offsetBy(0, 0, 5), 3, 4);
        addRoom(globalRoom.BottomLeft.offsetBy(3, 0, 5), 3, 4);
        Room staircase = addRoom(globalRoom.TopLeft.offsetBy(z:-2), 8, -2);
        Room elevatorCase = addRoom(globalRoom.TopLeft, staircase.TopRight);
        addRoom(staircase.BottomRight, globalRoom.TopRight);
        setTextures(3, 4, 5);

        addWideEntrance(staircase.BottomRight.offsetBy(-2), staircase.BottomRight, doorModels[0]);
        addWideEntrance(globalRoom.BottomLeft, globalRoom.BottomLeft.offsetBy(3), WindowModels[1]);
        addWideEntrance(globalRoom.BottomLeft.offsetBy(3), globalRoom.BottomLeft.offsetBy(6), WindowModels[1]);
        addWideEntrance(globalRoom.BottomLeft.offsetBy(6), globalRoom.BottomRight, WindowModels[2]);
        addWideEntrance(globalRoom.BottomRight, globalRoom.TopRight.offsetBy(z: -4), WindowModels[3]);
        addWideEntrance(office3.BottomLeft, office3.TopLeft, WindowModels[4]);
        addDoor(staircase.TopLeft.offsetBy(5), Directions.EAST, false, 2);
        addDoor(office3.BottomRight.offsetBy(-3), Directions.EAST, false, 1);
        addDoor(office3.BottomRight.offsetBy(-3, 0, -1), Directions.EAST, false, 1);
        addDoor(office3.BottomRight, Directions.EAST, false, 1);
        addDoor(office3.BottomRight.offsetBy(0, 0, -1), Directions.EAST, false, 1);
        addDoor(office3.BottomLeft, Directions.SOUTH, false, 1);
        addDoor(staircase.TopRight, Directions.SOUTH, false, 2);
        setStairStyle(RAILSTYLE.FULL, false);
        if (!last)
            addDoubleStair(office3.TopLeft,
                           office3.TopLeft.offsetBy(4, 1, 2),
                           10, Directions.EAST);
        if (last)
            addWall(office3.TopLeft.offsetBy(4, 0, 0), Directions.NORTH, 1);
    }
}
