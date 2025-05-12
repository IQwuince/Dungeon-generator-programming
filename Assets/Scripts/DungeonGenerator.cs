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
    public GameObject wallPrefab;

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

        DrawGraphConnections();

    }
    private void Start()
    {
        // Randomly choosing to split horizontal or vertical
        if (Random.value > 0.5f)
        {
            splitHorizontally = true;
        }

        roomList.Add(initalroom);

        // Create a room for each room in the list
        for (int i = 0; i < rooms; i++)
        {
            if (roomList.Count > 0)
            {
                CreateRoom();
            }
        }
        Rooms();

        // Add perimeter walls for each room in the room list
        foreach (var room in roomList)
        {
            AddPerimeterWalls(room);
        }

        // InitializeGraph();

        BSFSearch();
        // DFSSearch();

        SpawnDungeonAssets();
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
    private void AddPerimeterWalls(RectInt room)
    {
        // Top wall
        wallList.Add(new RectInt(room.xMin, room.yMax - 1, room.width, 1));
        // Bottom wall
        wallList.Add(new RectInt(room.xMin, room.yMin, room.width, 1));
        // Left wall
        wallList.Add(new RectInt(room.xMin, room.yMin, 1, room.height));
        // Right wall
        wallList.Add(new RectInt(room.xMax - 1, room.yMin, 1, room.height));
    }

    //Walls, doors
    void Rooms()
    {
        int doorSize = 2;
        int doorMargin = 2; // Minimum distance from corners

        for (int i = 0; i < roomList.Count; i++)
        {
            RectInt roomA = roomList[i];

            for (int j = i + 1; j < roomList.Count; j++)
            {
                RectInt roomB = roomList[j];

                RectInt sharedWall = AlgorithmsUtils.Intersect(roomA, roomB);

                // Doors
                if (sharedWall.width > 0 && sharedWall.height > 0) // Valid overlap
                {
                    wallList.Add(sharedWall);

                    // Vertical wall (door along Y axis)
                    if (sharedWall.height > sharedWall.width && sharedWall.width >= doorSize && sharedWall.height >= doorSize && (roomA.yMin == roomB.yMin || roomA.xMin == roomB.xMin))
                    {
                        int minY = sharedWall.yMin + doorMargin;
                        int maxY = sharedWall.yMax - doorSize - doorMargin;
                        if (maxY >= minY)
                        {
                            int doorY = Random.Range(minY, maxY + 1);
                            RectInt doorRectY = new RectInt(sharedWall.xMin, doorY, doorSize, doorSize);
                            doorList.Add(doorRectY);

                            graph.AddEdge(roomA, roomB);
                        }
                    }

                    // Horizontal wall (door along X axis)
                    if (sharedWall.width > sharedWall.height && sharedWall.width >= doorSize && sharedWall.height >= doorSize && (roomA.yMin == roomB.yMin || roomA.xMin == roomB.xMin))
                    {
                        int minX = sharedWall.xMin + doorMargin;
                        int maxX = sharedWall.xMax - doorSize - doorMargin;
                        if (maxX >= minX)
                        {
                            int doorX = Random.Range(minX, maxX + 1);
                            RectInt doorRectX = new RectInt(doorX, sharedWall.yMin, doorSize, doorSize);
                            doorList.Add(doorRectX);

                            graph.AddEdge(roomA, roomB);
                        }
                    }
                }
            }
        }
    }
    public void SpawnDungeonAssets()
    {

        foreach (var wallRect in wallList)
        {
            bool hasDoor = false;
            foreach (var doorRect in doorList)
            {
                if (AlgorithmsUtils.Intersects(wallRect, doorRect))
                {
                    hasDoor = true;

                    // Split the wall into two segments, skipping the door area
                    // Determine if the wall is horizontal or vertical
                    if (wallRect.width > wallRect.height)
                    {
                        // Horizontal wall
                        int leftWidth = doorRect.xMin - wallRect.xMin;
                        int rightWidth = wallRect.xMax - doorRect.xMax;

                        if (leftWidth > 0)
                        {
                            RectInt leftWall = new RectInt(wallRect.xMin, wallRect.yMin, leftWidth, wallRect.height);
                            InstantiateWall(leftWall);
                        }
                        if (rightWidth > 0)
                        {
                            RectInt rightWall = new RectInt(doorRect.xMax, wallRect.yMin, rightWidth, wallRect.height);
                            InstantiateWall(rightWall);
                        }
                    }
                    else
                    {
                        // Vertical wall
                        int bottomHeight = doorRect.yMin - wallRect.yMin;
                        int topHeight = wallRect.yMax - doorRect.yMax;

                        if (bottomHeight > 0)
                        {
                            RectInt bottomWall = new RectInt(wallRect.xMin, wallRect.yMin, wallRect.width, bottomHeight);
                            InstantiateWall(bottomWall);
                        }
                        if (topHeight > 0)
                        {
                            RectInt topWall = new RectInt(wallRect.xMin, doorRect.yMax, wallRect.width, topHeight);
                            InstantiateWall(topWall);
                        }
                    }
                    break; // Only one door per wall segment
                }
            }
            if (!hasDoor)
            {
                InstantiateWall(wallRect);
            }
        }
    }
    // Helper method to instantiate a wall segment
    private void InstantiateWall(RectInt wallRect)
    {
        Vector3 wallPosition = new Vector3(
            wallRect.center.x,
            heightWall,
            wallRect.center.y
        );

        GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity, this.transform);
        wall.transform.localScale = new Vector3(wallRect.width, wall.transform.localScale.y, wallRect.height);
    }

    void DrawGraphConnections()
    {
        foreach (var room in graph.GetNodes())
        {
            foreach (var neighbor in graph.GetNeighbors(room))
            {
                // Find the shared door between these two rooms
                RectInt? door = null;
                foreach (var d in doorList)
                {
                    if (AlgorithmsUtils.Intersects(room, d) && AlgorithmsUtils.Intersects(neighbor, d))
                    {
                        door = d;
                        break;
                    }
                }

                if (door.HasValue)
                {
                    Vector3 centerA = new Vector3(room.center.x, height, room.center.y);
                    Vector3 centerB = new Vector3(neighbor.center.x, height, neighbor.center.y);
                    Vector3 doorCenter = new Vector3(door.Value.center.x, heightDoor, door.Value.center.y);

                    // Draw from room A to door, then door to room B
                    Debug.DrawLine(centerA, doorCenter, Color.yellow, duration, depthTest);
                    Debug.DrawLine(doorCenter, centerB, Color.yellow, duration, depthTest);
                }
                else
                {
                    // Fallback: draw direct if no door found
                    Vector3 centerA = new Vector3(room.center.x, height, room.center.y);
                    Vector3 centerB = new Vector3(neighbor.center.x, height, neighbor.center.y);
                    Debug.DrawLine(centerA, centerB, Color.yellow, duration, depthTest);
                }
            }
        }
    }


    void BSFSearch()
    {
        List<RectInt> visitedRooms = graph.BFS(roomList[0]);
        Debug.Log($"Connected: {visitedRooms.Count} / {roomList.Count}");

    }

}
