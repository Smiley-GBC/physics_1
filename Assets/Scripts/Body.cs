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
    public Vector3 direction;
    // calculate normal based on direction
    // retrieve position from gameObject
}

/*struct Plane
{
	Vector2 position;
	Vector2 normal;
};*/

public class Body : MonoBehaviour
{
    // Using transform.position since we're deriving from MonoBehaviour!
    //public Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vel = new Vector3(0.0f, 0.0f, 0.0f);
    public float mass = 1.0f;
    public float drag = 0.0f;

    public Shape shape;

    public void Simulate(Vector3 acc, float dt)
    {
        vel = vel + acc * mass * dt;
        transform.position = transform.position + vel * dt;
    }

    public bool CheckCollision(Body body)
    {
        // Simplest test -- both objects are spheres
        if (shape.type == ShapeType.SPHERE && body.shape.type == ShapeType.SPHERE)
        {
            float radius0 = ((Sphere)shape).radius;
            float radius1 = ((Sphere)body.shape).radius;
            return CheckCollisionSpheres(transform.position, radius0, body.transform.position, radius1);
        }

        // Current object is a plane, passed in object is a sphere
        if (shape.type == ShapeType.PLANE && body.shape.type == ShapeType.SPHERE)
        {
            // 0 = current, 1 = other
            Vector3 direction = ((Plane)shape).direction;

            // Need a forward and a right vector to determine normal
            Vector3 normal = Vector3.Cross(direction, Vector3.up);

            float radius1 = ((Sphere)body.shape).radius;
            return CheckCollisionSpherePlane(body.transform.position, radius1, transform.position, normal);
        }

        // Current object is a sphere, passed in object is a plane
        if (shape.type == ShapeType.SPHERE && body.shape.type == ShapeType.PLANE)
        {
            // 0 = current, 1 = other
            Vector3 direction = ((Plane)body.shape).direction;

            // Need a forward and a right vector to determine normal
            Vector3 normal = Vector3.Cross(direction, Vector3.up);

            float radius1 = ((Sphere)shape).radius;
            return CheckCollisionSpherePlane(transform.position, radius1, body.transform.position, normal);
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
        return false;
    }
}
