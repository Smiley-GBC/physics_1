using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair
{
    public Body body1;
    public Body body2;
}

// Homework option 1: Fix my cross-product/rendering issue
// Homework option 2: Port https://github.com/Smiley98/game_concepts/blob/56_circle_plane/game/src/main.cpp to Unity in 2D or 3D
public class PhysicsWorld : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject planePrefab;
    Body plane;

    // Start is called before the first frame update
    void Start()
    {
        Launch();
    }

    void Launch()
    {
        RemoveAll();
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body body = Add(spherePrefab, new Vector3(angle, 2.5f, 0.0f), Quaternion.identity);
            body.vel = new Vector3(0.0f, 0.0f, 0.0f);
            body.shape = new Sphere { radius = 0.5f };
            body.shape.type = ShapeType.SPHERE;
            body.gravity = false;

            // Scale = diameter so our radius needs to be half the diameter
            // (Also look at the default value of sphere collider radius)
            //body.radius = body.transform.localScale.z * 0.5f;
        }

        plane = Add(planePrefab, new Vector3(0.0f, 5.0f, 0.0f), Quaternion.identity);
        plane.shape = new Plane { direction = Vector3.forward };
        plane.shape.type = ShapeType.PLANE;
        plane.gravity = false;
    }

    private void Update()
    {
        ((Plane)plane.shape).direction = plane.transform.forward;

        if (Input.GetKeyDown(KeyCode.Space))
            Launch();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 acc = Physics.gravity;

        List<Pair> overlapping = new List<Pair>();
        for (int i = 0; i < bodies.Count; i++)
        {
            // Reset all objects to green every frame
            bodies[i].gameObject.GetComponent<Renderer>().material.color = Color.green;

            for (int j = 0; j < bodies.Count; j++)
            {
                // Don't check the same object against itself
                if (i == j) continue;

                // Reads better if we append all overlapping pairs, then resolve later on
                Body bodyA = bodies[i];
                Body bodyB = bodies[j];

                if (bodyA.CheckCollision(bodyB))
                {
                    Pair pair = new Pair();
                    pair.body1 = bodyA;
                    pair.body2 = bodyB;
                    overlapping.Add(pair);
                }
            }
            bodies[i].Simulate(acc, dt);
        }

        // Color overlapping pairs red
        for (int i = 0; i < overlapping.Count; i++)
        {
            overlapping[i].body1.gameObject.GetComponent<Renderer>().material.color = Color.red;
            overlapping[i].body2.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public Body Add(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject physicsObject = Instantiate(prefab, position, rotation);
        Body body = physicsObject.GetComponent<Body>();
        bodies.Add(body);
        return body;
    }

    public void Remove(Body body)
    {
        bodies.Remove(body);
        Destroy(body.gameObject);
    }

    public void RemoveAll()
    {
        for (int i = 0; i < bodies.Count; i++)
            Remove(bodies[i]);
    }

    List<Body> bodies = new List<Body>();
}
