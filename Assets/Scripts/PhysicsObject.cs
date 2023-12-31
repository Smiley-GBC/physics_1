using System;
using UnityEngine;

public enum Shape
{
    SPHERE,
    PLANE
}

[Serializable]
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
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 acc;
    public Vector3 force;

    // Properties
    public float gravityScale;
    public float invMass;
    public float friction;
    public float restitution;
    public bool InfMass() { return invMass <= Mathf.Epsilon; }
    public void SetMass(float mass) { invMass = mass <= Mathf.Epsilon ? 0.0f : 1.0f / invMass; }

    // Collision
    public Collider collider;

    private void Awake()
    {
        // Interpret value in inspector as mass (not inverse-mass).
        SetMass(invMass);
    }
}
