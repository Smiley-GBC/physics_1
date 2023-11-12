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
                Body body1 = bodies[i];
                Body body2 = bodies[j];
                if (Check(body1, body2, mtv))
                {
                    Manifold manifold = new Manifold();
                    manifold.body1 = body1;
                    manifold.body2 = body2;
                    manifold.mtv = mtv;
                    collisions.Add(manifold);
                }
            }
        }
        return collisions;
    }

    public static void ResolveDynamics(Manifold manifold)
    {
        Body body1 = manifold.body1;
        Body body2 = manifold.body2;
        ShapeType shape1 = body1.shape.type;
        ShapeType shape2 = body2.shape.type;

        if (shape1 == ShapeType.SPHERE && shape2 == ShapeType.SPHERE)
        {
            // TODO -- resolve this properly with impulses
        }
        else
        {
            Body sphere = shape1 == ShapeType.SPHERE ? body1 : body2;
            Body plane = shape1 == ShapeType.PLANE ? body1 : body2;
            sphere.AddForce(plane.transform.up * sphere.Force().magnitude);
            //sphere.AddVelocity(plane.transform.up * sphere.Velocity().magnitude);
            // TODO -- fix this with impulse?
        }
    }

    public static void ResolvePenetration(Manifold manifold)
    {
        Body body1 = manifold.body1;
        Body body2 = manifold.body2;
        ShapeType shape1 = body1.shape.type;
        ShapeType shape2 = body2.shape.type;

        if (shape1 == ShapeType.SPHERE && shape2 == ShapeType.SPHERE)
        {
            Vector3 mtv1 = Vector3.zero;
            Vector3 mtv2 = Vector3.zero;
            if (body1.Dynamic() && body2.Dynamic())
            {
                mtv1 = manifold.mtv.normal * manifold.mtv.depth * 0.5f;
                mtv2 = -manifold.mtv.normal * manifold.mtv.depth * 0.5f;
            }
            else if (body1.Dynamic())
            {
                mtv1 = manifold.mtv.normal * manifold.mtv.depth;
            }
            else if (body2.Dynamic())
            {
                mtv2 = -manifold.mtv.normal * manifold.mtv.depth;
            }

            body1.gameObject.transform.position += mtv1;
            body2.gameObject.transform.position += mtv2;
        }
        else
        {
            float depth = Mathf.Abs(manifold.mtv.depth);
            Vector3 normal = manifold.mtv.normal;
            Body body = shape1 == ShapeType.SPHERE ? body1 : body2;
            if (body.Dynamic())
                body.gameObject.transform.position += normal * depth;
        }
    }
}
