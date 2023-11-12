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
    private Vector3 velocity = Vector3.zero;
    private float inverseMass = 1.0f;

    public float damping = 1.0f;
    public float gravityScale = 1.0f;
    public bool colliding = false;  // internal
    public Shape shape;

    public void SetMass(float mass)
    {
        inverseMass = 1.0f / mass;
    }

    public void SetInfiniteMass()
    {
        inverseMass = 0.0f;
    }

    public bool Dynamic()
    {
        return inverseMass > 0.0f;
    }

    public Vector3 Force()
    {
        return force;
    }

    public Vector3 Velocity()
    {
        return velocity;
    }

    public void ResetForce()
    {
        force = Vector3.zero;
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    public void AddForce(Vector3 force)
    {
        this.force += force;
    }

    public void AddAcceleration(Vector3 acceleration)
    {
        force += acceleration * inverseMass;
    }

    public void AddImpulse(Vector3 impulse)
    {
        velocity += impulse * inverseMass;
    }

    public void AddVelocity(Vector3 velocity)
    {
        this.velocity += velocity;
    }

    public void AddNormalForce(Vector3 normal, float depth)
    {
        // Unsure how to correct velocity;
        // Normal force counteracts one frame worth of gravitational force,
        // but not enough to couneract velocity (accumulated gravitational acceleration)...
        AddForce(Vector3.Reflect(force, normal));
        transform.position += normal * depth;
        // 1. Spheres accumulate acceleration
        // 2. Spheres collide with plane
        // 3. Normal force applied
        // 4. MTV applied
        // 5. Integration -- downwards velocity pushes spheres below plane

        // Fix: delay position resolution until after dynamics integration;
        // 1. Detect collisions and apply collision forces (normal & friction)
        // 2. Integrate
        // 3. Detection collisions again.
        // 4. Resolve positions
    }

    public void ApplyGravity(Vector3 gravity)
    {
        // Can't apply gravity to masses of zero (divide by zero error)
        if (Dynamic())
            force += gravity * gravityScale / inverseMass;
    }

    public void Integrate(float dt)
    {
        Vector3 acc = force * inverseMass;
        velocity += acc * dt;
        transform.position += velocity * damping * dt + 0.5f * acc * dt * dt;
        force = Vector3.zero;
    }
}
