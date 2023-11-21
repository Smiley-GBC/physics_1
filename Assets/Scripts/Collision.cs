using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mtv
{
    public Vector3 normal = Vector3.zero;
    public float depth = 0.0f;
}

public class Manifold
{
    public Body body1 = null;
    public Body body2 = null;
    public Mtv mtv = null;
}

public class Collision
{
    // MTV resolves 1 from 2 (resolves left from right)
    public static bool Check(Body body1, Body body2, Mtv mtv = null)
    {
        if (body1.shape.type == ShapeType.SPHERE && body2.shape.type == ShapeType.SPHERE)
        {
            return SphereSphere(
                body1.gameObject.transform.position, ((Sphere)body1.shape).radius, 
                body2.gameObject.transform.position, ((Sphere)body2.shape).radius,
                mtv
            );
        }

        if (body1.shape.type == ShapeType.SPHERE && body2.shape.type == ShapeType.PLANE)
        {
            return SpherePlane(
                body1.transform.position, ((Sphere)body1.shape).radius,
                body2.transform.position, body2.transform.up,
                mtv
            );
        }

        if (body1.shape.type == ShapeType.PLANE && body2.shape.type == ShapeType.SPHERE)
        {
            return SpherePlane(
                body2.transform.position, ((Sphere)body2.shape).radius,
                body1.transform.position, body1.transform.up,
                mtv
            );
        }

        return false;
    }

    public static bool SphereSphere(Vector3 position1, float radius1, Vector3 position2, float radius2, Mtv mtv = null)
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

    public static bool SpherePlane(Vector3 spherePosition, float sphereRadius, Vector3 planePosition, Vector3 planeNormal, Mtv mtv = null)
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

    public static List<Manifold> DetectCollisions(List<Body> bodies)
    {
        List<Manifold> collisions = new List<Manifold>();
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                Mtv mtv = new Mtv();
                if (Check(bodies[i], bodies[j], mtv))
                {
                    Body body1 = bodies[i];
                    Body body2 = bodies[j];
                    if (!body1.Dynamic() && body2.Dynamic())
                    {
                        mtv.normal *= -1.0f;
                        collisions.Add(new Manifold() { body2 = body1, body1 = body2, mtv = mtv });
                    }
                    else
                    {
                        collisions.Add(new Manifold() { body1 = body1, body2 = body2, mtv = mtv });
                    }
                }
            }
        }
        return collisions;
    }

    // TODO -- separate Collision and Dynamics into separate files
    public static void ResolveDynamics(Manifold manifold)
    {
        if (manifold.body2.Dynamic())
        {
            // Don't worry about rendering forces if both objects are dynamic
            // Use the frenet-frame algorithm to do so if interested
            manifold.body1.AddForce( manifold.mtv.normal * manifold.body1.Force().magnitude * 0.5f);
            manifold.body2.AddForce(-manifold.mtv.normal * manifold.body2.Force().magnitude * 0.5f);
        }
        else
        {
            Body body = manifold.body1;
            // "Frenet-Frame algorithm" -- determines perpendicular vector to forward vector given a world up vector
            float forceMagnitude = body.Force().magnitude;
            Vector3 forward = Vector3.Cross(manifold.mtv.normal, Vector3.up);

            Vector3 fn = manifold.body2.transform.up * forceMagnitude;
            Vector3 fs = Vector3.Cross(forward, manifold.mtv.normal).normalized * forceMagnitude;
            Vector3 fg = body.GravitationalForce(Physics.gravity);

            Vector3 start = body.transform.position;
            Debug.DrawLine(start, start + fn, Color.green);
            Debug.DrawLine(start, start + fs, Color.yellow);
            Debug.DrawLine(start, start + fg, Color.magenta);
            body.AddForce(manifold.mtv.normal * body.Force().magnitude);

            // Proof of concept.
            // Since the velocity will be different after integrating Fg & Fn, this doesn't quite work.
            // Fix by separating vel vs pos integration
            Vector3 counterAcceleration = CounterAcceleration(body.Velocity(), Time.fixedDeltaTime);
            body.AddAcceleration(counterAcceleration);
        }
    }

    // Return an acceleration that will zero the passed-in velocity after integration
    public static Vector3 CounterAcceleration(Vector3 velocity, float dt)
    {
        // vf = vi + a * t
        // 0 = vi + a * t
        // -vi = a * t
        // -vi / t = a
        return (-velocity / dt);
    }

    public static void ResolvePenetration(Manifold manifold)
    {
        if (manifold.body2.Dynamic())
        {
            Vector3 mtv = manifold.mtv.normal * manifold.mtv.depth * 0.5f;
            manifold.body1.transform.position += mtv;
            manifold.body2.transform.position -= mtv;
        }
        else
        {
            manifold.body1.transform.position += manifold.mtv.normal * manifold.mtv.depth;
        }
    }

    public static float SeparationVelocity(Manifold manifold)
    {
        Vector3 relativeVelocity = manifold.body1.Velocity();
        if (manifold.body2 != null)
            relativeVelocity -= manifold.body2.Velocity();
        return Vector3.Dot(relativeVelocity, manifold.mtv.normal);
    }
}
