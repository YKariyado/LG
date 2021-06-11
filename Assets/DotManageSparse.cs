using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManageSparse : PoolObj<DotManageSparse>
{
    //public int x, y, z;
    //public int state = 0; //num of own
    //public int neighbor = 0; //num of around

    //public bool isAlive;

    //// Update is called once per frame
    //void Update()
    //{
    //    DotManageSparse.Pool(this);
    //}

    public override void Init()
    {
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        gameObject.SetActive(false);
    }

}
