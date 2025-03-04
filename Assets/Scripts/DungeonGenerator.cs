using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public List<RectInt> roomList = new List<RectInt>();

    RectInt initalroom = new RectInt(0, 0, 100, 50);

    public float duration= 0;
    public bool depthTest = false;
    public float height = 0.0f;
    public int rooms = 5;

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
        roomList.Add(initalroom); // Ensure at least one initial room exists

        for (int i = 0; i < rooms; i++)
        {
            if (roomList.Count > 0) // Ensure there are rooms to split
            {
                CreateRoom();
            }
        }
    }

    void CreateRoom()
    {
        int roomIndex = Random.Range(0, roomList.Count);
        RectInt currentRoom = roomList[roomIndex];

        int halfWidth = currentRoom.width / 2;

        RectInt firstHalf, secondHalf;

        int lineX = currentRoom.xMin + halfWidth;
        firstHalf = new RectInt(currentRoom.xMin, currentRoom.yMin, halfWidth + 1, currentRoom.height);
        secondHalf = new RectInt(lineX - 1, currentRoom.yMin, currentRoom.width - halfWidth + 1, currentRoom.height);

        roomList.RemoveAt(roomIndex);
        roomList.Add(firstHalf);
        roomList.Add(secondHalf);

    }
}
