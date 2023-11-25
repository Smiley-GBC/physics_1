using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsWorld
{
    // The following values are initialized at runtime so they can't be used as constants...
    public float step;      //Time.fixedDeltaTime 
    public Vector3 gravity; //Physics.gravity

    public void Simulate()
    {
        // Forces
        for (int i = 0; i < bodies.Count; i++)
            bodies[i].ApplyGravity(gravity);

        // Apply collision forces
        List<Manifold> collisions = Collision.DetectCollisions(bodies);
        for (int i = 0; i < collisions.Count; i++)
            Dynamics.Resolve(collisions[i]);

        // Update positions & velocities (integration)
        for (int i = 0; i < bodies.Count; i++)
            bodies[i].Integrate(step);

        // Resolve positions (pure force engine shouldn't need this)
        collisions = Collision.DetectCollisions(bodies);
        for (int i = 0; i < collisions.Count; i++)
            Collision.Resolve(collisions[i]);

        // Render
        SetColors();
    }

    public Body Add(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject physicsObject = Object.Instantiate(prefab, position, rotation);
        Body body = physicsObject.GetComponent<Body>();
        bodies.Add(body);
        return body;
    }

    public void Remove(Body body)
    {
        bodies.Remove(body);
        Object.Destroy(body.gameObject);
    }

    public void Clear()
    {
        for (int i = 0; i < bodies.Count; i++)
            Object.Destroy(bodies[i].gameObject);
        bodies.Clear();
    }

    void SetColors()
    {
        List<bool> collisions = new List<bool>(new bool[bodies.Count]);

        // No guarantee about the order of Collision.DetectCollisions(bodies) so must test manually
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
