using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shape
{
    SPHERE,
    PLANE
}

public class Collider
{
    public Shape shape;     // SPHERE or PLANE
    public float radius;    // if SPHERE, radius will have a value
    public Vector3 normal;  // if PLANE,  normal will have a value
}

// Data class. Derived from MonoBehaviour to interface nicely with Unity (simplifies add & remove).
public class PhysicsObject : MonoBehaviour
{
    // Motion
    public Vector3 pos = Vector3.zero;
    public Vector3 vel = Vector3.zero;
    public Vector3 acc = Vector3.zero;
    public Vector3 fNet = Vector3.zero;

    // Properties
    public float gravityScale = 1.0f;
    public float invMass = 1.0f;
    public float friction = 0.0f;
    public float restitution = 1.0f;
    public bool InfMass() { return invMass > Mathf.Epsilon; }

    // Collision
    public Collider collider = null;
}
