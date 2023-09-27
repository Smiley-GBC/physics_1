using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    // Using transform.position since we're deriving from MonoBehaviour!
    //public Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vel = new Vector3(0.0f, 0.0f, 0.0f);
    public float mass = 1.0f;
    public float drag = 0.0f;

    public void Simulate(Vector3 acc, float dt)
    {
        vel = vel + acc * mass * dt;
        transform.position = transform.position + vel * dt;
    }
}
