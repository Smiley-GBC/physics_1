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

    public static bool Check2(
        Vector3 position1, Vector3 position2, Collider collider1, Collider collider2, Mtv mtv = null)
    {
        if (collider1.shape == Shape2.SPHERE && collider2.shape == Shape2.SPHERE)
            return SphereSphere(position1, collider1.radius, position2, collider2.radius, mtv);

        if (collider1.shape == Shape2.SPHERE && collider2.shape == Shape2.PLANE)
            return SpherePlane(position1, collider1.radius, position2, collider2.normal, mtv);

        if (collider1.shape == Shape2.PLANE && collider2.shape == Shape2.SPHERE)
            return SpherePlane(position2, collider2.radius, position1, collider2.normal, mtv);

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

    public static void Resolve(Manifold manifold)
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
}
