using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Mtv
{
    public Vector3 normal = Vector3.zero;
    public float depth = 0.0f;
}

public class Manifold
{
    public Body body1;
    public Body body2;
    public Mtv mtv;
}

public class PhysicsWorld : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject planePrefab;
    Body plane;

    // Start is called before the first frame update
    void Start()
    {
        Init7();
    }

    void Init7()
    {
        for (float angle = -15.0f; angle < 15.0f; angle += 5.0f)
        {
            Body body = Add(spherePrefab, new Vector3(angle, 5.0f, 0.0f), Quaternion.identity);
            body.shape = new Sphere { radius = 0.5f };
            body.shape.type = ShapeType.SPHERE;
            body.dynamic = true;
        }

        plane = Add(planePrefab, Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 0.0f));
        plane.shape = new Plane { type = ShapeType.PLANE };
        plane.gravityScale = 0.0f;
        plane.dynamic = false;
    }

    private void FixedUpdate()
    {
        // Forces
        for (int i = 0; i < bodies.Count; i++)
            bodies[i].ApplyGravity(Physics.gravity);

        // Resolve collisions
        List<Manifold> collisions = DetectCollisions();
        for (int i = 0; i < collisions.Count; i++)
            ResolveCollision(collisions[i]);

        // Render
        DetectCollisions();
        for (int i = 0; i < bodies.Count; i++)
            SetColor(bodies[i], bodies[i].colliding ? Color.red : Color.green);

        // Kinematics
        for (int i = 0; i < bodies.Count; i++)
            bodies[i].Integrate(Time.fixedDeltaTime);
    }

    public List<Manifold> DetectCollisions()
    {
        List<Manifold> collisions = new List<Manifold>();
        for (int i = 0; i < bodies.Count; i++)
        {
            for (int j = i; j < bodies.Count; j++)
            {
                Mtv mtv = new Mtv();
                Body body1 = bodies[i];
                Body body2 = bodies[j];
                bool colliding = Collision.Check(body1, body2, mtv);
                if (colliding)
                {
                    Manifold manifold = new Manifold();
                    manifold.body1 = body1;
                    manifold.body2 = body2;
                    manifold.mtv = mtv;
                    collisions.Add(manifold);
                }
                body1.colliding = body2.colliding = colliding;
            }
        }
        return collisions;
    }

    public void ResolveCollision(Manifold manifold)
    {
        Body body1 = manifold.body1;
        Body body2 = manifold.body2;
        ShapeType shape1 = body1.shape.type;
        ShapeType shape2 = body2.shape.type;

        if (shape1 == ShapeType.SPHERE && shape2 == ShapeType.SPHERE)
        {
            Vector3 mtv1 = Vector3.zero;
            Vector3 mtv2 = Vector3.zero;
            if (body1.dynamic && body2.dynamic)
            {
                mtv1 = manifold.mtv.normal * manifold.mtv.depth * 0.5f;
                mtv2 = -manifold.mtv.normal * manifold.mtv.depth * 0.5f;
            }
            else if (body1.dynamic)
            {
                mtv1 = manifold.mtv.normal * manifold.mtv.depth;
            }
            else if (body2.dynamic)
            {
                mtv2 = -manifold.mtv.normal * manifold.mtv.depth;
            }

            body1.gameObject.transform.position += mtv1;
            body2.gameObject.transform.position += mtv2;
        }
        else
        {
            // TODO -- apply enough normal force to counter-act the velocity
            // if done correctly, we won't need to manually translate the position by normal * depth
            float depth = Mathf.Abs(manifold.mtv.depth);
            Vector3 normal = manifold.mtv.normal;
            if (shape1 == ShapeType.SPHERE && body1.dynamic)
            {
                Vector3 normalForce = Vector3.Reflect(body1.Force(), normal);
                body1.AddForce(normalForce);
                body1.gameObject.transform.position += normal * depth;
            }
            else if (shape2 == ShapeType.SPHERE && body2.dynamic)
            {
                Vector3 normalForce = Vector3.Reflect(body2.Force(), normal);
                body2.AddForce(normalForce);
                body2.gameObject.transform.position += normal * depth;
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

    void SetColor(Body body, Color color)
    {
        body.gameObject.GetComponent<Renderer>().material.color = color;
    }

    List<Body> bodies = new List<Body>();
}
