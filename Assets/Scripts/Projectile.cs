using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float launchVelocity;
    public float launchAngle;
    public float launchHeight;

    private float velX = 0.0f;  // Doesn't change between frames
    private float velY = 0.0f;  // Under constant acceleration (gravity)

    void Start()
    {
        LaunchBall();
    }

    void LaunchBall()
    {
        // 1. Update velX and velY based on horizontal and vertical components of launch velocity & launch angle
        // 2. Assign position to new launch height and re-launch the ball!
        velX = launchVelocity * Mathf.Cos(launchAngle * Mathf.Deg2Rad);
        velY = launchVelocity * Mathf.Sin(launchAngle * Mathf.Deg2Rad);
        transform.position = new Vector3(0.0f, launchHeight, 0.0f);
    }

    void FixedUpdate()
    {
        // Restart the launch
        if (Input.GetKey(KeyCode.Space))
        {
            LaunchBall();
        }

        float dt = Time.fixedDeltaTime;
        float acc = Physics.gravity.y;

        // Update velocity using acceleration over time (velX remains constant)
        velY = velY + acc * dt;

        // Update position using velocity over time
        transform.position = new Vector3(
            transform.position.x + velX * dt,
            transform.position.y + velY * dt,
            transform.position.z
        );
    }
}
