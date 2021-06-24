using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotManageSparse : PoolObj<DotManageSparse>
{
    public override void Init()
    {
        gameObject.SetActive(true);
    }

    public override void Sleep()
    {
        gameObject.SetActive(false);
    }

}
