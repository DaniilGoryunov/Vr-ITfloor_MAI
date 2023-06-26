using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoomArchitectEngine;

public class HouseGarage : RoomArchitect
{
    public Room garage;

    public override void buildTemplate()
    {
        if (Random.RandomRange(0, 100) > 50)
            return;
        setTextures(outdoorTexture:4, floorTexture:6);
        garage = addRoom(new Position(2, 0, 4), 4, 7);
        addWideEntrance(garage.BottomLeft, garage.BottomRight, doorModels[0], true);
        removeWall(new Position(6, 0, 4), Directions.NORTH, 7);
        addFloorEverywhere();
        addFloor(garage.BottomLeft, garage.BottomRight.offsetBy(z: -3));
        autoBuildRoof();
    }

    public void Start()
    {
        //Build();
    }
}
