using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game2 : MonoBehaviour
{
    PhysicsWorld world = new PhysicsWorld();
    public GameObject spherePrefab;
    public GameObject planePrefab;

    void Start()
    {
        // Instantiate spheres and add them to our physics world
        //for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        //{
        //    GameObject sphere = Instantiate(spherePrefab);
        //    Particle sphereData = new Particle
        //    {
        //        pos = new Vector3(angle, 5.0f, 0.0f),
        //        vel = Vector3.zero,
        //        mass = 1.0f,
        //        gravityScale = 0.25f,
        //
        //        collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true }
        //    };
        //    world.Add(sphere, sphereData);
        //}

        {
            GameObject sphere = Instantiate(spherePrefab);
            Particle sphereData = new Particle
            {
                pos = new Vector3(0.0f, 5.0f, 0.0f),
                vel = Vector3.zero,
                mass = 1.0f,
                gravityScale = 1.0f,

                collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true }
            };
            world.Add(sphere, sphereData);
        }

        {
            GameObject sphere = Instantiate(spherePrefab);
            Particle sphereData = new Particle
            {
                pos = new Vector3(-3.0f, 5.0f, 0.0f),
                vel = new Vector3(1.0f, 0.0f, 0.0f),
                mass = 1.0f,
                gravityScale = 1.0f,

                collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true }
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
                mass = 1.0f,
                gravityScale = 0.0f,

                collider = new Collider { shape = Shape.PLANE, normal = Vector3.up, dynamic = false }
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
