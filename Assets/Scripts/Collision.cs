using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision
{
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

    public static bool SphereSphere(Vector3 position1, float radius1, Vector3 position2, float radius2, Mtv mtv)
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
                    // Ensure body1 is a dynamic object
                    // Ensure normal points from body2 to body1
                    Body body1 = bodies[i].Dynamic() ? bodies[i] : null;
                    Body body2 = bodies[j].Dynamic() ? bodies[j] : null;
                    if (body1 == null && body2 != null)
                    {
                        body1 = body2;
                        body2 = null;
                        mtv.normal *= -1.0f;
                    }
                    collisions.Add(new Manifold() { body1 = body1, body2 = body2, mtv = mtv });
                }
            }
        }
        return collisions;
    }

    public static void ResolveDynamics(Manifold manifold)
    {
        if (manifold.body2 != null)
        {
            manifold.body1.AddForce( manifold.mtv.normal * manifold.body1.Force().magnitude * 0.5f);
            manifold.body2.AddForce(-manifold.mtv.normal * manifold.body2.Force().magnitude * 0.5f);
        }
        else
        {
            manifold.body1.AddForce(manifold.mtv.normal * manifold.body1.Force().magnitude);
        }
    }

    public static void ResolvePenetration(Manifold manifold)
    {
        if (manifold.body2 != null)
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
}
