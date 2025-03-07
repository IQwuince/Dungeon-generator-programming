using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> roomList = new List<RectInt>();

    RectInt initalroom = new RectInt(0, 0, 100, 50);

    public float duration= 0;
    public bool depthTest = false;
    public float height = 0.0f;
    public int rooms = 5;
    public bool splitHorizontally;
    public bool makeRoom;
    private IEnumerator coroutine;

    [Header("room size")]
    public float minSizeX = 20;
    public float minSizeY = 20;


    public AlgorithmsUtils algorithmsUtils;

    private void Update()
    {
        foreach (var room in roomList)
        {
            AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
        }
    }
    private void Start()
    {

        if (Random.value > 0.5f)
        {
            splitHorizontally = true;
        }

        roomList.Add(initalroom); // Ensure at least one initial room exists

        for (int i = 0; i < rooms; i++)
        {
            if (roomList.Count > 0) // Ensure there are rooms to split
            {
                CreateRoom();
            }
        }

        //StartCoroutine(CreatingRooms());
    }
    /*private IEnumerator CreatingRooms()
    {
        while (makeRoom == true)
        {
            foreach (var room in roomList)
            {
                AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
            }
            yield return new();
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartCoroutine(CreatingRooms());
            }
            
        }
    }*/
    void CreateRoom()
    {
        int roomIndex = Random.Range(0, roomList.Count);
        RectInt currentRoom = roomList[roomIndex];

        int halfWidth = currentRoom.width / 2;
        int halfLength = currentRoom.height / 2;

        float surfaceRoom = currentRoom.width * currentRoom.height;

        RectInt firstHalf, secondHalf;

        int lineX = currentRoom.xMin + halfWidth;
        int lineY = currentRoom.yMin + halfLength;

        if (splitHorizontally == true && currentRoom.width > minSizeX && currentRoom.height > minSizeY)
        {
            firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, currentRoom.width, halfLength + 1);
            secondHalf = new RectInt(currentRoom.xMin, lineY - 1, currentRoom.width, currentRoom.height - halfLength + 1);

            roomList.RemoveAt(roomIndex);
            roomList.Add(firstHalf);
            roomList.Add(secondHalf);
            
            splitHorizontally = false;
        }
        else if (currentRoom.height > minSizeY && currentRoom.width > minSizeX)
        {
            firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, halfWidth + 1, currentRoom.height);
            secondHalf = new RectInt(lineX - 1, currentRoom.yMin, currentRoom.width - halfWidth + 1, currentRoom.height);

            roomList.RemoveAt(roomIndex);
            roomList.Add(firstHalf);
            roomList.Add(secondHalf);

            splitHorizontally = true;
        }
        

    }
}
