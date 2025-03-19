using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEditor.Overlays;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> roomList = new();
    public List<RectInt> wallList = new();
    public List<RectInt> doorList = new();

    RectInt initalroom = new RectInt(0, 0, 100, 50);

    public float duration= 0;
    public bool depthTest = false;
    public float height = 0.0f;
    public float heightDoor = 5f;
    public int rooms = 5;
    public bool splitHorizontally;
    public bool makeRoom;

    [Header("room size")]
    public float minSizeX = 20;
    public float minSizeY = 20;
    public float minDoorSize = 4;


    public AlgorithmsUtils algorithmsUtils;

    private void Update()
    {
        foreach (var room in roomList)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, 0, depthTest, height);
        }

        foreach (var door in doorList)
        {
            AlgorithmsUtils.DebugRectInt(door, Color.red, duration, depthTest, height);
        }

    }
    private void Start()
    {

        if (Random.value > 0.5f)
        {
            splitHorizontally = true;
        }

        roomList.Add(initalroom);

        for (int i = 0; i < rooms; i++)
        {
            if (roomList.Count > 0) 
            {
                CreateRoom();
            }
        }
        FindAdjacentRooms();
        PrintRoomPositions();
    }
    void CreateRoom()
    {
        int roomIndex = Random.Range(0, roomList.Count);
        RectInt currentRoom = roomList[roomIndex];

       // int halfWidth = currentRoom.width / 2;
        //int halfLength = currentRoom.height / 2;

        int halfWidth = (int)Random.Range(minSizeX, currentRoom.width - minSizeX);
        int halfLength = (int)Random.Range(minSizeY, currentRoom.height - minSizeY);

        RectInt firstHalf, secondHalf;

        int lineX = currentRoom.xMin + halfWidth;
        int lineY = currentRoom.yMin + halfLength;

        if (splitHorizontally == true && currentRoom.width > minSizeX && halfLength > minSizeY)
        {
            firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, currentRoom.width, halfLength + 1);
            secondHalf = new RectInt(currentRoom.xMin, lineY - 1, currentRoom.width, currentRoom.height - halfLength + 1);

            roomList.RemoveAt(roomIndex);
            roomList.Add(firstHalf);
            roomList.Add(secondHalf);
            
            splitHorizontally = false;
        }
        else if (currentRoom.height > minSizeY && halfWidth > minSizeX)
        {
            firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, halfWidth + 1, currentRoom.height);
            secondHalf = new RectInt(lineX - 1, currentRoom.yMin, currentRoom.width - halfWidth + 1, currentRoom.height);

            roomList.RemoveAt(roomIndex);
            roomList.Add(firstHalf);
            roomList.Add(secondHalf);

            splitHorizontally = true;
        }

    }
    void FindAdjacentRooms()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RectInt roomA = roomList[i];

            for (int j = i + 1; j < roomList.Count; j++)
            {
                RectInt roomB = roomList[j];

                RectInt sharedWall = AlgorithmsUtils.Intersect(roomA, roomB);


                if (sharedWall.width > 0 && sharedWall.height > 0) // Valid overlap
                {
                    Debug.Log($"Shared Wall between Room {i} and Room {j}: X[{sharedWall.xMin}, {sharedWall.xMax}] Y[{sharedWall.yMin}, {sharedWall.yMax}]");
                    wallList.Add(sharedWall);
                    //AlgorithmsUtils.DebugRectInt(sharedWall, Color.red, 100f, depthTest, heightDoor); // Draw the shared wall in red

                    if (sharedWall.height > sharedWall.width && sharedWall.width >= 2 && sharedWall.height >= 2 && roomA.yMin == roomB.yMin || roomA.xMin == roomB.xMin)
                    {

                        RectInt doorRectY = new RectInt(sharedWall.xMin,sharedWall.yMin + sharedWall.height /2, 2, 2);

                        AlgorithmsUtils.DebugRectInt(doorRectY, Color.blue, duration, depthTest, heightDoor);
                    }

                    if (sharedWall.width > sharedWall.height && sharedWall.width >= 2 && sharedWall.height >= 2 && roomA.yMin == roomB.yMin || roomA.xMin == roomB.xMin)
                    {
                        RectInt doorRectX = new RectInt(sharedWall.xMin +sharedWall.width /2, sharedWall.yMin, 2, 2);

                       AlgorithmsUtils.DebugRectInt(doorRectX, Color.blue, duration, depthTest, heightDoor);
                    }
                }
            }
        }
    }

    void PrintRoomPositions()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RectInt room = roomList[i];
           // Debug.Log($"Room {i}: X[{room.xMin}, {room.xMax}] Y[{room.yMin}, {room.yMax}]");
        }
    }

    void CreateDoors()
    {

    }

}
