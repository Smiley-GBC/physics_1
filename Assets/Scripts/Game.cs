using UnityEngine;

public class Game : MonoBehaviour
{
    PhysicsWorld world = new PhysicsWorld();
    public GameObject spherePrefab;
    public GameObject planePrefab;

    void Start()
    {
        // Ground plane
        {
            world.Add(Instantiate(planePrefab));
        }

        // Right sphere that will bounce off left sphere then slowly comes to rest
        {
            GameObject sphereGO = Instantiate(spherePrefab);
            PhysicsObject sphere = sphereGO.GetComponent<PhysicsObject>();
            sphere.pos = new Vector3(0.0f, 1.0f, 0.0f);
            //sphere.pos = new Vector3(-1.0f, 1.0f, 0.0f);
            //sphere.vel = new Vector3(-1.0f, 0.0f, 0.0f);
            //sphere.friction = 0.01f;
            //sphere.restitution = 0.75f;
            world.Add(sphereGO);
        }

        // Left sphere moving right that will quickly stop bouncing and come to rest
        //{
        //    GameObject sphereGO = Instantiate(spherePrefab);
        //    PhysicsObject sphere = sphereGO.GetComponent<PhysicsObject>();
        //    sphere.pos = new Vector3(-3.0f, 1.0f, 0.0f);
        //    sphere.vel = new Vector3( 3.0f, 0.0f, 0.0f);
        //    sphere.friction = 0.01f;
        //    sphere.restitution = 0.75f;
        //    world.Add(sphereGO);
        //}
    }

    // Late update to ensure external changes are made before stepping the physics simulation
    // (ie teleports, inspector modifications, MonoBehaviour updates)
    void LateUpdate()
    {
        world.Update(Time.deltaTime);
    }
}
