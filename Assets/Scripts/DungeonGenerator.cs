using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEditor.Overlays;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> roomList = new();
    public List<RectInt> doorList = new();
    public List<RectInt> wallList = new();

    Graph<RectInt> graph = new Graph<RectInt>();
    Dictionary<RectInt, List<RectInt>> roomGraph = new Dictionary<RectInt, List<RectInt>>();


    RectInt initalroom = new RectInt(0, 0, 100, 50);

    public float duration = 0;
    public bool depthTest = false;
    public float height = 0.0f;
    public float heightDoor = 0f;
    public float heightWall = 2f;
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
            AlgorithmsUtils.DebugRectInt(door, Color.blue, duration, depthTest, heightDoor);
        }

        foreach (var sharedWall in wallList)
        {
            AlgorithmsUtils.DebugRectInt(sharedWall, Color.red, 100f, depthTest, heightWall);
        }

    }
    private void Start()
    {
        //Randomly choosing to split horizontal or vertical
        if (Random.value > 0.5f)
        {
            splitHorizontally = true;
        }

        roomList.Add(initalroom);

        //Create a room for each room in the list
        for (int i = 0; i < rooms; i++)
        {
            if (roomList.Count > 0)
            {
                CreateRoom();
            }
        }
        Rooms();

        //InitializeGraph();

        BSFSearch();
        //DFSSearch();


    }
    //Creating the rooms
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

    //Walls, doors
    void Rooms()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RectInt roomA = roomList[i];

            for (int j = i + 1; j < roomList.Count; j++)
            {
                RectInt roomB = roomList[j];

                RectInt sharedWall = AlgorithmsUtils.Intersect(roomA, roomB);


                //Doors
                if (sharedWall.width > 0 && sharedWall.height > 0) // Valid overlap
                {
                    //Debug.Log($"Shared Wall between Room {i} and Room {j}: X[{sharedWall.xMin}, {sharedWall.xMax}] Y[{sharedWall.yMin}, {sharedWall.yMax}]");
                    wallList.Add(sharedWall);

                    // Ensure we don't place doors on corners
                    if (sharedWall.height > sharedWall.width && sharedWall.width >= 2 && sharedWall.height >= 2 && (roomA.yMin == roomB.yMin || roomA.xMin == roomB.xMin))
                    {
                        RectInt doorRectY = new RectInt(sharedWall.xMin, sharedWall.yMin + sharedWall.height / 2, 2, 2);
                        doorList.Add(doorRectY);

                        graph.AddEdge(roomA, roomB);
                    }

                    if (sharedWall.width > sharedWall.height && sharedWall.width >= 2 && sharedWall.height >= 2 && (roomA.yMin == roomB.yMin || roomA.xMin == roomB.xMin))
                    {
                        RectInt doorRectX = new RectInt(sharedWall.xMin + sharedWall.width / 2, sharedWall.yMin, 2, 2);
                        doorList.Add(doorRectX);

                        graph.AddEdge(roomA, roomB);
                    }
                }



            }
        }
    }


    void BSFSearch()
    {
        graph.BFS(roomList[0]);
    }

}
