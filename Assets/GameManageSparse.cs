using System.Globalization;
using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.IO;
using System.Text;
using SimpleFileBrowser;
using System.Linq;
using System.Threading.Tasks;

public class GameManageSparse : MonoBehaviour
{
    [SerializeField] int n = 2048; //infinite universe
    [SerializeField] int r1 = 4, r2 = 4, r3 = 0, r4 = 0; //rules
    [SerializeField] int range; //apprear range
    int pre_x, pre_y, pre_z;
    Vector3 head_location; //change pos to location

    [SerializeField] GameObject head_pref; //head prefab flag_
    [SerializeField] GameObject dot_pref; //dot prefab flag_

    // the list of dots that's displaying now
    // I need this when I delete all cells in a scene
    List<DotManageSparse> displaying_dots_list = new List<DotManageSparse>();

    Dictionary<Tuple<int, int, int>, int> current_cell_list = new Dictionary<Tuple<int, int, int>, int>();
    Dictionary<Tuple<int, int, int>, int> cell_list_for_judge = new Dictionary<Tuple<int, int, int>, int>();

    // if cell_location_matrix[x,y,z] == 1, the cell near by the player will be appeared
    Sparse3DArray<int> cell_location_matrix = new Sparse3DArray<int>();
    Sparse3DArray<int> pre_cell_location_matrix;

    float dotInterval = 1;
    public float bpm;
    //These are not constant.
    float BAR, BEAT;
    //every_bar is the time to refresh model with chords, every_beat is the time to refresh model with sequential.
    float every_bar = 1, every_beat = 0;

    bool isRun = true, isPeriodic = true, isSequential = false;

    // Awake is called before Start
    void Awake()
    {
        DotManageSparse.SetOriginal(dot_pref); //flag_
    }

    // Start is called before the first frame update
    void Start()
    {
        //setting head_pref position flag_
        head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);

        //Random Debug Preset
        for (int i = n / 2 - 4; i < n / 2 + 4; i++)
        {
            for (int j = n / 2 - 4; j < n / 2 + 4; j++)
            {
                for (int k = n / 2 - 4; k < n / 2 + 4; k++)
                {
                    if (UnityEngine.Random.Range(0, 5) == 0)
                    {
                        cell_location_matrix[i, j, k] = 1;
                        var key1 = new Tuple<int, int, int>(i, j, k);
                        current_cell_list.Add(key1, 0);
                    }
                }
            }
        }


        // //Blinker Debug Preset
        // cell_location_matrix[n / 2, n / 2, n / 2] = 1;
        // var key1 = new Tuple<int, int, int>(n / 2, n / 2, n / 2);
        // current_cell_list.Add(key1, 0);

        // cell_location_matrix[n / 2 + 1, n / 2, n / 2] = 1;
        // var key2 = new Tuple<int, int, int>(n / 2 + 1, n / 2, n / 2);
        // current_cell_list.Add(key2, 0);

        // cell_location_matrix[n / 2, n / 2 + 1, n / 2 + 1] = 1;
        // var key3 = new Tuple<int, int, int>(n / 2, n / 2 + 1, n / 2 + 1);
        // current_cell_list.Add(key3, 0);

        // cell_location_matrix[n / 2 + 1, n / 2 + 1, n / 2 + 1] = 1;
        // var key4 = new Tuple<int, int, int>(n / 2 + 1, n / 2 + 1, n / 2 + 1);
        // current_cell_list.Add(key4, 0);


        // //Rocket Debug Preset
        // cell_location_matrix[n / 2, n / 2, n / 2] = 1;
        // var key1 = new Tuple<int, int, int>(n / 2, n / 2, n / 2);
        // current_cell_list.Add(key1, 0);

        // cell_location_matrix[n / 2, n / 2, n / 2 + 1] = 1;
        // var key2 = new Tuple<int, int, int>(n / 2, n / 2, n / 2 + 1);
        // current_cell_list.Add(key2, 0);

        // cell_location_matrix[n / 2, n / 2 + 1, n / 2] = 1;
        // var key3 = new Tuple<int, int, int>(n / 2, n / 2 + 1, n / 2);
        // current_cell_list.Add(key3, 0);

        // cell_location_matrix[n / 2, n / 2 + 1, n / 2 + 1] = 1;
        // var key4 = new Tuple<int, int, int>(n / 2, n / 2 + 1, n / 2 + 1);
        // current_cell_list.Add(key4, 0);

    }

    // using 'await' because tasks have to wait its finish 
    async Task Update()
    {
        //Moving the head
        head_location = head_pref.transform.position;
        head_location.x = Mathf.Clamp(head_location.x, (-n / 2) + range, (n / 2) - range);
        head_location.y = Mathf.Clamp(head_location.y, (-n / 2) + range, (n / 2) - range);
        head_location.z = Mathf.Clamp(head_location.z, (-n / 2) + range, (n / 2) - range);
        head_pref.transform.position = new Vector3(head_location.x, head_location.y, head_location.z);

        BAR = 4f / (bpm / 60f);
        BEAT = 1f / ((bpm / 60f) * 2f);

        if (isRun)
        {
            every_beat += Time.deltaTime;

            //Store previous location of the head.
            pre_x = (int)head_location.x;
            pre_y = (int)head_location.y;
            pre_z = (int)head_location.z;

            //View update
            //Updates the view when the player's position changes in integer increments
            if (every_bar == 0)
            {
                every_bar++;
                await Task.Run(() =>
                {
                    //add cells to alives and deads
                    foreach (var e in current_cell_list)
                    {
                        //add 1 to each adjacency cell
                        for (int _i = -1; _i < 2; _i++)
                        {
                            for (int _j = -1; _j < 2; _j++)
                            {
                                for (int _k = -1; _k < 2; _k++)
                                {
                                    int x = _i + e.Key.Item1;
                                    int y = _j + e.Key.Item2;
                                    int z = _k + e.Key.Item3;

                                    if (_i == 0 && _j == 0 && _k == 0)
                                        continue;

                                    if (x < 0)
                                    {
                                        x += n;
                                    }
                                    else if (x >= n)
                                    {
                                        x -= n;
                                    }

                                    if (y < 0)
                                    {
                                        y += n;
                                    }
                                    else if (y >= n)
                                    {
                                        y -= n;
                                    }

                                    if (z < 0)
                                    {
                                        z += n;
                                    }
                                    else if (z >= n)
                                    {
                                        z -= n;
                                    }

                                    var key = new Tuple<int, int, int>(x, y, z);

                                    if (cell_list_for_judge.ContainsKey(key))
                                    {
                                        cell_list_for_judge[key]++;
                                    }
                                    else
                                    {
                                        cell_list_for_judge.Add(key, 1);
                                    }
                                }
                            }
                        }
                    }

                    //clear cells in cell_location_matrix and current_cell_list.
                    cell_location_matrix.dataClear();
                    current_cell_list.Clear();

                    //add current cell's location **this takes a minute (means heavy process)**
                    foreach (var e in cell_list_for_judge)
                    {
                        if (e.Value <= r2 && e.Value >= r1)
                        {
                            //flag
                            cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 1;
                            current_cell_list.Add(e.Key, 0);
                        }
                    }

                    UnityEngine.Debug.Log(cell_list_for_judge.Count());
                    //clear cells in cell_list_for_judge
                    cell_list_for_judge.Clear();
                });

            }

            // I dun know why this process causes a lag :(
            // if (pre_x != (int)head_location.x || pre_y != (int)head_location.y || pre_z != (int)head_location.z)
            // {
            //     UpdateDotView();
            // }

            UpdateDotView();

            if (every_beat >= (BAR / 4.0) && !isSequential)
            {
                every_bar = 0;
                every_beat = 0;

            }

        }

    }

    public void UpdateDotView()
    {
        //clear all cells displayed on scene 
        foreach (DotManageSparse e in displaying_dots_list)
        {
            DotManageSparse.Pool(e);
        }
        displaying_dots_list.Clear();

        float count = 0f;

        for (int i = (int)head_location.x + (n / 2) - range; i < (int)head_location.x + (n / 2) + range; i++)
        {
            for (int j = (int)head_location.y + (n / 2) - range; j < (int)head_location.y + (n / 2) + range; j++)
            {
                for (int k = (int)head_location.z + (n / 2) - range; k < (int)head_location.z + (n / 2) + range; k++)
                {
                    count++;
                    DotManageSparse displaying_dot = DotManageSparse.Create();
                    displaying_dot.transform.position = new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k));
                    displaying_dot.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(count / (8f * 8f * 8f), 1f, 1f);
                    displaying_dots_list.Add(displaying_dot);
                    if (cell_location_matrix[i, j, k] == 1)
                    {
                        //appear alive
                        displaying_dot.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        //appear dead 
                        displaying_dot.transform.GetChild(0).gameObject.SetActive(false);
                    }
                }
            }
        }

    }

    public async void Judge()
    {

        // if (current_cell_list != null)
        // {
        //     //do this statement asynchronously.
        //     await Task.Run(() =>
        //     {
        //         //add cells to alives and deads
        //         foreach (var e in current_cell_list)
        //         {
        //             //add 1 to each adjacency cell
        //             for (int _i = -1; _i < 2; _i++)
        //             {
        //                 for (int _j = -1; _j < 2; _j++)
        //                 {
        //                     for (int _k = -1; _k < 2; _k++)
        //                     {
        //                         int x = _i + e.Key.Item1;
        //                         int y = _j + e.Key.Item2;
        //                         int z = _k + e.Key.Item3;

        //                         if (_i == 0 && _j == 0 && _k == 0)
        //                             continue;

        //                         if (x < 0)
        //                         {
        //                             x += n;
        //                         }
        //                         else if (x >= n)
        //                         {
        //                             x -= n;
        //                         }

        //                         if (y < 0)
        //                         {
        //                             y += n;
        //                         }
        //                         else if (y >= n)
        //                         {
        //                             y -= n;
        //                         }

        //                         if (z < 0)
        //                         {
        //                             z += n;
        //                         }
        //                         else if (z >= n)
        //                         {
        //                             z -= n;
        //                         }

        //                         var key = new Tuple<int, int, int>(x, y, z);

        //                         if (cell_list_for_judge.ContainsKey(key))
        //                         {
        //                             cell_list_for_judge[key]++;
        //                         }
        //                         else
        //                         {
        //                             cell_list_for_judge.Add(key, 1);
        //                         }
        //                     }
        //                 }
        //             }
        //         }

        //         //clear cells in cell_location_matrix and current_cell_list.
        //         cell_location_matrix.dataClear();
        //         current_cell_list.Clear();

        //         //add current cell's location **this takes a minute (means heavy process)**
        //         foreach (var e in cell_list_for_judge)
        //         {
        //             if (e.Value <= r2 && e.Value >= r1)
        //             {
        //                 //flag
        //                 cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 1;
        //                 current_cell_list.Add(e.Key, 0);
        //             }
        //         }

        //         UnityEngine.Debug.Log(cell_list_for_judge.Count());
        //         //clear cells in cell_list_for_judge
        //         cell_list_for_judge.Clear();

        //     });
        // }
    }

}
