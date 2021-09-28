using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirRot : MonoBehaviour
{
    public GameObject camera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //this.gameObject.transform.rotation = camera.transform.rotation;
        this.gameObject.transform.rotation = Quaternion.LookRotation(Vector3.forward);
    }
}
