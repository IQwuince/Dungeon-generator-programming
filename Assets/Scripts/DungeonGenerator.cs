using UnityEngine;
using Unity.VisualScripting;

public class DungeonGenerator : MonoBehaviour
{
    RectInt room = new RectInt(0, 0, 100, 50);
    public float duration= 0;
    public bool depthTest = false;
    public float height = 0.0f;

    public AlgorithmsUtils algorithmsUtils;

    private void Update()
    {
        AlgorithmsUtils.DebugRectInt(room, Color.green, duration, depthTest, height);
    }

    
}
