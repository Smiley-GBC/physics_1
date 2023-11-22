using System.Collections.Generic;
using UnityEngine;

public class Dynamics
{
    // Update velocities from forces
    public static void IntegrateVelocities(List<Body> bodies, Vector3 gravity, float dt)
    {
        foreach (Body body in bodies)
        {
            Vector3 acc = body.Force() * body.InverseMass();
            Vector3 vel = body.Velocity() + acc * dt;
            vel *= Mathf.Pow(body.damping, dt);
            body.SetVelocity(vel);
        }
    }

    // Update positions from velocities
    public static void IntegratePositions(List<Body> bodies, float dt)
    {
        foreach (Body body in bodies)
            body.transform.position += body.Velocity() * dt;
    }

    // Compute change in velocity over time (acceleration)
    public static Vector3 VelocityDelta(Vector3 force, float inverseMass, float dt)
    {
        return force * inverseMass * dt;
    }

    // Compute change in position over time (velocity)
    public static Vector3 PositionDelta(Vector3 velocity, float dt)
    {
        return velocity * dt;
    }

    public static void Resolve(Manifold manifold)
    {
        if (manifold.body2.Dynamic())
        {
            // Don't worry about rendering forces if both objects are dynamic
            // Use the frenet-frame algorithm to do so if interested
            manifold.body1.AddForce(manifold.mtv.normal * manifold.body1.Force().magnitude * 0.5f);
            manifold.body2.AddForce(-manifold.mtv.normal * manifold.body2.Force().magnitude * 0.5f);
        }
        else
        {
            Body body = manifold.body1;
            // "Frenet-Frame algorithm" -- determines perpendicular vector to forward vector given a world up vector
            float forceMagnitude = body.Force().magnitude;
            Vector3 forward = Vector3.Cross(manifold.mtv.normal, Vector3.up);

            Vector3 fn = manifold.body2.transform.up * forceMagnitude;
            float u = (1.0f - body.friction);
            Vector3 fs = Vector3.Cross(forward, manifold.mtv.normal).normalized * forceMagnitude * u;
            Vector3 fg = body.GravitationalForce(Physics.gravity);

            Vector3 start = body.transform.position;
            Debug.DrawLine(start, start + fn, Color.green);
            Debug.DrawLine(start, start + fs, Color.yellow);
            Debug.DrawLine(start, start + fg, Color.magenta);
            body.AddForce(manifold.mtv.normal * body.Force().magnitude);

            // Smiley's personal project -- "pure force" engine
            // Proof of concept:
            // Since the velocity will be different after integrating Fg & Fn, this doesn't quite work.
            // Fix by separating vel vs pos integration
            //Vector3 counterAcceleration = CounterAcceleration(body.Velocity(), Time.fixedDeltaTime);
            //body.AddAcceleration(counterAcceleration);

            // 1. Apply gravity
            // 2. See if gravity causes a collision
            // 3. Resolve the collision in terms of force
            // 4. Simulate motion
            // 5. Resolve motion-based collisions
        }
    }

    // Return an acceleration that will zero the passed-in velocity after integration
    public static Vector3 CounterAcceleration(Vector3 velocity, float dt)
    {
        // vf = vi + a * t
        // 0 = vi + a * t
        // -vi = a * t
        // -vi / t = a
        return (-velocity / dt);
    }

    public static float SeparationVelocity(Manifold manifold)
    {
        Vector3 relativeVelocity = manifold.body1.Velocity();
        if (!manifold.body2.Dynamic())
            relativeVelocity -= manifold.body2.Velocity();
        return Vector3.Dot(relativeVelocity, manifold.mtv.normal);
    }
}
