using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class Sphere
//{
//    Vector3 position;
//    float radius;
//}

public class Body : MonoBehaviour
{
    // Using transform.position since we're deriving from MonoBehaviour!
    //public Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 vel = new Vector3(0.0f, 0.0f, 0.0f);
    public float mass = 1.0f;
    public float drag = 0.0f;
    public float radius = 0.0f;

    public void Simulate(Vector3 acc, float dt)
    {
        vel = vel + acc * mass * dt;
        transform.position = transform.position + vel * dt;
    }

    public bool CheckCollisionSpheres(Body body)
    {
        float distance = (transform.position - body.transform.position).magnitude;
        return distance <= radius + body.radius;
    }
}
