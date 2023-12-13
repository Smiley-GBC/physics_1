using UnityEngine;

public class Game : MonoBehaviour
{
    PhysicsWorld world = new PhysicsWorld();
    public GameObject spherePrefab;
    public GameObject planePrefab;

    public GameObject launcher;
    GameObject bird;
    GameObject pig;
    Vector3 launchPosition;

    public GameObject test;

    void OnBirdCollision(Manifold manifold)
    {
        if (world.mTags[manifold.b] == Tag.PIG)
        {
            // Handle bird-pig collision
            Debug.Log("Pig hit!");
            pig.GetComponent<MeshRenderer>().material.color = Color.magenta;
        }
        else if (world.mTags[manifold.b] == Tag.WALL)
        {
            Debug.Log("Wall hit!");
            // Handle bird-wall collision
        }
    }

    void Start()
    {
        CapsuleCollider launcherCollider = launcher.GetComponent<CapsuleCollider>();
        launchPosition = launcherCollider.transform.position + new Vector3(0.0f, launcherCollider.height * 0.5f + launcherCollider.radius, 0.0f);
        
        // Right sphere that will bounce off left sphere then slowly comes to rest
        //{
        //    GameObject sphere = Instantiate(spherePrefab);
        //    Particle sphereData = new Particle
        //    {
        //        pos = new Vector3(-1.0f, 5.0f, 0.0f),
        //        vel = new Vector3(-1.0f, 0.0f, 0.0f),
        //        acc = Vector3.zero,
        //
        //        mass = 1.0f,
        //        gravityScale = 1.0f,
        //
        //        collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true },
        //
        //        friction = 0.01f,
        //        restitution = 0.75f
        //    };
        //    world.Add(sphere, sphereData);
        //}

        // Left sphere moving right that will quickly stop bouncing and come to rest
        //{
        //    GameObject sphere = Instantiate(spherePrefab);
        //    Particle sphereData = new Particle
        //    {
        //        pos = new Vector3(-3.0f, 0.5f, 0.0f),
        //        vel = new Vector3( 3.0f, 0.0f, 0.0f),
        //        acc = Vector3.zero,
        //
        //        mass = 1.0f,
        //        gravityScale = 1.0f,
        //
        //        collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true },
        //
        //        friction = 0.5f,
        //        restitution = 0.5f
        //    };
        //    world.Add(sphere, sphereData);
        //}

        bird = Instantiate(spherePrefab);
        Particle birdData = new Particle
        {
            pos = new Vector3(-5.0f, 2.5f, 0.0f),
            vel = Vector3.zero,
            acc = Vector3.zero,

            mass = 1.0f,
            gravityScale = 0.0f,

            collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true },

            friction = 0.01f,
            restitution = 0.5f,

            onCollision = OnBirdCollision,
            tag = Tag.BIRD,
        };
        world.Add(bird, birdData);

        pig = Instantiate(spherePrefab);
        Particle pigData = new Particle
        {
            pos = new Vector3(5.0f, 0.5f, 0.0f),
            vel = Vector3.zero,
            acc = Vector3.zero,

            mass = 1.0f,
            gravityScale = 1.0f,

            collider = new Collider { shape = Shape.SPHERE, radius = 0.5f, dynamic = true },

            friction = 0.01f,
            restitution = 0.5f,

            onCollision = null,
            tag = Tag.PIG,
        };
        world.Add(pig, pigData);

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
                restitution = 1.0f,

                onCollision = null,
                tag = Tag.WALL,
            };
            world.Add(plane, planeData);
        }
    }

    // Late upadte so users apply transformations & forces before physics simulation
    void LateUpdate()
    {
        world.PreUpdate();              // Write from Unity to Engine
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;
        test.transform.position = mouse;

        if (Input.GetMouseButton(0))
        {
            int i = world.Get(bird);
            Vector3 launchDirection = (launchPosition - mouse).normalized;
            world.mVelocities[i] = launchDirection * 5.0f;
            world.mGravityScales[i] = 1.0f;
        }

        world.Update(Time.deltaTime);   // Update Engine
        world.PostUpdate();             // Write from Engine to Unity
    }
}
