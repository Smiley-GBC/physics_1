using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mtv
{
    public Vector3 normal;
    public float depth;
}

public class Manifold
{
    public PhysicsObject a = null;
    public PhysicsObject b = null;
    public Mtv mtv = null;
}

public class PhysicsWorld
{
    Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);
    float timestep = 1.0f / 50.0f;
    float prevTime = 0.0f;
    float currTime = 0.0f;

    List<GameObject> objects = new List<GameObject>();

    void Add(GameObject obj)
    {
        objects.Add(obj);
    }

    void Remove(GameObject obj)
    {
        objects.Remove(obj);
        Object.Destroy(obj);
    }

    void Clear()
    {
        foreach (GameObject obj in objects)
            Object.Destroy(obj);
        objects.Clear();
    }

    // Unity is already slow and this physics engine ain't AAA,
    // so its easier to query all components every frame vs
    // maintaining some nonsensical Physics-to-GameObject data-structure...
    public void Update(float dt)
    {
        // Fetch physics components and step physics simulation
        PhysicsObject[] physicsObjects = Object.FindObjectsOfType<PhysicsObject>();
        while (prevTime < currTime)
        {
            prevTime += timestep;
            Simulate(physicsObjects, timestep);
        }
        currTime += dt;

        // Determine collisions
        List<bool> collisions = new List<bool>(new bool[physicsObjects.Length]);
        for (int i = 0; i < physicsObjects.Length; i++)
        {
            for (int j = i + 1; j < physicsObjects.Length; j++)
            {
                collisions[i] |= collisions[j] |= Collision.Check(
                    physicsObjects[i].pos, physicsObjects[i].collider,
                    physicsObjects[j].pos, physicsObjects[j].collider);
            }
        }

        // Write back positions & normals to/from game objects and render red/green for collision vs no collision
        for (int i = 0; i < physicsObjects.Length; i++)
        {
            PhysicsObject obj = physicsObjects[i];
            obj.gameObject.GetComponent<Renderer>().material.color = collisions[i] ? Color.red : Color.green;
            obj.transform.position = obj.pos;
            if (obj.collider.shape == Shape.PLANE)
                obj.collider.normal = obj.transform.up;
        }
    }

    void Simulate(PhysicsObject[] physicsObjects, float dt)
    {
        foreach (PhysicsObject obj in physicsObjects)
        {
            obj.acc = obj.fNet * obj.invMass;       // Fa
            obj.acc += gravity * obj.gravityScale;  // Fg
            obj.fNet = Vector3.zero;

            obj.vel = Dynamics.Integrate(obj.vel, obj.acc, dt);
            obj.pos = Dynamics.Integrate(obj.pos, obj.vel, dt);
        }

        List<Manifold> collisions = Collision.Collisions(physicsObjects);
        foreach (Manifold collision in collisions)
        {
            Dynamics.ResolveVelocity(collision);
            Dynamics.ResolvePosition(collision);
        }
    }

    public static class Dynamics
    {
        // "Compute the rate of change"
        // Ex: solve for v given pi = 5m, pf = 10m, and t = 2s
        // v = (pf - pi) / t --> v = (10 - 5) / 2 --> v = 2.5m/s
        public static Vector3 Differentiate(Vector3 final, Vector3 initial, float dt)
        {
            return (final - initial) / dt;
        }

        // "Apply the rate of change"
        // Ex: solve for pf given pi = 5m, vi = 2.5m/s, and t = 2s 
        // pf = pi + vi * t --> pf = 5 + 2.5 * 2 --> pf = 10m
        public static Vector3 Integrate(Vector3 value, Vector3 change, float dt)
        {
            return value + change * dt;
        }

        public static void ResolveVelocity(Manifold collision)
        {
            PhysicsObject a = collision.a;
            PhysicsObject b = collision.b;
            // Exit if both objects are immovable
            float invMassSum = a.invMass + b.invMass;
            if (invMassSum <= Mathf.Epsilon) return;

            // Exit if both objects are moving away from each other
            Vector3 vBA = a.vel - b.vel;
            float t = Vector3.Dot(vBA, collision.mtv.normal);
            if (t > 0.0f) return;

            // Apply impulse to velocities
            float restitution = Mathf.Min(a.restitution, b.restitution);
            float impulseMagnitude = -(1.0f + restitution) * t / invMassSum;
            Vector3 impulse = collision.mtv.normal * impulseMagnitude;
            a.vel += impulse * a.invMass;
            b.vel -= impulse * b.invMass;

            // Scale friction based on how similar the relative velocity is to the collision normal
            Vector3 frictionDirection = (vBA - (collision.mtv.normal * t)).normalized;
            float frictionMagnitude = -Vector3.Dot(vBA, frictionDirection) / invMassSum;

            // Coulomb's Law
            float mu = Mathf.Sqrt(a.friction * b.friction);
            frictionMagnitude = Mathf.Clamp(frictionMagnitude,
                -impulseMagnitude * mu, impulseMagnitude * mu);

            // Apply friction to velocities
            Vector3 friction = frictionMagnitude * frictionDirection;
            a.vel += friction * a.invMass;
            b.vel -= friction * b.invMass;
        }

        public static void ResolvePosition(Manifold collision)
        {
            Vector3 mtv = collision.mtv.normal * collision.mtv.depth;
            if (!collision.b.InfMass())
            {
                collision.a.pos += mtv * 0.5f;
                collision.b.pos -= mtv * 0.5f;
            }
            else
            {
                collision.a.pos += mtv;
            }
        }
    }

    public static class Collision
    {
        public static List<Manifold> Collisions(PhysicsObject[] physicsObjects)
        {
            List<Manifold> collisions = new List<Manifold>();
            for (int i = 0; i < physicsObjects.Length; i++)
            {
                for (int j = i + 1; j < physicsObjects.Length; j++)
                {
                    Mtv mtv = new Mtv();
                    if (Check(
                        physicsObjects[i].pos, physicsObjects[i].collider,
                        physicsObjects[j].pos, physicsObjects[j].collider, mtv))
                    {
                        // Ensure A is always moveable. Flip mtv if A and B are swapped.
                        if (physicsObjects[i].InfMass() && !physicsObjects[j].InfMass())
                        {
                            mtv.normal *= -1.0f;
                            collisions.Add(new Manifold() { a = physicsObjects[j], b = physicsObjects[i], mtv = mtv });
                        }
                        else
                            collisions.Add(new Manifold() { a = physicsObjects[i], b = physicsObjects[j], mtv = mtv });
                        // Flipping mtv based on dot-product only works if objects don't tunnel
                        // (ie if 60% of a sphere is inside a plane, then the above logic would flip the normal the wrong way)
                        // Continuous collision detection would fix this, although I'm pretty sure this isn't actually broken!
                    }
                }
            }
            return collisions;
        }

        public static bool Check(
            Vector3 position1, Collider collider1, Vector3 position2, Collider collider2, Mtv mtv = null)
        {
            if (collider1.shape == Shape.SPHERE && collider2.shape == Shape.SPHERE)
                return SphereSphere(position1, collider1.radius, position2, collider2.radius, mtv);

            if (collider1.shape == Shape.SPHERE && collider2.shape == Shape.PLANE)
                return SpherePlane(position1, collider1.radius, position2, collider2.normal, mtv);

            if (collider1.shape == Shape.PLANE && collider2.shape == Shape.SPHERE)
                return SpherePlane(position2, collider2.radius, position1, collider2.normal, mtv);

            return false;
        }

        static bool SphereSphere(
            Vector3 position1, float radius1, Vector3 position2, float radius2, Mtv mtv = null)
        {
            Vector3 direction = position1 - position2;
            float distance = direction.magnitude;
            float radiiSum = radius1 + radius2;
            bool collision = distance <= radiiSum;
            if (collision && mtv != null)
            {
                // Normal points from 2 to 1
                mtv.normal = direction.normalized;
                mtv.depth = radiiSum - distance;
            }
            return collision;
        }

        static bool SpherePlane(
            Vector3 spherePosition, float sphereRadius, Vector3 planePosition, Vector3 planeNormal, Mtv mtv = null)
        {
            float distance = Vector3.Dot(spherePosition - planePosition, planeNormal);
            bool collision = distance <= sphereRadius;
            if (collision && mtv != null)
            {
                mtv.normal = planeNormal;
                mtv.depth = sphereRadius - distance;
            }
            return collision;
        }
    }
}
