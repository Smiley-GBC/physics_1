using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Particle
{
    public Vector3 pos;
    public Vector3 vel;

    public float gravityScale;
    public float mass;

    public Collider collider;

    // Works in progress:
    public float friction;
    public float restitution;
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

public class PhysicsWorld
{
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
        mVelocities.Add(p.vel);

        // If the user wants force, they must call AddForce to accumulate acceleration
        mAccelrations.Add(Vector3.zero);
        mNetForces.Add(Vector3.zero);

        mInvMasses.Add(p.collider.dynamic ? 1.0f / p.mass : 0.0f);
        mGravityScales.Add(p.gravityScale);

        mColliders.Add(p.collider);
    }

    public void Remove(GameObject obj)
    {
        // TODO -- swap key-value pair with last element
        int index = links[obj];

        mPositions.RemoveAt(index);
        mVelocities.RemoveAt(index);
        mAccelrations.RemoveAt(index);

        mNetForces.RemoveAt(index);
        mInvMasses.RemoveAt(index);
        mGravityScales.RemoveAt(index);

        mColliders.RemoveAt(index);
        links.Remove(obj);
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
        // 1. Accumulate applied force (user input)
        for (int i = 0; i < mAccelrations.Count; i++)
            mAccelrations[i] = mNetForces[i] * mInvMasses[i];

        // 2. Accumnulate gravitational force
        for (int i = 0; i < mAccelrations.Count; i++)
            mAccelrations[i] += gravity * mGravityScales[i];

        //List<Vector3> velocities = new List<Vector3>(mVelocities);
        //List<Vector3> positions = new List<Vector3>(mPositions);
        //Dynamics.Integrate(velocities, mAccelrations, dt);
        //Dynamics.Integrate(positions, velocities, dt);
        //
        //List<Manifold> collisions = Collision.Collisions(positions, mColliders);

        // (Pure-Force engine would run its 2nd step here instead of continuing integration)
        Dynamics.Integrate(mVelocities, mAccelrations, dt);
        Dynamics.Integrate(mPositions, mVelocities, dt);

        // Resolve penetration
        Collision.ResolvePositions(mPositions, mColliders, Collision.Collisions(mPositions, mColliders));

        // Reset net forces
        for (int i = 0; i < mNetForces.Count; i++)
            mNetForces[i] = Vector3.zero;
    }

    List<Vector3> mPositions = new List<Vector3>();
    List<Vector3> mVelocities = new List<Vector3>();
    List<Vector3> mAccelrations = new List<Vector3>();

    List<Vector3> mNetForces = new List<Vector3>();
    List<float> mInvMasses = new List<float>();
    List<float> mGravityScales = new List<float>();

    List<Collider> mColliders = new List<Collider>();

    // Gotta put everything in the same file if I want my typedefs...
    public static class Dynamics
    {
        public static void Integrate(List<Vector3> output, List<Vector3> input, float dt)
        {
            for (int i = 0; i < input.Count; i++)
            {
                output[i] += input[i] * dt;
            }
        }

        // a = F / m
        //public static Lf3 Accelerations(Lf3 netForces, Lf1 inverseMasses)
        //{
        //    Lf3 accelerations = new Lf3(netForces.Count);
        //    for (int i = 0; i < accelerations.Count; i++)
        //        accelerations[i] = netForces[i] * inverseMasses[i];
        //    return accelerations;
        //}
    }

    // Essentially the same as Collision, but as an inner class of PhysicsWorld
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
                    if (Collision.Check(positions[i], colliders[i], positions[j], colliders[j], mtv))
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

        public static bool SphereSphere(
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

        public static bool SpherePlane(
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
