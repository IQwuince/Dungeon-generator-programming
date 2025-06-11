using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEditor.Overlays;
using Unity.AI.Navigation;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Lists")]
    public List<RectInt> roomList = new();
    public List<RectInt> doorList = new();
    public List<RectInt> wallList = new();

    Graph<RectInt> graph = new Graph<RectInt>();


    RectInt initalroom = new RectInt(0, 0, 100, 50);

    [Header("Assets")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public NavMeshSurface navMeshSurface;

    [Header("Settings")]
    public float duration = 0;
    public bool depthTest = false;
    public float height = 0.0f;
    public float heightDoor = 0f;
    public float heightWall = 2f;
    public int rooms = 5;
    public bool splitHorizontally;
    public bool showDebug = true;

    [Header("room size")]
    public float minSizeX = 20;
    public float minSizeY = 20;
    public float minDoorSize = 4;


    public AlgorithmsUtils algorithmsUtils;



    private void Update()
    {   //Show or hide debug lines for rooms, doors and walls
        if ( showDebug == true)
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

        BSFSearch();

        SpawnDungeonAssets();
    }
    //Creating the rooms
    void CreateRoom()
    {
        // Select a random room from the list to split
        int roomIndex = Random.Range(0, roomList.Count);
        RectInt currentRoom = roomList[roomIndex];

        // Determine random split positions within the allowed size constraints
        int halfWidth = (int)Random.Range(minSizeX, currentRoom.width - minSizeX);
        int halfLength = (int)Random.Range(minSizeY, currentRoom.height - minSizeY);

        RectInt firstHalf, secondHalf;

        int lineX = currentRoom.xMin + halfWidth;
        int lineY = currentRoom.yMin + halfLength;

        // Split horizontally if flag is set and room is large enough
        if (splitHorizontally == true && currentRoom.width > minSizeX && halfLength > minSizeY)
        {
            // Create two new rooms by splitting along the Y axis
            firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, currentRoom.width, halfLength + 1);
            secondHalf = new RectInt(currentRoom.xMin, lineY - 1, currentRoom.width, currentRoom.height - halfLength + 1);

            // Replace the original room with the two new rooms
            roomList.RemoveAt(roomIndex);
            roomList.Add(firstHalf);
            roomList.Add(secondHalf);

            // Alternate split direction for next room
            splitHorizontally = false;
        }
        // Otherwise, split vertically if possible
        else if (currentRoom.height > minSizeY && halfWidth > minSizeX)
        {
            // Create two new rooms by splitting along the X axis
            firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, halfWidth + 1, currentRoom.height);
            secondHalf = new RectInt(lineX - 1, currentRoom.yMin, currentRoom.width - halfWidth + 1, currentRoom.height);

            // Replace the original room with the two new rooms
            roomList.RemoveAt(roomIndex);
            roomList.Add(firstHalf);
            roomList.Add(secondHalf);

            // Alternate split direction for next room
            splitHorizontally = true;
        }
    }

    //Walls, doors
    void Rooms()
    {
        int doorSize = 2;      // The width/height of each door (doors are square)
        int doorMargin = 2;    // Minimum distance from the edge of a wall to a door (prevents doors at corners)

        for (int i = 0; i < roomList.Count; i++)
        {
            RectInt roomA = roomList[i];

            for (int j = i + 1; j < roomList.Count; j++)
            {
                RectInt roomB = roomList[j];

                // Find the shared wall between the two rooms
                RectInt sharedWall = AlgorithmsUtils.Intersect(roomA, roomB);

                if (sharedWall.width > 0 && sharedWall.height > 0)
                {
                    wallList.Add(sharedWall); // Store the shared wall for debug/visualization

                    // If the shared wall is vertical (taller than it is wide)
                    if (sharedWall.height > sharedWall.width && sharedWall.width >= doorSize && sharedWall.height >= doorSize)
                    {
                        // Calculate the valid Y range for placing a door, avoiding corners
                        int minY = sharedWall.yMin + doorMargin;
                        int maxY = sharedWall.yMax - doorSize - doorMargin;
                        if (maxY >= minY)
                        {
                            // Randomly select a Y position for the door within the valid range
                            int doorY = Random.Range(minY, maxY + 1);
                            RectInt doorRectY = new RectInt(sharedWall.xMin, doorY, doorSize, doorSize);
                            doorList.Add(doorRectY); // Store the door

                            // Add an edge in the graph to make sure these rooms are connected
                            graph.AddEdge(roomA, roomB);
                        }
                    }

                    // If the shared wall is horizontal (wider than it is tall)
                    if (sharedWall.width > sharedWall.height && sharedWall.width >= doorSize && sharedWall.height >= doorSize)
                    {
                        // Calculate the valid X range for placing a door, avoiding corners
                        int minX = sharedWall.xMin + doorMargin;
                        int maxX = sharedWall.xMax - doorSize - doorMargin;
                        if (maxX >= minX)
                        {
                            // Randomly select an X position for the door within the valid range
                            int doorX = Random.Range(minX, maxX + 1);
                            RectInt doorRectX = new RectInt(doorX, sharedWall.yMin, doorSize, doorSize);
                            doorList.Add(doorRectX); // Store the door

                            // Add an edge in the graph to make sure these rooms are connected
                            graph.AddEdge(roomA, roomB);
                        }
                    }
                }
            }
        }
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

                Vector3 centerA = new Vector3(room.center.x, height, room.center.y);
                Vector3 centerB = new Vector3(neighbor.center.x, height, neighbor.center.y);

                if (door.HasValue)
                {
                    Vector3 doorCenter = new Vector3(door.Value.center.x, heightDoor, door.Value.center.y);

                    // Draw from room A to door, then door to room B
                    Debug.DrawLine(centerA, doorCenter, Color.yellow, duration, depthTest);
                    Debug.DrawLine(doorCenter, centerB, Color.yellow, duration, depthTest);
                }
                else
                {
                    // Fallback: draw direct if no door found
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
    public void SpawnDungeonAssets()
    {
        // Find or create the "Rooms" parent object
        GameObject roomsParent = GameObject.Find("Rooms");
        if (roomsParent == null)
            roomsParent = new GameObject("Rooms");
        roomsParent.transform.SetParent(transform);

        for (int i = 0; i < roomList.Count; i++)
        {
            var room = roomList[i];
            GameObject roomGO = new GameObject($"Room_{i}");
            roomGO.transform.SetParent(roomsParent.transform);

            // --- Spawn Floor ---
            if (floorPrefab != null)
            {
                Vector3 floorPos = new Vector3(
                    room.center.x,
                    0, // Place at y=0, adjust if needed
                    room.center.y
                );
                GameObject floor = Instantiate(floorPrefab, floorPos, Quaternion.Euler(90, 0, 0), roomGO.transform);
                floor.transform.localScale = new Vector3(room.width, room.height, 1);
            }

            // Top and bottom walls
            for (int x = room.xMin; x < room.xMax; x++)
            {
                Vector2Int topPosInt = new Vector2Int(x, room.yMin);
                Vector2Int bottomPosInt = new Vector2Int(x, room.yMax - 1);

                Vector3 topPos = new Vector3(x + 0.5f, heightWall, room.yMin + 0.5f);
                Vector3 bottomPos = new Vector3(x + 0.5f, heightWall, room.yMax - 1 + 0.5f);

                bool topIsDoor = doorList.Exists(d => d.Contains(topPosInt));
                bool bottomIsDoor = doorList.Exists(d => d.Contains(bottomPosInt));

                if (!topIsDoor)
                    Instantiate(wallPrefab, topPos, Quaternion.identity, roomGO.transform);
                if (!bottomIsDoor)
                    Instantiate(wallPrefab, bottomPos, Quaternion.identity, roomGO.transform);
            }

            // Left and right walls (excluding corners to avoid double instantiation)
            for (int y = room.yMin + 1; y < room.yMax - 1; y++)
            {
                Vector2Int leftPosInt = new Vector2Int(room.xMin, y);
                Vector2Int rightPosInt = new Vector2Int(room.xMax - 1, y);

                Vector3 leftPos = new Vector3(room.xMin + 0.5f, heightWall, y + 0.5f);
                Vector3 rightPos = new Vector3(room.xMax - 1 + 0.5f, heightWall, y + 0.5f);

                bool leftIsDoor = doorList.Exists(d => d.Contains(leftPosInt));
                bool rightIsDoor = doorList.Exists(d => d.Contains(rightPosInt));

                if (!leftIsDoor)
                    Instantiate(wallPrefab, leftPos, Quaternion.identity, roomGO.transform);
                if (!rightIsDoor)
                    Instantiate(wallPrefab, rightPos, Quaternion.identity, roomGO.transform);
            }
        }

        //make the NavMesh after all geometry is spawned 
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

}
