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
    // Vector3 position = transform.position;
    // Vector3 normal = transform.up;
}

public class Body : MonoBehaviour
{
    public Vector3 vel = Vector3.zero;
    public Vector3 force = Vector3.zero;
    public float mass = 1.0f;
    public float drag = 0.0f;
    public float gravityScale = 1.0f;
    public bool dynamic = false;
    public Shape shape;

    public void Simulate(Vector3 acc, float dt)
    {
        vel = vel + acc * gravityScale * mass * dt;
        transform.position = transform.position + vel * dt;
    }
}
