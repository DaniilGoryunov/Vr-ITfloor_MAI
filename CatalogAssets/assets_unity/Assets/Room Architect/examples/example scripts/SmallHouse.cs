using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoomArchitectEngine;

public class SmallHouse : RoomArchitect
{
    public Room living1, living2, playroom, toilets, bathroom, garage, bedroom2;
    public Roof balconyRoof;
    public GameObject pillar;

    public override void buildTemplate()
    {
        generateChildren = true;
        if (Random.Range(0, 100) > 50)
            generateChildren = false;
        int outdoorTexture = (Random.Range(0, 100) > 50 ? 4 : 7);
        applyMaterialsToChildren();
        setTextures(3, outdoorTexture, 2, 2, 2, 5, 1, 0, outdoorTexture, true);
        living1 = addRoom(new Position(5, 0, 3), 5, 6);
        living2 = addRoom(living1.BottomRight.offsetBy(-2, 0, -2), 5, 5);
        mergeRooms(living1, living2);
        setTextures(outdoorTexture: 3);
        toilets = addRoom(living1.TopRight, new Position(living1.TopRight.x - 1, 0, living2.TopRight.z));
        setTextures(outdoorTexture: outdoorTexture);
        bathroom = addRoom(toilets.BottomRight, new Position(living2.TopRight.x, 0, living1.TopRight.z));

        Room bedroom1 = addRoom(living2.TopRight.offsetBy(0, 0, -2), bathroom.TopRight.offsetBy(4, 0, -1)); //BUGGY

        playroom = addRoom(new Position(living1.BottomLeft.x, 1, living2.BottomLeft.z), living2.TopRight.offsetBy(y: 1));
        bool balconyEast = (Random.Range(0, 100) > 50 ? true : false);
        updstairbedRoom(balconyEast);
        addDoor(living1.BottomLeft.offsetBy(x: 1), Directions.EAST);
        addDoor(toilets.BottomLeft, Directions.EAST);
        addDoor(bathroom.BottomLeft, Directions.EAST);
        addDoor(living2.TopRight, Directions.SOUTH);
        addDoor(living1.TopLeft.offsetBy(1), Directions.EAST);
        addWindow(toilets.TopLeft, Directions.EAST, 0.5f, false, 1);
        addWindow(bathroom.TopLeft, Directions.EAST, 0.5f, false, 1);
        addWindow(bathroom.TopRight.offsetBy(-1), Directions.EAST, 0.5f, false, 1);
        addWindow(living1.TopLeft.offsetBy(2), Directions.EAST);
        addWindow(living2.BottomLeft.offsetBy(1), Directions.EAST);
        addWindow(living2.BottomLeft.offsetBy(3), Directions.EAST);
        addWindow(living2.BottomLeft.offsetBy(1, 1), Directions.EAST);
        addWindow(living2.BottomLeft.offsetBy(3, 1), Directions.EAST);
        addWindow(bedroom1.BottomLeft.offsetBy(1), Directions.EAST);
        addWindow(bedroom1.TopLeft.offsetBy(1), Directions.EAST);
        addWindow(bedroom1.BottomRight.offsetBy(z: 2), Directions.NORTH);
        addWindow(playroom.BottomLeft.offsetBy(1), Directions.EAST);
        addWindow(playroom.BottomLeft.offsetBy(z: 2), Directions.NORTH);
        addWindow(playroom.BottomRight.offsetBy(z: 2), Directions.NORTH);
        addFloorEverywhere();
        setStairStyle(RAILSTYLE.FULL, false);
        addSingleStair(living2.TopRight.offsetBy(-1, 0, -4), living2.TopRight.offsetBy(y: 1, z: -1), 10, Directions.NORTH);
        setStairStyle(RAILSTYLE.FULL, true);
        addSingleStair(living2.BottomLeft.offsetBy(-2, 0, -1).toVector3(RealDimensionsVector) - new Vector3(0, elevation, -0.4f), living2.BottomLeft.offsetBy(-1).toVector3(RealDimensionsVector), 4, Directions.NORTH);
        setStairStyle(RAILSTYLE.NONE, true);
        addSingleStair(living1.TopLeft.offsetBy(1).toVector3(RealDimensionsVector) - new Vector3(0, elevation, 0), living1.TopLeft.offsetBy(2, 0, 1).toVector3(RealDimensionsVector) - new Vector3(0, 0, 0.6f), 4, Directions.SOUTH);
        List<Roof> roofs = mapRoofs();
        setRoofStyle(ROOFTYPE.DEFAULT);
        for (int i = 0; i < roofs.Count; i++)
        {
            if (roofs[i].position.y == 1 && (roofs[i].BottomRight == bedroom2.BottomLeft || roofs[i].position == bedroom2.BottomRight))
                balconyRoof = roofs[i];
            else
            {
                buildRoof(roofs[i]);
                setTextures(roofTexture: 5);
            }
        }
        balcony(balconyEast);
        addFence(new Position(living1.BottomLeft.x, 0, living2.BottomLeft.z), 2, Directions.NORTH, 20);
        addFence(new Position(living1.BottomLeft.x, 0, living2.BottomLeft.z), 1, Directions.EAST, 10);
        addFence(new Position(living2.BottomLeft.x, 0, living2.BottomLeft.z), 1, Directions.WEST, 10);
        addFence(living2.TopRight.offsetBy(-1, 1, -4), 1, Directions.EAST, 10);
        addFence(living2.TopRight.offsetBy(-1, 1, -4), 3, Directions.NORTH, 30);

        addFloor(new Position(living1.BottomLeft.x, 0, living2.BottomLeft.z),
                    new Position(living2.BottomLeft.x, 0, living1.BottomLeft.z));
        GameObject pillarInstance = GameObject.Instantiate(pillar);
        pillarInstance.transform.SetParent(objectHolder.transform, false);
        pillarInstance.transform.localPosition = playroom.BottomLeft.toVector3(RealDimensionsVector) + new Vector3(0.2f, -VerticalScale, 0.2f);
        pillarInstance.gameObject.SetActive(true);
    }

    public void balcony(bool balconyEast)
    {
        setTextures(roofTexture: 6);
        setRoofStyle(ROOFTYPE.FLAT);
        buildRoof(balconyRoof);
        if (balconyEast)
            addFence(balconyRoof.BottomRight, balconyRoof.size.z, Directions.NORTH, balconyRoof.size.z * 10);
        else
            addFence(balconyRoof.BottomLeft, balconyRoof.size.z, Directions.NORTH, balconyRoof.size.z * 10);
        addFence(balconyRoof.TopLeft, balconyRoof.size.x, Directions.EAST, balconyRoof.size.z * 10);
        addWideEntrance(balconyRoof.BottomLeft.offsetBy(1), balconyRoof.BottomLeft.offsetBy(3), doorModels[1]);

    }

    public void updstairbedRoom(bool balconyEast)
    {
        if (balconyEast)
            bedroom2 = addRoom(living1.TopLeft.upOneFloor(), toilets.BottomLeft.upOneFloor());
        else
            bedroom2 = addRoom(toilets.BottomLeft.upOneFloor(), bathroom.TopRight.upOneFloor());
        addWindow(bedroom2.TopLeft.offsetBy(1), Directions.EAST);
        addWindow(bedroom2.BottomLeft.offsetBy(z: 1), Directions.NORTH);
        addWindow(bedroom2.BottomRight.offsetBy(z: 1), Directions.NORTH);
        addDoor(bedroom2.BottomLeft, Directions.EAST);
    }

    public void Start()
    {
        //Build();
    }
}
