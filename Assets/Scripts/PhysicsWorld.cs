using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsWorld : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body body = Add(new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity);
            body.vel = new Vector3(angle, 10.0f, 0.0f);
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 acc = Physics.gravity;
        for (int i = 0; i < bodies.Count; i++)
        {
            bodies[i].Simulate(acc, dt);
        }
        if (Time.realtimeSinceStartup > 3.0f)
        {
            RemoveAll();
        }
    }

    public Body Add(Vector3 position, Quaternion rotation)
    {
        GameObject physicsObject = Instantiate(prefab, position, rotation);
        Body body = physicsObject.GetComponent<Body>();
        bodies.Add(body);
        return body;
    }

    public void Remove(Body body)
    {
        bodies.Remove(body);
        Destroy(body.gameObject);
    }

    public void RemoveAll()
    {
        for (int i = 0; i < bodies.Count; i++)
            Remove(bodies[i]);
    }

    List<Body> bodies = new List<Body>();
}
