using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsWorld : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject planePrefab;
    Body plane;

    // Start is called before the first frame update
    void Start()
    {
        Init7();
    }

    void Init7()
    {
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body body = Add(spherePrefab, new Vector3(angle, 5.0f, 0.0f), Quaternion.identity);
            body.shape = new Sphere { radius = 0.5f };
            body.shape.type = ShapeType.SPHERE;
        }

        plane = Add(planePrefab, Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        plane.shape = new Plane { type = ShapeType.PLANE };
        plane.SetInfiniteMass();
    }

    private void FixedUpdate()
    {
        // Forces
        for (int i = 0; i < bodies.Count; i++)
            bodies[i].ApplyGravity(Physics.gravity);

        // Apply collision forces
        List<Manifold> collisions = Collision.DetectCollisions(bodies);
        for (int i = 0; i < collisions.Count; i++)
            Collision.ResolveDynamics(collisions[i]);

        // Update positions & velocities (integration)
        for (int i = 0; i < bodies.Count; i++)
            bodies[i].Integrate(Time.fixedDeltaTime);

        // Resolve positions
        collisions = Collision.DetectCollisions(bodies);
        for (int i = 0; i < collisions.Count; i++)
            Collision.ResolvePenetration(collisions[i]);

        // Render
        SetColors();
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

    void SetColors()
    {
        List<bool> collisions = new List<bool>(new bool[bodies.Count]);

        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                collisions[i] |= collisions[j] |= Collision.Check(bodies[i], bodies[j]);
            }
        }

        for (int i = 0; i < collisions.Count; i++)
        {
            Color color = collisions[i] ? Color.red : Color.green;
            bodies[i].GetComponent<Renderer>().material.color = color;
        }
    }

    List<Body> bodies = new List<Body>();
}
