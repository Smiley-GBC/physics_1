using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBody : Body
{
    // Homework: Incorporate launch position, launch speed, and launch angle into our new custom-physics system!
    // Plot 3D projectile motion by specifying a pitch (launch-angle about X) and yaw (launch-angle about Y)
    public Vector3 launchPosition;
    public float launchSpeed;
    public float launchPitch = 0.0f;
    public float launchYaw = 0.0f;

    void Launch()
    {
        // TODO -- Decompose pitch and yaw into velocity x, y & z!
        // TODO -- Add controls to change the direction of the acceleration (key input or GUI slider)

        // TODO -- Add drag (air resistance) to the simulation (similar to exercise 2 where we dampened the velocity on-bounce)
        // ex: multiplying by 0.99 gives a little air resistance, multiplying by 0.01 gives a lot of air resistance!

        // TODO -- Add terminal velocity; constrain your velocity so its magnitude cannot exceed a certain threshold
        // ex: object cannot travel faster than 500m/s (if speed > 500, speed = 500)
    }

    private void Start()
    {
        Launch();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Launch();
        }
        Simulate(Physics.gravity, Time.fixedDeltaTime);
    }
}
