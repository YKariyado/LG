using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameView : MonoBehaviour
{
    [SerializeField] GameObject head_pref; //head prefab
    [SerializeField] GameObject dot_pref; //dot prefab

    Dictionary<Tuple<int, int, int>, int> current_cell_list;

    public void InitPlayerPosition(float init_coordinate)
    {
        head_pref.transform.position = new Vector3(init_coordinate, init_coordinate, init_coordinate);
    }

    public void SetDotPrefab()
    {
        DotManageSparse.SetOriginal(dot_pref);
    }

    public void SetCurrentCellList(Dictionary<Tuple<int, int, int>, int> list)
    {
        current_cell_list = list;
    }

    public static void ViewUpdate()
    {
        
    }
}
