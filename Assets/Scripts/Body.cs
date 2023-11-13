using System.Collections;
using System.Collections.Generic;
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

    public Shape shape;
    public float damping = 1.0f;
    public float gravityScale = 1.0f;
    public float restitution = 1.0f;

    public float InverseMass()
    {
        return Dynamic() ? inverseMass : 0.0f;
    }

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

    public Vector3 Acceleration()
    {
        return force * inverseMass;
    }

    public Vector3 Impulse()
    {
        return velocity * inverseMass;
    }

    public Vector3 Velocity()
    {
        return velocity;
    }

    public void SetForce(Vector3 force)
    {
        this.force = force;
    }

    public void SetAcceleration(Vector3 acceleration)
    {
        force = acceleration * inverseMass;
    }

    public void SetImpulse(Vector3 impulse)
    {
        velocity = impulse * inverseMass;
    }

    public void SetVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
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

    public void ApplyGravity(Vector3 gravity)
    {
        // Can't apply gravity to masses of zero (divide by zero error)
        if (Dynamic())
            force += gravity * gravityScale / inverseMass;
    }

    public void Integrate(float dt)
    {
        // No point in integrating static objects
        if (Dynamic())
        {
            Vector3 acc = force * inverseMass;
            velocity += acc * dt;
            velocity *= Mathf.Pow(damping, dt);
            transform.position += velocity * damping * dt;
            force = Vector3.zero;
        }
    }
}
