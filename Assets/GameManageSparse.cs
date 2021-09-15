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
using System.Threading;

public class GameManageSparse : MonoBehaviour
{
    [SerializeField] int n = 512; //infinite universe
    [SerializeField] int r1 = 4, r2 = 4, r3 = 0, r4 = 0; //rules
    [SerializeField] int range; //apprear range
    Vector3 head_location; //change pos to location
    // public GameObject follower;

    [SerializeField] GameObject head_pref;
    [SerializeField] GameObject dot_pref;

    // the list of dots that's displaying now
    // I need this when I delete all cells in a scene
    List<List<DotManageSparse>> displaying_dots_list = new List<List<DotManageSparse>>();

    Dictionary<Tuple<int, int, int>, int> current_cell_list = new Dictionary<Tuple<int, int, int>, int>();
    Dictionary<Tuple<int, int, int>, int> cell_list_for_judge = new Dictionary<Tuple<int, int, int>, int>();

    // if cell_location_matrix[x,y,z] == 1, the cell near by the player will be appeared
    Sparse3DArray<int> cell_location_matrix = new Sparse3DArray<int>();
    Queue<Sparse3DArray<int>> pool_locations = new Queue<Sparse3DArray<int>>();
    //Sparse3DArray<int> pre_cell_location_matrix;

    float dotInterval = 1;
    public float bpm;
    //These are not constant.
    float BAR, BEAT;
    //every_bar is the time to refresh model with chords, every_beat is the time to refresh model with sequential, and delta_beat for calc every beat.
    float every_bar = 1, every_beat = 0, delta_time = 0;

    bool isRun = false, isPeriodic = true, isSequential = false;

    public InputField r1Input, r2Input, r3Input, r4Input, rangeInput, bpmInput, nInput;
    public Slider bpm_slider, range_slider, r1_slider, r2_slider, r3_slider, r4_slider, n_slider;
    StreamWriter writer = null;
    public static string path = null;

    Thread parallel;
    bool updating = false;
    float threshold_update = 0.025f;

    // Awake is called before Start
    void Awake()
    {
        DotManageSparse.SetOriginal(dot_pref);
    }

    // Start is called before the first frame update
    void Start()
    {
        //setting head_pref position flag_
        head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);

        r1Input.text = r1.ToString();
        r2Input.text = r2.ToString();
        r3Input.text = r3.ToString();
        r4Input.text = r4.ToString();
        nInput.text = n.ToString();
        rangeInput.text = range.ToString();
        bpmInput.text = bpm.ToString();

        UpdateDotView();
        parallel = new Thread(GoL);
    }

    void GoL()
    {
        //UnityEngine.Debug.Log("start thread");
        if (isPeriodic) // periodic
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

                            //process: itself
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
                //this one commented
                //UnityEngine.Debug.Log(e.Key.Item1 + " " + e.Key.Item2 + " " + e.Key.Item3 + " " + e.Value);

                if (e.Value > r3 || e.Value < r4)
                {
                    cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 0;
                    current_cell_list.Remove(e.Key);
                }
                if (e.Value <= r2 && e.Value >= r1)
                {
                    //flag
                    cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 1;
                    current_cell_list.Add(e.Key, 0);
                }

            }

            // UnityEngine.Debug.Log(cell_list_for_judge.Count());

            //clear cells in cell_list_for_judge
            cell_list_for_judge.Clear();

        }
        else //non-periodic 
        {
            // Head location is started by 0,0,0, so we need adjust this head location to n/2 (center).
            // After that, we wanna calc something from 0 to range*2. that's why we put this formula here.
            // store previous location of the head.
            int pre_x = (int)head_location.x + (n / 2);
            int pre_y = (int)head_location.y + (n / 2);
            int pre_z = (int)head_location.z + (n / 2);

            // calc for 8x8x8 cells
            for (int i = pre_x - range; i < pre_x + range; i++)
            {
                for (int j = pre_y - range; j < pre_y + range; j++)
                {
                    for (int k = pre_z - range; k < pre_z + range; k++)
                    {
                        //UnityEngine.Debug.Log(i + "," + j + "," + k);
                        if (cell_location_matrix[i, j, k] == 1)
                        {
                            for (int _i = -1; _i < 2; _i++)
                            {
                                for (int _j = -1; _j < 2; _j++)
                                {
                                    for (int _k = -1; _k < 2; _k++)
                                    {
                                        int x = _i + i;
                                        int y = _j + j;
                                        int z = _k + k;

                                        //process: itself
                                        if (_i == 0 && _j == 0 && _k == 0)
                                            continue;

                                        if (x < pre_x - range || y < pre_y - range || z < pre_z - range)
                                        {

                                        }
                                        else if (x >= pre_x + range || y >= pre_y + range || z >= pre_z + range)
                                        {

                                        }
                                        else
                                        {
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
                        }
                    }
                }
            }

            //上のループでkeyをポップして消したら計算量が減るかも...？
            for (int i = pre_x - range; i < pre_x + range; i++)
            {
                for (int j = pre_y - range; j < pre_y + range; j++)
                {
                    for (int k = pre_z - range; k < pre_z + range; k++)
                    {
                        if (cell_location_matrix[i, j, k] == 1)
                        {
                            var remove_key = new Tuple<int, int, int>(i, j, k);
                            cell_location_matrix[i, j, k] = 0;
                            current_cell_list.Remove(remove_key);
                        }
                    }
                }
            }

            //add current cell's location **this takes a minute (means heavy process)**
            foreach (var e in cell_list_for_judge)
            {
                if (e.Value > r3 || e.Value < r4)
                {
                    cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 0;
                    current_cell_list.Remove(e.Key);
                }
                if (e.Value <= r2 && e.Value >= r1)
                {
                    //flag
                    cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 1;
                    current_cell_list.Add(e.Key, 0);
                }

            }

            //clear cells in cell_list_for_judge
            cell_list_for_judge.Clear();

        }

        // //clear cells in cell_list_for_judge
        // cell_list_for_judge.Clear();
        //UnityEngine.Debug.Log("finish thread");
        lock (pool_locations)
        {
            pool_locations.Enqueue(cell_location_matrix);
        }
    }

    // using 'await' because tasks have to wait its finish 
    //async Task Update()
    void Update()
    {
        if (!updating)
        {
            //Moving a head
            head_location = head_pref.transform.position;
            head_location.x = Mathf.Clamp(head_location.x, (-n / 2) + range, (n / 2) - range);
            head_location.y = Mathf.Clamp(head_location.y, (-n / 2) + range, (n / 2) - range);
            head_location.z = Mathf.Clamp(head_location.z, (-n / 2) + range, (n / 2) - range);
            head_pref.transform.position = new Vector3(head_location.x, head_location.y, head_location.z);
        }

        BAR = 4f / (bpm / 60f);
        BEAT = 1f / ((bpm / 60f) * 2f);

        if (isRun)
        {
            every_beat += Time.deltaTime;

            //View update
            //Updates the view when the player's position changes in integer increments
            if (every_bar == 0)
            {
                every_bar++;

                //await Task.Run(() =>
                //{
                //});
                int how_many = 0;
                lock (pool_locations)
                {
                    how_many = pool_locations.Count;
                }
                if (!parallel.IsAlive && how_many < 20)
                {
                    parallel = new Thread(GoL);
                    parallel.Start();
                }
            }

            /**
            * calc time for sequential mode.
            */
            if (every_beat >= BEAT && isSequential)
            {
                delta_time = delta_time % (range * 2);

                if (delta_time == 0)
                {
                    every_bar = 0;
                }

                every_beat = 0;

                delta_time++;

            }

            /**
            * calc time for chord mode.
            */
            if (every_beat >= (BAR / 4.0) && !isSequential)
            {
                every_bar = 0;
                every_beat = 0;
            }

        }

        if (!updating)
        {
            if (pool_locations.Count == 0) pool_locations.Enqueue(cell_location_matrix);
            StartCoroutine(UpdateDotView());
        }

    }

    public IEnumerator UpdateDotView()
    {
        updating = true;
        float delta_update = Time.realtimeSinceStartup + threshold_update;
        Sparse3DArray<int> tmp_locations = new Sparse3DArray<int>();
        int how_many = 0;
        lock (pool_locations)
        {
            how_many = pool_locations.Count;
            if (how_many > 0)
                tmp_locations = pool_locations.Dequeue();
        }
        //UnityEngine.Debug.Log("start updateview");
        //clear all cells displayed on scene 
        //foreach (List<DotManageSparse> ee in displaying_dots_list)
        //{
        //    foreach(DotManageSparse e in ee)
        //        DotManageSparse.Pool(e);
        //}
        //displaying_dots_list.Clear();

        float count = 0f;

        for (int i = (int)head_location.x + (n / 2) - range; i < (int)head_location.x + (n / 2) + range; i++)
        {
            if (displaying_dots_list.Count >= range * 2)
            {
                foreach (DotManageSparse e in displaying_dots_list[0])
                {
                    DotManageSparse.Pool(e);
                }
                displaying_dots_list[0].Clear();
                displaying_dots_list.RemoveAt(0);
            }

            displaying_dots_list.Add(new List<DotManageSparse>());

            for (int j = (int)head_location.y + (n / 2) - range; j < (int)head_location.y + (n / 2) + range; j++)
            {
                for (int k = (int)head_location.z + (n / 2) - range; k < (int)head_location.z + (n / 2) + range; k++)
                {
                    count++;
                    DotManageSparse displaying_dot = DotManageSparse.Create();
                    displaying_dot.transform.position = new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k));
                    displaying_dot.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(count / ((Int64)Math.Pow(range * 2, 3)), 1f, 1f);
                    // displaying_dots_list.Add(displaying_dot);                    
                    displaying_dots_list[displaying_dots_list.Count - 1].Add(displaying_dot);
                    if (tmp_locations[i, j, k] == 1)
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
            if (Time.realtimeSinceStartup > delta_update)
            {
                yield return null;
                delta_update = Time.realtimeSinceStartup + threshold_update;
            }
        }

        //UnityEngine.Debug.Log("finish updateview");
        updating = false;
    }

    // setters
    public void setR1()
    {
        r1 = (int)r1_slider.value;
        r1Input.text = r1.ToString();
    }

    public void setR2()
    {
        r2 = (int)r2_slider.value;
        r2Input.text = r2.ToString();
    }

    public void setR3()
    {
        r3 = (int)r3_slider.value;
        r3Input.text = r3.ToString();
    }

    public void setR4()
    {
        r4 = (int)r4_slider.value;
        r4Input.text = r4.ToString();
    }

    public void setN()
    {
        n = 2 * (int)n_slider.value;
        nInput.text = n.ToString();
    }

    public void setRangeFive()
    {
    }

    public void setRangeEight()
    {
        range = 4;
        rangeInput.text = range.ToString();
    }

    public void setRangeTwelve()
    {
        range = 6;
        rangeInput.text = range.ToString();
    }

    public void change_range()
    {
        range = (int)range_slider.value;
        rangeInput.text = range.ToString();
    }

    public void change_bpm()
    {
        bpm = (int)bpm_slider.value;
        bpmInput.text = bpm.ToString();
    }

    public void on_periodic()
    {
        isPeriodic = !isPeriodic;
    }

    public void on_sequential()
    {
        isSequential = !isSequential;
    }

    public void RunStop()
    {
        isRun = !isRun;
        if (isRun) GameObject.Find("Run").GetComponentInChildren<Text>().text = "Stop";
        else GameObject.Find("Run").GetComponentInChildren<Text>().text = "Run";
    }

    public void setRandom()
    {
        cell_location_matrix.dataClear();
        current_cell_list.Clear();

        int pre_x = (int)head_location.x + (n / 2);
        int pre_y = (int)head_location.y + (n / 2);
        int pre_z = (int)head_location.z + (n / 2);

        for (int i = pre_x - range; i < pre_x + range; i++)
        {
            for (int j = pre_y - range; j < pre_y + range; j++)
            {
                for (int k = pre_z - range; k < pre_z + range; k++)
                {

                    if (UnityEngine.Random.Range(0, 5) == 0)
                    {
                        cell_location_matrix[i, j, k] = 1;
                        var random_key = new Tuple<int, int, int>(i, j, k);
                        current_cell_list.Add(random_key, 0);
                    }
                }
            }

        }

        UpdateDotView();

    }

    public void PresetGenerate()
    {
        cell_location_matrix.dataClear();
        current_cell_list.Clear();

        FileBrowser.RequestPermission();
        StartCoroutine(ShowLoadDialog());

        UpdateDotView();
    }

    private IEnumerator ShowLoadDialog()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, false, Application.streamingAssetsPath + "/Save/", "Load File", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        //Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            StreamReader sr = new StreamReader(FileBrowser.Result[0]);

            //for windows
            //TitleBarSetter.Instance.SetTitleBar(FileBrowser.Result[0]);

            List<string> lists = new List<string>();
            List<int> nums = new List<int>();

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] values = line.Split(',');

                // array to list
                lists.AddRange(values);
                nums = lists.ConvertAll(int.Parse);
            }

            List<int> rules_setter = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                rules_setter.Add(nums[i]);
                UnityEngine.Debug.Log(rules_setter[i]);
            }

            n = nums[0];
            r1 = nums[1];
            r2 = nums[2];
            r3 = nums[3];
            r4 = nums[4];

            nInput.text = n.ToString();
            r1Input.text = r1.ToString();
            r2Input.text = r2.ToString();
            r3Input.text = r3.ToString();
            r4Input.text = r4.ToString();

            int pre_x = (int)head_location.x + (n / 2);
            int pre_y = (int)head_location.y + (n / 2);
            int pre_z = (int)head_location.z + (n / 2);

            for (int i = 5; i < nums.Count - 2; i += 3)
            {
                cell_location_matrix[nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range] = 1;
                var key1 = new Tuple<int, int, int>(nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range);
                current_cell_list.Add(key1, 0);

                //UnityEngine.Debug.Log(key1.Item1 + " " + key1.Item2 + " " + key1.Item3);

            }
        }
    }

    // private IEnumerator ShowSaveDialog()
    // {
    //     // Show a load file dialog and wait for a response from user
    //     // Load file/folder: file, Allow multiple selection: true
    //     // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
    //     yield return FileBrowser.WaitForSaveDialog(false, false, Application.streamingAssetsPath + "/Save/", "Save File", "Save");

    //     // Dialog is closed
    //     // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
    //     //Debug.Log(FileBrowser.Result[0]);

    //     if (FileBrowser.Success)
    //     {
    //         ////DO SOMETHING, IN
    //         Encoding enc = Encoding.GetEncoding("utf-8");
    //         writer = new StreamWriter(FileBrowser.Result[0], false, enc); //<<--- file to save the data

    //         writer.WriteLine("{0},{1},{2},{3}", r1, r2, r3, r4);

    //         foreach (GameObject e in cpalives)
    //         {
    //             writer.WriteLine("{0},{1},{2}", e.GetComponent<DotManage>().x, e.GetComponent<DotManage>().y, e.GetComponent<DotManage>().z);
    //             writer.Flush();
    //         }

    //         writer.Close();
    //     }
    // }

}
