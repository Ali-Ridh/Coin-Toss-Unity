using UnityEngine;

public class BoundaryCreator : MonoBehaviour
{
    // You can adjust the thickness of the boundary walls in the Inspector.
    public float wallThickness = 1f;
    // You can adjust the bounciness of the walls. 1 means perfect bounce.
    [Range(0, 1)]
    public float wallBounciness = 0.8f;

    void Start()
    {
        // Get the main camera to determine the screen size.
        Camera mainCamera = Camera.main;

        // Create a PhysicsMaterial2D for the walls to control bounciness.
        PhysicsMaterial2D wallMaterial = new PhysicsMaterial2D();
        wallMaterial.bounciness = wallBounciness;
        wallMaterial.friction = 0.4f; // Default friction

        // Calculate the screen boundaries in world coordinates.
        // This finds the top-right corner of the screen in the game world.
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        float screenWidth = screenBounds.x;
        float screenHeight = screenBounds.y;

        // Create the four boundary walls.
        CreateBoundaryWall("TopWall", new Vector2(0, screenHeight + (wallThickness / 2)), new Vector2(screenWidth * 2, wallThickness), wallMaterial);
        CreateBoundaryWall("BottomWall", new Vector2(0, -screenHeight - (wallThickness / 2)), new Vector2(screenWidth * 2, wallThickness), wallMaterial);
        CreateBoundaryWall("LeftWall", new Vector2(-screenWidth - (wallThickness / 2), 0), new Vector2(wallThickness, screenHeight * 2), wallMaterial);
        CreateBoundaryWall("RightWall", new Vector2(screenWidth + (wallThickness / 2), 0), new Vector2(wallThickness, screenHeight * 2), wallMaterial);
    }

    // A helper function to create each wall to avoid repeating code.
    void CreateBoundaryWall(string name, Vector2 position, Vector2 size, PhysicsMaterial2D material)
    {
        // Create a new empty GameObject for the wall.
        GameObject wall = new GameObject(name);

        // Set its position in the world.
        wall.transform.position = position;

        // Make the wall a child of this BoundaryCreator object for organization.
        wall.transform.parent = this.transform;

        // Add a BoxCollider2D to give it physical shape.
        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();

        // Set the size of the collider.
        collider.size = size;

        // Assign the physics material for bounciness.
        collider.sharedMaterial = material;
    }
}
