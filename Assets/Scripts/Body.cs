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
    private Vector3 force = Vector3.zero;
    private Vector3 vel = Vector3.zero;
    private float invMass = 1.0f;

    public float damping = 1.0f;
    public float gravityScale = 1.0f;
    public bool dynamic = false;
    public bool colliding = false;  // internal
    public Shape shape;

    public Vector3 Force()
    {
        return force;
    }

    public Vector3 Velocity()
    {
        return vel;
    }

    public float Mass()
    {
        return 1.0f / invMass;
    }

    public void ResetForce()
    {
        force = Vector3.zero;
    }

    public void ResetVelocity()
    {
        vel = Vector3.zero;
    }

    public void ResetMass()
    {
        invMass = 1.0f;
    }

    public void AddForce(Vector3 force)
    {
        this.force += force;
    }

    public void AddAcceleration(Vector3 acceleration)
    {
        force += acceleration * invMass;
    }

    public void AddImpulse(Vector3 impulse)
    {
        vel += impulse * invMass;
    }

    public void AddVelocity(Vector3 velocity)
    {
        vel += velocity;
    }

    public void SetMass(float mass)
    {
        invMass = 1.0f / mass;
    }

    public void ApplyGravity(Vector3 gravity)
    {
        force += gravity * gravityScale / invMass;
    }

    public void Integrate(float dt)
    {
        Vector3 acc = force * invMass;
        vel += acc * dt;
        transform.position += vel * damping * dt + 0.5f * acc * dt * dt;
        force = Vector3.zero;
    }
}
