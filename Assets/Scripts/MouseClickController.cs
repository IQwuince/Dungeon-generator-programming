using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseClickController : MonoBehaviour
{
    public Vector3 clickPosition;
    public List<Vector3> clickPositions = new();
    public PlayerController playerController;

    [Header("Settings")]
    public bool showDebug = true;

    // Define the UnityEvent to notify other scripts about the click
    public UnityEvent<Vector3> OnClickEvent = new();
    //public UnityEvent destination;

    private Ray lastRay;
    private bool hasValidClick = false;

    void Update()
    {
        // Get the mouse click position in world space 
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
            {
                Vector3 clickWorldPosition = hitInfo.point;
                Debug.Log(clickWorldPosition);

                // Store the click position here
                clickPosition = clickWorldPosition; // Store last valid click
                clickPositions.Add(clickWorldPosition);
                lastRay = mouseRay;
                hasValidClick = true;

                // Trigger the UnityEvent to notify other scripts about the click
                OnClickEvent.Invoke(clickWorldPosition);
                playerController.GoToDestination(clickWorldPosition);
            }
        }
        if (showDebug == true)
        {
            // Visual debug for all positions (optional, can be removed if only last is needed)
            foreach (var position in clickPositions)
            {
                Debug.DrawRay(position, Vector3.up, Color.blue, 1.0f);
            }

            // Visual debug for the last valid ray and position
            if (hasValidClick)
            {
                Debug.DrawRay(lastRay.origin, lastRay.direction * 100f, Color.green); // Draw the last ray
                DebugExtension.DebugWireSphere(clickPosition, Color.green, 0.5f); // Draw a wire sphere at the last position
            }
        }
        
    }
}
