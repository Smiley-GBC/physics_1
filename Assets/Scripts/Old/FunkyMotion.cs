using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunkyMotion : MonoBehaviour
{
    // a = frequency, b = amplitude
    public float frequency = 1.0f;
    public float amplitude = 3.0f;
    public float t = 0.0f;
    float x = 0.0f;
    float y = 0.0f;

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        x = x + (-Mathf.Sin(t * frequency) * frequency * amplitude * dt);
        y = y + (-Mathf.Cos(t * frequency) * frequency * amplitude * dt);
        transform.position = new Vector3(x, y, transform.position.z);

        t = Time.realtimeSinceStartup;
        //t = t + dt;
    }
}
