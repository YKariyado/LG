using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canvas_rot : MonoBehaviour
{
    public GameObject camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = camera.transform.forward * 10f;
        //find the vector pointing from our position to the target
        Vector3 _direction = (camera.transform.position - transform.position).normalized;

        //create the rotation we need to be in to look at the target
        Quaternion _lookRotation = Quaternion.LookRotation(_direction);

        //rotate us over time according to speed until we are in the required rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * 0.1f);
    }
}
