using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Lf3 = System.Collections.Generic.List<UnityEngine.Vector3>;
using Lf1 = System.Collections.Generic.List<float>;

public struct Particle
{
    public Vector3 pos;
    public Vector3 vel;

    public float gravityScale;
    public float mass;
    public bool moves;

    public Collider collider;

    // Works in progress:
    public float friction;
    public float restitution;
}

public class Collider
{
    public Shape2 shape;    // SPHERE or PLANE
    public float radius;    // if SPHERE, radius will have a value
    public Vector3 normal;  // if PLANE,  normal will have a value
}

public enum Shape2
{
    SPHERE,
    PLANE
}

public class Manifold2
{
    public int a = -1;
    public int b = -1;
    public Mtv mtv = null;
}

public class PhysicsWorld2
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

        mInvMasses.Add(p.moves ? 1.0f / p.mass : 0.0f);
        mGravityScales.Add(p.gravityScale);
        mMovements.Add(p.moves);

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
        mMovements.RemoveAt(index);

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
                case Shape2.SPHERE:
                    collider.radius = pair.Key.transform.localScale.x * 0.5f;
                    break;

                case Shape2.PLANE:
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
                collisions[i] |= collisions[j] |= Collision2.Check(
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
            mAccelrations[i] += gravity * mInvMasses[i] * mGravityScales[i];

        // (Pure-Force engine would run its 2nd step here instead of continuing integration)
        Dynamics2.Integrate(mVelocities, mAccelrations, dt);
        Dynamics2.Integrate(mPositions, mVelocities, dt);

        // Resolve penetration
        List<Manifold2> collisions = Collisions(0, mPositions.Count);
        ResolvePositions(collisions);

        // Reset net forces
        for (int i = 0; i < mNetForces.Count; i++)
            mNetForces[i] = Vector3.zero;
    }

    List<Manifold2> Collisions(int start, int end)
    {
        List<Manifold2>collisions = new List<Manifold2>();
        for (int i = start; i < end; i++)
        {
            for (int j = start + 1; j < end; j++)
            {
                Mtv mtv = new Mtv();
                if (Collision2.Check(mPositions[i], mColliders[i], mPositions[j], mColliders[j], mtv))
                {
                    if (!mMovements[i] && mMovements[j])
                    {
                        mtv.normal *= 1.0f;
                        collisions.Add(new Manifold2() { a = j, b = i, mtv = mtv });
                    }
                    else
                        collisions.Add(new Manifold2() { a = i, b = j, mtv = mtv });
                }
            }
        }
        return collisions;
    }

    void ResolvePositions(List<Manifold2> collisions)
    {
        foreach (Manifold2 collision in collisions)
        {
            Vector3 mtv = collision.mtv.normal * collision.mtv.depth;
            if (mMovements[collision.b])
            {
                mPositions[collision.a] += mtv * 0.5f;
                mPositions[collision.b] -= mtv * 0.5f;
            }
            else
            {
                mPositions[collision.a] += mtv;
            }
        }
    }

    /*
    public static List<Manifold> DetectCollisions(List<Body> bodies)
    {
        List<Manifold> collisions = new List<Manifold>();
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i + 1; j < bodies.Count; j++)
            {
                Mtv mtv = new Mtv();
                if (Check(bodies[i], bodies[j], mtv))
                {
                    Body body1 = bodies[i];
                    Body body2 = bodies[j];
                    if (!body1.Dynamic() && body2.Dynamic())
                    {
                        mtv.normal *= -1.0f;
                        collisions.Add(new Manifold() { body2 = body1, body1 = body2, mtv = mtv });
                    }
                    else
                    {
                        collisions.Add(new Manifold() { body1 = body1, body2 = body2, mtv = mtv });
                    }
                }
            }
        }
        return collisions;
    }
    */

    Lf3 mPositions = new Lf3();
    Lf3 mVelocities = new Lf3();
    Lf3 mAccelrations = new Lf3();

    Lf3 mNetForces = new Lf3();
    Lf1 mInvMasses = new Lf1();
    Lf1 mGravityScales = new Lf1();
    List<bool> mMovements = new List<bool>();

    List<Collider> mColliders = new List<Collider>();

    // Gotta put everything in the same file if I want my typedefs...
    public static class Dynamics2
    {
        public static void Integrate(Lf3 output, Lf3 input, float dt)
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
    public static class Collision2
    {
        public static bool Check(
            Vector3 position1, Collider collider1, Vector3 position2, Collider collider2, Mtv mtv = null)
        {
            if (collider1.shape == Shape2.SPHERE && collider2.shape == Shape2.SPHERE)
                return SphereSphere(position1, collider1.radius, position2, collider2.radius, mtv);

            if (collider1.shape == Shape2.SPHERE && collider2.shape == Shape2.PLANE)
                return SpherePlane(position1, collider1.radius, position2, collider2.normal, mtv);

            if (collider1.shape == Shape2.PLANE && collider2.shape == Shape2.SPHERE)
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
