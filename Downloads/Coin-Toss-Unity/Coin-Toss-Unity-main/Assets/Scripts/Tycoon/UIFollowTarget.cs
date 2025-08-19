using UnityEngine;

public class UIFollowTarget : MonoBehaviour
{
    // The target in the game world that this UI element will follow.
    public Transform targetToFollow;

    // An offset to position the UI (e.g., slightly above the target's head).
    public Vector3 offset = new Vector3(0, 40, 0);

    private RectTransform rectTransform;
    private Camera mainCamera;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    // LateUpdate is used to prevent the UI from jittering.
    // It updates the position after the target has finished moving for the frame.
    void LateUpdate()
    {
        if (targetToFollow == null)
        {
            // If the target has been destroyed, destroy this UI element as well.
            Destroy(gameObject);
            return;
        }

        // Convert the target's world position to a screen position.
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(targetToFollow.position);

        // Set this UI element's position to the converted screen point plus the offset.
        rectTransform.position = screenPoint + (Vector2)offset;
    }
}
