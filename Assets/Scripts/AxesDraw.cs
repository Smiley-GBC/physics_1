using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxesDraw : MonoBehaviour
{
    public float lineLength = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game started!");
    }

    // Update is called once per frame
    void Update()
    {
        // World-XYZ
        //Debug.DrawLine(transform.position, transform.position + new Vector3(lineLength, 0.0f, 0.0f), Color.red);
        //Debug.DrawLine(transform.position, transform.position + new Vector3(0.0f, lineLength, 0.0f), Color.green);
        //Debug.DrawLine(transform.position, transform.position + new Vector3(0.0f, 0.0f, lineLength), Color.blue);

        // Local-XYZ
        Debug.DrawLine(transform.position, transform.position + transform.right * lineLength, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.up * lineLength, Color.green);
        Debug.DrawLine(transform.position, transform.position + transform.forward * lineLength, Color.blue);
    }
}
