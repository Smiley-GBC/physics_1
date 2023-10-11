using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityCollisionTest : MonoBehaviour
{
    void Start()
    {
        GetComponent<Renderer>().material.color = Color.green;
    }

    // Called once per collision.
    // For example, if a sphere A is colliding with a plane P and another sphere B,
    // Then this function will be invoked once where collision = B and once where collision = P.
    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<Renderer>().material.color = Color.red;
    }

    private void OnCollisionExit(Collision collision)
    {
        GetComponent<Renderer>().material.color = Color.green;
    }
}
