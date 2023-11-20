using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver : MonoBehaviour
{
    const float speed = 120.0f;
    const float gravity = 1.6f;

    void Start()
    {
        // Question 1 a, c, and d:
        float angle1 = 20.0f * Mathf.Deg2Rad;
        float angle2 = 70.0f * Mathf.Deg2Rad;
        float range1 = Range(speed, angle1, gravity);
        float range2 = Range(speed, angle2, gravity);
        Vector2 velocity = new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1)) * speed;
        Debug.Log("1a: " + velocity);
        Debug.Log("1c: " + range1);
        Debug.Log("1d: " + range2);
        Debug.Log("Ranges are identical because both angles are 20 degrees from horizontal/vertical axes");

        // Questions 2 and 3:
        for (float range = 1000.0f; range <= 10000.0f; range += 1000.0f)
            Debug.Log(Angle(range, speed, gravity) * Mathf.Rad2Deg);
    }

    float Range(float vi, float theta, float g)
    {
        // x = (vi^2 * sin(2 * theta)) / g
        return ((vi * vi) * Mathf.Sin(theta * 2.0f)) / g;
    }

    float Angle(float range, float vi, float g)
    {
        // x = 0.5sin-1 (Rg / vi^2)
        return 0.5f * Mathf.Asin((range * gravity) / (vi * vi));
    }
}
