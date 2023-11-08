using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair
{
    public Body body1;
    public Body body2;
}

public class PhysicsWorld : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject planePrefab;
    Body plane;

    // Start is called before the first frame update
    void Start()
    {
        //Init6();
        Init7();
    }

    void Init6()
    {
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body body = Add(spherePrefab, new Vector3(angle, 2.5f, 0.0f), Quaternion.identity);
            body.vel = new Vector3(0.0f, 0.0f, 0.0f);
            body.shape = new Sphere { radius = 0.5f };
            body.shape.type = ShapeType.SPHERE;
            body.gravityScale = 0.0f;
        }

        plane = Add(planePrefab, new Vector3(0.0f, 5.0f, 0.0f), Quaternion.identity);
        plane.shape = new Plane { type = ShapeType.PLANE };
        plane.gravityScale = 0.0f;
    }

    void Init7()
    {
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body body = Add(spherePrefab, new Vector3(angle, 5.0f, 0.0f), Quaternion.identity);
            body.vel = new Vector3(0.0f, 0.0f, 0.0f);
            body.shape = new Sphere { radius = 0.5f };
            body.shape.type = ShapeType.SPHERE;
            body.gravityScale = 0.1f;
            body.dynamic = true;
        }

        //Body body = Add(spherePrefab, new Vector3(0.0f, 5.0f, 0.0f), Quaternion.identity);
        //body.vel = new Vector3(0.0f, 0.0f, 0.0f);
        //body.shape = new Sphere { radius = 0.5f };
        //body.shape.type = ShapeType.SPHERE;
        //body.gravityScale = 0.1f;
        //body.dynamic = true;

        plane = Add(planePrefab, Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 45.0f));
        plane.shape = new Plane { type = ShapeType.PLANE };
        plane.gravityScale = 0.0f;
        plane.dynamic = false;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 acc = Physics.gravity;

        List<Pair> overlapping = new List<Pair>();
        for (int i = 0; i < bodies.Count; i++)
        {
            // Reset all objects to green every frame
            bodies[i].gameObject.GetComponent<Renderer>().material.color = Color.green;

            for (int j = 0; j < bodies.Count; j++)
            {
                // Don't check the same object against itself
                if (i == j) continue;

                // Reads better if we append all overlapping pairs, then resolve later on
                Body bodyA = bodies[i];
                Body bodyB = bodies[j];

                if (bodyA.CheckCollision(bodyB))
                {
                    Pair pair = new Pair();
                    pair.body1 = bodyA;
                    pair.body2 = bodyB;
                    overlapping.Add(pair);
                }
            }
            bodies[i].Simulate(acc, dt);
        }

        // Color overlapping pairs red
        for (int i = 0; i < overlapping.Count; i++)
        {
            overlapping[i].body1.gameObject.GetComponent<Renderer>().material.color = Color.red;
            overlapping[i].body2.gameObject.GetComponent<Renderer>().material.color = Color.red;
            ResolveCollisions(overlapping[i].body1.gameObject, overlapping[i].body2.gameObject);
        }
    }

    public void ResolveCollisions(GameObject obj1, GameObject obj2)
    {
        Vector3 position1 = obj1.transform.position;
        Vector3 position2 = obj2.transform.position;
        Body body1 = obj1.GetComponent<Body>();
        Body body2 = obj2.GetComponent<Body>();
        ShapeType shape1 = body1.shape.type;
        ShapeType shape2 = body2.shape.type;
        float radius1 = shape1 == ShapeType.SPHERE ? ((Sphere)body1.shape).radius : 0.0f;
        float radius2 = shape2 == ShapeType.SPHERE ? ((Sphere)body2.shape).radius : 0.0f;

        if (shape1 == ShapeType.SPHERE && shape2 == ShapeType.SPHERE)
        {
            Vector3 normal = Vector3.zero;
            float depth = 0.0f;
            Body.ResolveSpheres(position1, radius1, position2, radius2, out normal, out depth);

            Vector3 mtv1 = Vector3.zero;
            Vector3 mtv2 = Vector3.zero;
            if (body1.dynamic && body2.dynamic)
            {
                mtv1 = normal * depth * 0.5f;
                mtv2 = -normal * depth * 0.5f;
            }
            else if (body1.dynamic)
            {
                mtv1 = normal * depth;
            }
            else if (body2.dynamic)
            {
                mtv2 = -normal * depth;
            }

            obj1.transform.position += mtv1;
            obj2.transform.position += mtv2;
        }
        else
        {
            Vector3 normal = Vector3.zero;
            float depth = 0.0f;
            if (shape1 == ShapeType.SPHERE && body1.dynamic)
            {
                depth = Mathf.Abs(Body.ResolveSpherePlane(position1, radius1, position2, obj2.transform.up));
                normal = obj2.transform.up;
                obj1.transform.position += normal * Mathf.Abs(depth);
            }
            if (shape2 == ShapeType.SPHERE && body2.dynamic)
            {
                depth = Mathf.Abs(Body.ResolveSpherePlane(position2, radius2, position1, obj1.transform.up));
                normal = obj1.transform.up;
                obj2.transform.position += normal * depth;
            }
        }
    }

    public Body Add(GameObject prefab, Vector3 position, Quaternion rotation)
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
