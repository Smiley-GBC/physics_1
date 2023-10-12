using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ShapeType
{
    SPHERE,
    PLANE
}

public class Shape
{
    public ShapeType type;
}

public class Sphere : Shape
{
    public float radius;
}

public class Plane : Shape
{
    // Typically planes are stored in "point-normal" form, which is
    // Vector3 position;
    // Vector3 normal;

    // However, we want our plane's normal and position to change with that of its gameObject.
    // Hence, we don't actually need to store anything at all!
    // (position = transform.position, normal = transform.up)
}

public class Body : MonoBehaviour
{
    // Using transform.position since we're deriving from MonoBehaviour!
    //public Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vel = new Vector3(0.0f, 0.0f, 0.0f);
    public float mass = 1.0f;
    public float drag = 0.0f;
    public bool gravity = true;

    public Shape shape;

    public void Simulate(Vector3 acc, float dt)
    {
        if (gravity)
            vel = vel + acc * mass * dt;
        transform.position = transform.position + vel * dt;
    }

    public bool CheckCollision(Body body)
    {
        // Simplest case -- both objects are spheres
        if (shape.type == ShapeType.SPHERE && body.shape.type == ShapeType.SPHERE)
        {
            float radius0 = ((Sphere)shape).radius;
            float radius1 = ((Sphere)body.shape).radius;
            return CheckCollisionSpheres(transform.position, radius0, body.transform.position, radius1);
        }

        // Current object is a plane, passed in object is a sphere
        if (shape.type == ShapeType.PLANE && body.shape.type == ShapeType.SPHERE)
        {
            return CheckCollisionSpherePlane(
                body.transform.position, ((Sphere)body.shape).radius,
                transform.position, transform.up
            );
        }

        // Current object is a sphere, passed in object is a plane
        if (shape.type == ShapeType.SPHERE && body.shape.type == ShapeType.PLANE)
        {
            return CheckCollisionSpherePlane(
                transform.position, ((Sphere)shape).radius,
                body.transform.position, body.transform.up
            );
        }

        // Default to false otherwise (ie if both objects are planes we can't do a collision test)
        return false;
    }

    public bool CheckCollisionSpheres(Vector3 position1, float radius1, Vector3 position2, float radius2)
    {
        float distance = (position1 - position2).magnitude;
        return distance <= radius1 + radius2;
    }

    public bool CheckCollisionSpherePlane(
        Vector3 spherePosition, float sphereRadius,
        Vector3 planePosition, Vector3 planeNormal)
    {
        // Plane-sphere intersection (homework):
        // 1. Project vector from plane to circle onto plane normal to determine distance from plane
        // 2. Point-circle collision from here -- simply compare distance to circle's radius!

        // (Don't actually return false. Return the outcome of the collision)
        return false;
    }
}
