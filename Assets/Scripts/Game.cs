using UnityEngine;

public class Game : MonoBehaviour
{
    PhysicsWorld world = new PhysicsWorld();
    public GameObject spherePrefab;
    public GameObject planePrefab;

    void Start()
    {
        // Right sphere that will bounce off left sphere then slowly comes to rest
        {
            GameObject sphere = Instantiate(spherePrefab);
            Particle sphereData = new Particle
            {
                pos = new Vector3(-1.0f, 5.0f, 0.0f),
                vel = new Vector3(-1.0f, 0.0f, 0.0f),
                acc = Vector3.zero,

                mass = 1.0f,
                gravityScale = 1.0f,

                collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true },

                friction = 0.01f,
                restitution = 0.75f
            };
            world.Add(sphere, sphereData);
        }

        // Left sphere moving right that will quickly stop bouncing and come to rest
        {
            GameObject sphere = Instantiate(spherePrefab);
            Particle sphereData = new Particle
            {
                pos = new Vector3(-3.0f, 0.5f, 0.0f),
                vel = new Vector3( 3.0f, 0.0f, 0.0f),
                acc = Vector3.zero,

                mass = 1.0f,
                gravityScale = 1.0f,

                collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true },

                friction = 0.5f,
                restitution = 0.5f
            };
            world.Add(sphere, sphereData);
        }

        // Instatiate the ground plane and add it to our physics world
        {
            GameObject plane = Instantiate(planePrefab);
            Particle planeData = new Particle
            {
                pos = Vector3.zero,
                vel = Vector3.zero,
                acc = Vector3.zero,

                mass = 1.0f,
                gravityScale = 0.0f,

                collider = new Collider { shape = Shape.PLANE, normal = Vector3.up, dynamic = false },

                friction = 1.0f,
                restitution = 1.0f
            };
            world.Add(plane, planeData);
        }
    }

    // Late upadte so users apply transformations & forces before physics simulation
    void LateUpdate()
    {
        world.PreUpdate();              // Write from Unity to Engine
        world.Update(Time.deltaTime);   // Update Engine
        world.PostUpdate();             // Write from Engine to Unity
    }
}
