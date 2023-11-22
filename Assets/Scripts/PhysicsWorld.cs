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
        // 1. Apply gravity
        // 2. See if gravity causes a collision
        // 3. Resolve the collision in terms of force
        // 4. Simulate motion
        // 5. Resolve motion-based collisions

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

        // Resolve positions
        //collisions = Collision.DetectCollisions(bodies);
        //for (int i = 0; i < collisions.Count; i++)
        //    Collision.Resolve(collisions[i]);

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

    public void RemoveAll()
    {
        for (int i = 0; i < bodies.Count; i++)
            Remove(bodies[i]);
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
