using UnityEngine;

// Decouples Unity-specific functionality (Start, FixedUpdate, Inspector) from our C# physics engine.
public class Game : MonoBehaviour
{
    PhysicsWorld world = new PhysicsWorld();
    public GameObject spherePrefab;
    public GameObject planePrefab;

    void Start()
    {
        // Define world settings
        world.step = Time.fixedDeltaTime;
        world.gravity = Physics.gravity;

        // Instantiate spheres and add them to our physics world
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body sphere = world.Add(spherePrefab, new Vector3(angle, 5.0f, 0.0f), Quaternion.identity);
            sphere.shape = new Sphere { type = ShapeType.SPHERE, radius = 0.5f };
        }

        // Instatiate the ground plane and add it to our physics world
        {
            Body plane = world.Add(planePrefab, Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 0.0f));
            plane.shape = new Plane { type = ShapeType.PLANE };
            plane.SetInfiniteMass();
        }
    }

    // World update frequency must match its time-step otherwise things will explode
    void FixedUpdate()
    {
        world.Simulate();
    }
}
