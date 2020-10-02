using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManage : MonoBehaviour
{

    public int x, y, z;
    public int state = 0; //num of own
    public int neighbor = 0; //num of around

    public bool isAlive = false;

    public void dotGenerate()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        isAlive = true;
    }

    public void dotDestroy()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        isAlive = false;
    }
}
