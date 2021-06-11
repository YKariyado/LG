using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charcontroller : MonoBehaviour
{
    public float speed;
    void Start()
    {
    }

    void Update()
    {
        Transform trans = transform;
        transform.position = trans.position;
        trans.position += trans.TransformDirection(Vector3.forward) * Input.GetAxis("Vertical") * speed;
        trans.position += trans.TransformDirection(Vector3.right) * Input.GetAxis("Horizontal") * speed;
    }
}
