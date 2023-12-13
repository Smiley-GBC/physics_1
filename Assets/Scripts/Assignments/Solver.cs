using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver : MonoBehaviour
{
    const float speed = 120.0f;
    const float gravity = 1.6f;

    void Start()
    {
        //Assignment1();
        Assignment2();
    }

    float Time(float vi, float theta, float g)
    {
        // t = 2vi * sin(theta) / g
        return 2.0f * vi * Mathf.Sin(theta) / g;
    }

    float Range(float vi, float theta, float g)
    {
        // x = (vi^2 * sin(2 * theta)) / g
        return ((vi * vi) * Mathf.Sin(theta * 2.0f)) / g;
    }

    float Angle(float range, float vi, float g)
    {
        // x = 0.5 * sin-1 (Rg / vi^2)
        return 0.5f * Mathf.Asin((range * g) / (vi * vi));
    }

    void Assignment1()
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

    void Assignment2()
    {
        const int count = 19;
        Vector2[] velocities = new Vector2[count];
        float[] times = new float[count];
        float[] ranges = new float[count];
        float[] angles = new float[count];
        angles[0] = 0.0f;
        angles[1] = 3.19f;
        angles[2] = 6.42f;
        angles[3] = 9.736f;
        angles[4] = 13.194f;
        angles[5] = 16.874f;
        angles[6] = 20.905f;
        angles[7] = 25.529f;
        angles[8] = 31.367f;
        angles[9] = 45.0f;
        angles[10] = 58.633f;
        angles[11] = 64.471f;
        angles[12] = 69.095f;
        angles[13] = 73.126f;
        angles[14] = 76.806f;
        angles[15] = 80.264f;
        angles[16] = 83.58f;
        angles[17] = 86.81f;
        angles[18] = 90.0f;

        // Range = "how far package lands"
        // Velocity = "how fast the package lands"
        // Time = "how long the package was in the air"
        for (int i = 0; i < count; i++)
        {
            // vf = -vi (motion in x stays constant but goes from positive to negative in y)
            float angle = angles[i] * Mathf.Deg2Rad;
            Vector2 vf = new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * speed;
            float time = Time(speed, angle, gravity);
            float range = Range(speed, angle, gravity);

            velocities[i] = vf;
            times[i] = time;
            ranges[i] = range;

            Debug.Log("Vf: " + vf);
            Debug.Log("Angle: " + angle * Mathf.Rad2Deg);
            Debug.Log("Time: " + time);
            Debug.Log("Range: " + range);
        }
    }
}
