using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Particle
{
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 acc;

    public float gravityScale;
    public float mass;
    public float friction;
    public float restitution;

    public Collider collider;
}

public class Collider
{
    public Shape shape;     // SPHERE or PLANE
    public float radius;    // if SPHERE, radius will have a value
    public Vector3 normal;  // if PLANE,  normal will have a value
    public bool dynamic;
}

public enum Shape
{
    SPHERE,
    PLANE
}

public class Manifold
{
    public int a = -1;
    public int b = -1;
    public Mtv mtv = null;
}

public class Mtv
{
    public Vector3 normal;
    public float depth;
}

public enum Integrator
{
    EULER,
    VERLET
}

// TODO -- if I want multiple integrators, then I'll have to make an interface to account for
// different methods of manipulating velocity --> Euler = velocity, Verlet = (pos - pos0) / dt
// If I want re-use between ResolveVelocities and ResolvePositions, then I need to use interface
public class PhysicsWorld
{
    public Integrator integrator;
    Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);
    float timestep = 1.0f / 50.0f;
    float prevTime = 0.0f;
    float currTime = 0.0f;

    // Key is GameObject hash
    // Val is index of physics properties
    Dictionary<GameObject, int> links = new Dictionary<GameObject, int>();

    public void Add(GameObject obj, Particle p)
    {
        links.Add(obj, links.Count);

        mPositions.Add(p.pos);
        mPositions0.Add(p.pos);

        mVelocities.Add(p.vel);
        if (integrator == Integrator.VERLET)
        {
            mPositions0[links[obj]] = mPositions[links[obj]] - (p.vel * timestep);
            mVelocities[links[obj]] = Vector3.zero;
        }

        mAccelrations.Add(p.acc);
        mNetForces.Add(Vector3.zero);

        mInvMasses.Add(p.collider.dynamic ? 1.0f / p.mass : 0.0f);
        mGravityScales.Add(p.gravityScale);
        mFrictions.Add(p.friction);
        mRestitutions.Add(p.restitution);

        mColliders.Add(p.collider);
    }

    public void Remove(GameObject obj)
    {
        // TODO -- swap key-value pair with last element
        int index = links[obj];
        links.Remove(obj);

        mPositions.RemoveAt(index);
        mPositions0.RemoveAt(index);
        mVelocities.RemoveAt(index);
        mAccelrations.RemoveAt(index);
        mNetForces.RemoveAt(index);

        mInvMasses.RemoveAt(index);
        mGravityScales.RemoveAt(index);
        mFrictions.RemoveAt(index);
        mRestitutions.RemoveAt(index);

        mColliders.RemoveAt(index);
    }

    // Simulate physics at the frequency of timestep (aka run FixedUpdate)
    public void Update(float dt)
    {
        while (prevTime < currTime)
        {
            prevTime += timestep;
            Simulate(timestep);
        }
        currTime += dt;
    }

    // Write collider geometry from Unity to Engine
    public void PreUpdate()
    {
        foreach (var pair in links)
        {
            Collider collider = mColliders[pair.Value];
            switch (collider.shape)
            {
                case Shape.SPHERE:
                    collider.radius = pair.Key.transform.localScale.x * 0.5f;
                    break;

                case Shape.PLANE:
                    collider.normal = pair.Key.transform.up;
                    break;
            }
        }
    }

    // Write position from Engine to Unity and update color based on collision status
    public void PostUpdate()
    {
        List<bool> collisions = new List<bool>(new bool[mPositions.Count]);
        for (int i = 0; i < mPositions.Count; i++)
        {
            for (int j = i + 1; j < mPositions.Count; j++)
            {
                collisions[i] |= collisions[j] |= Collision.Check(
                    mPositions[i], mColliders[i], mPositions[j], mColliders[j]);
            }
        }

        foreach (var pair in links)
        {
            GameObject obj = pair.Key;
            Color color = collisions[pair.Value] ? Color.red : Color.green;
            obj.GetComponent<Renderer>().material.color = color;
            obj.transform.position = mPositions[pair.Value];
        }
    }

    void Simulate(float dt)
    {
        if (integrator == Integrator.EULER)
        {
            // 1. Apply motion
            for (int i = 0; i < mNetForces.Count; i++)
            {
                // Apply user force
                mAccelrations[i] = mNetForces[i] * mInvMasses[i];

                // Apply gravitational force
                mAccelrations[i] += gravity * mGravityScales[i];

                // Apply acceleration to velocity
                mVelocities[i] = Euler.Integrate(mVelocities[i], mAccelrations[i], dt);

                // Apply velocity to position
                mPositions[i] = Euler.Integrate(mPositions[i], mVelocities[i], dt);

                // Reset net force
                mNetForces[i] = Vector3.zero;
            }

            // 2. Correct motion & position
            List<Manifold> collisions = Collision.Collisions(mPositions, mColliders);
            Euler.ResolveVelocities(mVelocities, mInvMasses, mFrictions, mRestitutions, collisions);
            Euler.ResolvePositions(mPositions, mColliders, collisions);
        }
        else
        {
            // 1. Apply motion
            for (int i = 0; i < mNetForces.Count; i++)
            {
                // Apply user force
                mAccelrations[i] = mNetForces[i] * mInvMasses[i];

                // Apply gravitational force
                mAccelrations[i] += gravity * mGravityScales[i];

                // Integrate displacement & acceleration, then derive velocity
                Verlet.ApplyMotion(mPositions, mPositions0, mVelocities, mAccelrations, i, dt);

                // Reset net force
                mNetForces[i] = Vector3.zero;
            }

            // TODO -- Make verlet-based velocity & position resolution algorithms
            List<Manifold> collisions = Collision.Collisions(mPositions, mColliders);
            Euler.ResolveVelocities(mVelocities, mInvMasses, mFrictions, mRestitutions, collisions);
            Euler.ResolvePositions(mPositions, mColliders, collisions);
        }
    }

    List<Vector3> mPositions = new List<Vector3>();     // Current positions
    List<Vector3> mPositions0 = new List<Vector3>();    // Previous positions

    List<Vector3> mVelocities = new List<Vector3>();
    List<Vector3> mAccelrations = new List<Vector3>();
    List<Vector3> mNetForces = new List<Vector3>();

    List<float> mInvMasses = new List<float>();
    List<float> mGravityScales = new List<float>();
    List<float> mFrictions = new List<float>();
    List<float> mRestitutions = new List<float>();

    List<Collider> mColliders = new List<Collider>();

    public static class Euler
    {
        // "Compute the rate of change"
        // Ex: solve for v given pi = 5m, pf = 10m, and t = 2s
        // v = (pf - pi) / t --> v = (10 - 5) / 2 --> v = 2.5m/s
        public static Vector3 Differentiate(Vector3 final, Vector3 initial, float dt)
        {
            return (final - initial) / dt;
        }

        // "Apply the rate of change"
        // Ex: solve for pf given pi = 5m, vi = 2.5m/s, and t = 2s 
        // pf = pi + vi * t --> pf = 5 + 2.5 * 2 --> pf = 10m
        public static Vector3 Integrate(Vector3 value, Vector3 change, float dt)
        {
            return value + change * dt;
        }

        public static void ResolveVelocities(
            List<Vector3> velocities, List<float> invMasses,
            List<float> frictions, List<float> restitutions,
            List<Manifold> collisions)
        {
            foreach (Manifold collision in collisions)
            {
                // Exit if both objects are static
                float invMassSum = invMasses[collision.a] + invMasses[collision.b];
                if (invMassSum <= Mathf.Epsilon) continue;

                // Exit if both objects are moving away from each other
                Vector3 vBA = velocities[collision.a] - velocities[collision.b];
                float t = Vector3.Dot(vBA, collision.mtv.normal);
                if (t > 0.0f) continue;

                // Apply impulse to velocities
                float restitution = Mathf.Min(restitutions[collision.a], restitutions[collision.b]);
                float impulseMagnitude = -(1.0f + restitution) * t / invMassSum;
                Vector3 impulse = collision.mtv.normal * impulseMagnitude;
                velocities[collision.a] += impulse * invMasses[collision.a];
                velocities[collision.b] -= impulse * invMasses[collision.b];

                // Scale friction based on how similar the relative velocity is to the collision normal
                Vector3 frictionDirection = (vBA - (collision.mtv.normal * t)).normalized;
                float frictionMagnitude = -Vector3.Dot(vBA, frictionDirection) / invMassSum;

                // Coulomb's Law
                float mu = Mathf.Sqrt(frictions[collision.a] * frictions[collision.b]);
                frictionMagnitude = Mathf.Clamp(frictionMagnitude,
                    -impulseMagnitude * mu, impulseMagnitude * mu);

                // Apply friction to velocities
                Vector3 friction = frictionMagnitude * frictionDirection;
                velocities[collision.a] += friction * invMasses[collision.a];
                velocities[collision.b] -= friction * invMasses[collision.b];
            }
        }

        public static void ResolvePositions(List<Vector3> positions, List<Collider> colliders,
            List<Manifold> collisions)
        {
            foreach (Manifold collision in collisions)
            {
                Vector3 mtv = collision.mtv.normal * collision.mtv.depth;
                if (colliders[collision.b].dynamic)
                {
                    positions[collision.a] += mtv * 0.5f;
                    positions[collision.b] -= mtv * 0.5f;
                }
                else
                {
                    positions[collision.a] += mtv;
                }
            }
        }
    }

    public static class Verlet
    {
        public static void ApplyMotion(List<Vector3> positions, List<Vector3> positions0,
            List<Vector3> velocities, List<Vector3> accelerations, int i, float dt)
        {
            Vector3 displacement = positions[i] - positions0[i];
            positions0[i] = positions[i];
            positions[i] = positions[i] + displacement + accelerations[i] * dt * dt;
            velocities[i] = displacement / dt;
        }

        // TODO -- ResolveVelocities
        // TODO -- ResolvePositions
    }

    public static class Collision
    {
        public static List<Manifold> Collisions(List<Vector3> positions, List<Collider> colliders)
        {
            List<Manifold> collisions = new List<Manifold>();
            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = i + 1; j < positions.Count; j++)
                {
                    Mtv mtv = new Mtv();
                    if (Check(positions[i], colliders[i], positions[j], colliders[j], mtv))
                    {
                        if (!colliders[i].dynamic && colliders[j].dynamic)
                        {
                            mtv.normal *= 1.0f;
                            collisions.Add(new Manifold() { a = j, b = i, mtv = mtv });
                        }
                        else
                            collisions.Add(new Manifold() { a = i, b = j, mtv = mtv });
                    }
                }
            }
            return collisions;
        }

        public static bool Check(
            Vector3 position1, Collider collider1, Vector3 position2, Collider collider2, Mtv mtv = null)
        {
            if (collider1.shape == Shape.SPHERE && collider2.shape == Shape.SPHERE)
                return SphereSphere(position1, collider1.radius, position2, collider2.radius, mtv);

            if (collider1.shape == Shape.SPHERE && collider2.shape == Shape.PLANE)
                return SpherePlane(position1, collider1.radius, position2, collider2.normal, mtv);

            if (collider1.shape == Shape.PLANE && collider2.shape == Shape.SPHERE)
                return SpherePlane(position2, collider2.radius, position1, collider2.normal, mtv);

            return false;
        }

        static bool SphereSphere(
            Vector3 position1, float radius1, Vector3 position2, float radius2, Mtv mtv = null)
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

        static bool SpherePlane(
            Vector3 spherePosition, float sphereRadius, Vector3 planePosition, Vector3 planeNormal, Mtv mtv = null)
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
    }
}
