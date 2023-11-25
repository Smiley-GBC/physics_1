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

    // Write position from Engine to Unity
    public void PostUpdate()
    {
        foreach (var pair in links)
            pair.Key.transform.position = mPositions[pair.Value];
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
    }

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
}
