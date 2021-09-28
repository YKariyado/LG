#pragma warning disable 0414

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
using HTC.UnityPlugin.Vive;

public class VRGameManageSparse : MonoBehaviour
{
    [SerializeField] int n = 2048; //infinite universe
    private int t_n = 2048;
    [SerializeField] int r1 = 4, r2 = 4, r3 = 0, r4 = 0; //rules
    private int t_r1 = 4, t_r2 = 4, t_r3 = 0, t_r4 = 0;
    [SerializeField] int range = 6; //apprear range

    int follower_position;

    Vector3 head_location; //change pos to location
    // public GameObject follower;

    [SerializeField] GameObject head_pref = default;
    [SerializeField] GameObject dot_pref = default;
    [SerializeField] GameObject dot_alive_pref = default;
    public GameObject main_camera;
    public GameObject follower;
    public GameObject mini_follower;
    public GameObject mini_pos;
    public GameObject Cells;
    public GameObject Alive_Cells1;
    public GameObject Alive_Cells2;
    public GameObject canvas;
    private int current_display = 0;

    // the list of dots that's displaying now
    // I need this when I delete all cells in a scene
    // 二重リスト
    //List<List<DotManageSparse>> displaying_dots_list = new List<List<DotManageSparse>>();
    List<GameObject> playing_dots = new List<GameObject>();
    List<List<GameObject>> alive_dots = new List<List<GameObject>>(); //only 2, their are buffers

    //Dictionary<Tuple<int, int, int>, byte> current_cell_list = new Dictionary<Tuple<int, int, int>, byte>();
    //Dictionary<Tuple<int, int, int>, byte> cell_list_for_judge = new Dictionary<Tuple<int, int, int>, byte>();
    HashSet<ulong> current_cell_list = new HashSet<ulong>();
    Dictionary<ulong, byte> cell_list_for_judge = new Dictionary<ulong, byte>();

    // if cell_location_matrix[x,y,z] == 1, the cell near by the player will be appeared
    Sparse3DArray<byte> cell_location_matrix = new Sparse3DArray<byte>();
    Sparse3DArray<byte> painting_matrix = new Sparse3DArray<byte>();
    Queue<Dictionary<Tuple<int, int, int>, byte>> pool_locations = new Queue<Dictionary<Tuple<int, int, int>, byte>>();

    // array of audio clips
    //private List<float[]> pitch=new List<float[]>();
    private int pitch_freq;
    private int pitch_channels;
    //private AudioClip[,] pitch;
    private float[][] pitch;
    //int[] clips = new int[12];
    List<List<int>> seq_play;
    List<int> current_alive = new List<int>();
    int seq_start = 0, seq_end = 0;

    float dotInterval = 1;
    public float bpm;
    //These are not constant.
    float BAR, BEAT;
    //every_bar is the time to refresh model with chords, every_beat is the time to refresh model with sequential, and delta_beat for calc every beat.
    float every_bar = 1, every_beat = 0, delta_time = 0;

    bool isRun = false, isPeriodic = true, isSequential = false, follow = false, follower_end = false;

    public InputField bpmInput, rangeInput, r1Input, r2Input, r3Input, r4Input, nInput, coor_x, coor_y, coor_z;
    public Slider bpm_slider, range_slider, r1_slider, r2_slider, r3_slider, r4_slider, n_slider, sequence_slider;
    StreamWriter writer = null;
    public static string path = null;

    Thread parallel;
    bool updating = false, play_now = false, get_next = false, in_menu = false;
    float threshold_update = 0.025f;
    Coroutine update_coroutine;

    // Awake is called before Start
    void Awake()
    {
        FileBrowser.SetDefaultFilter(".csv");
        DotManageSparse.SetOriginal(dot_pref);
        pitch = new float[12][];
        for (int i = 0; i < 12; ++i)
        {
            AudioClip tmp = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab_sparse"), "pitch_" + (i + 1).ToString()));
            pitch[i] = new float[tmp.samples * tmp.channels];
            tmp.GetData(pitch[i], 0);
            pitch_freq = tmp.frequency;
            pitch_channels = tmp.channels;
        };
        //pitch = new AudioClip[12, 12 * 12];
        //for (int i = 0; i < 12; ++i)
        //    for (int j = 0; j < 12 * 12; ++j)
        //        pitch[i, j] = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab_sparse"), "pitch_" + (i + 1).ToString()));
        //pitch = new AudioClip[12];
        //for (int i = 0; i < 12; ++i) pitch[i] = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab_sparse"), "pitch_" + (i + 1).ToString()));
        //for (int i = 0; i < 12; i++)
        //{
        //    tmp = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab_sparse"), "pitch_" + (i + 1).ToString()));
        //    float[] temp = new float[tmp.samples*tmp.channels];
        //    tmp.GetData(temp, 0);            
        //    //pitch.Add(temp);
        //    //pitch_channels = tmp.channels;
        //    //pitch_freq = tmp.frequency;
        //}

        follower_position = (-n / 2) - 1;

        populate_display();
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

        //UpdateDotView();
        if (isSequential && isRun) follower.GetComponent<Renderer>().enabled = true;
        else follower.GetComponent<Renderer>().enabled = false;

        parallel = new Thread(GoL);
        setMenu();
    }

    void populate_display()
    {
        Alive_Cells1.SetActive(false);
        Alive_Cells2.SetActive(false);
        seq_play = new List<List<int>>();
        foreach (var e in playing_dots) Destroy(e);
        foreach (var e in alive_dots)
        {
            foreach (var e2 in e)
                Destroy(e2);
            e.Clear();
        }
        playing_dots.Clear();
        alive_dots.Clear();
        alive_dots.Add(new List<GameObject>());
        alive_dots.Add(new List<GameObject>());
        int count = 0;
        for (int k = -range; k < range; k++)
        {
            for (int j = -range; j < range; j++)
                for (int i = -range; i < range; i++)
                {
                    GameObject tmp = Instantiate<GameObject>(dot_pref, Cells.transform);
                    tmp.transform.localPosition = new Vector3(i * dotInterval, j * dotInterval, k * dotInterval);
                    //tmp.GetComponent<AudioSource>().clip = AudioClip.Create("p_" + i.ToString() + j.ToString() + k.ToString(), pitch[0].Length / pitch_channels, pitch_channels, pitch_freq, false);
                    tmp.GetComponent<AudioSource>().clip = AudioClip.Create("p_" + i.ToString() + j.ToString() + k.ToString(), pitch[0].Length / pitch_channels, pitch_channels, pitch_freq, false);
                    playing_dots.Add(tmp);
                    GameObject tmp2 = Instantiate<GameObject>(dot_alive_pref, Alive_Cells1.transform);
                    tmp2.transform.localPosition = new Vector3(i * dotInterval, j * dotInterval, k * dotInterval);
                    tmp2.SetActive(false);
                    //tmp2.GetComponent<Renderer>().material.color = Color.HSVToRGB((float)count / ((Int64)Math.Pow(range * 2, 3)), 1f, 1f);
                    alive_dots[0].Add(tmp2);
                    GameObject tmp3 = Instantiate<GameObject>(dot_alive_pref, Alive_Cells2.transform);
                    tmp3.transform.localPosition = new Vector3(i * dotInterval, j * dotInterval, k * dotInterval);
                    tmp3.SetActive(false);
                    //tmp3.GetComponent<Renderer>().material.color = Color.HSVToRGB((float)count / ((Int64)Math.Pow(range * 2, 3)), 1f, 1f);
                    alive_dots[1].Add(tmp3);
                    count += 1;
                }
            seq_play.Add(new List<int>());
        }
    }

    void GoL()
    {

        // if (isSequential) follower.GetComponent<Renderer>().enabled = true;
        // else follower.GetComponent<Renderer>().enabled = false;

        //UnityEngine.Debug.Log("start thread");
        if (isPeriodic) // periodic
        {
            //add cells to alives and deads
            foreach (ulong e in current_cell_list)
            {
                //add 1 to each adjacency cell
                for (int _i = -1; _i < 2; _i++)
                {
                    for (int _j = -1; _j < 2; _j++)
                    {
                        for (int _k = -1; _k < 2; _k++)
                        {
                            //process: itself
                            if (_i == 0 && _j == 0 && _k == 0)
                                continue;

                            int[] tmp = iidx(e, t_n);
                            int x = _i + tmp[0];
                            int y = _j + tmp[1];
                            int z = _k + tmp[2];

                            if (x < 0)
                            {
                                x += t_n;
                            }
                            else if (x >= t_n)
                            {
                                x -= t_n;
                            }

                            if (y < 0)
                            {
                                y += t_n;
                            }
                            else if (y >= t_n)
                            {
                                y -= t_n;
                            }

                            if (z < 0)
                            {
                                z += t_n;
                            }
                            else if (z >= t_n)
                            {
                                z -= t_n;
                            }
                            byte temp;
                            ulong tmp2 = idx(x, y, z, t_n);
                            cell_list_for_judge.TryGetValue(tmp2, out temp);
                            cell_list_for_judge[tmp2] = (byte)(temp + 1);
                            //var key = new Tuple<int, int, int>(x, y, z);

                            //if (cell_list_for_judge.ContainsKey(key))
                            //{
                            //    cell_list_for_judge[key]++;
                            //}
                            //else
                            //{
                            //    cell_list_for_judge.Add(key, 1);
                            //}

                        }
                    }
                }
            }

            //clear cells in cell_location_matrix and current_cell_list.
            //cell_location_matrix.dataClear();
            //current_cell_list.Clear();

            //add current cell's location **this takes a minute (means heavy process)**
            foreach (KeyValuePair<ulong, byte> e in cell_list_for_judge)
            {
                //this one commented
                //UnityEngine.Debug.Log(e.Key.Item1 + " " + e.Key.Item2 + " " + e.Key.Item3 + " " + e.Value);
                int[] tmp = iidx(e.Key, t_n);
                if (e.Value > t_r3 || e.Value < t_r4)
                {
                    cell_location_matrix[tmp[0], tmp[1], tmp[2]] = 0;
                    current_cell_list.Remove(e.Key);
                }
                if (e.Value <= t_r2 && e.Value >= t_r1)
                {
                    //flag                    
                    cell_location_matrix[tmp[0], tmp[1], tmp[2]] = 1;
                    current_cell_list.Add(e.Key);
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
            int pre_x = (int)head_location.x + (t_n / 2);
            int pre_y = (int)head_location.y + (t_n / 2);
            int pre_z = (int)head_location.z + (t_n / 2);

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
                                            byte temp;
                                            ulong e = idx(x, y, z, t_n);
                                            cell_list_for_judge.TryGetValue(e, out temp);
                                            cell_list_for_judge[e] = (byte)(temp + 1);
                                            //var key = new Tuple<int, int, int>(x, y, z);

                                            //if (cell_list_for_judge.ContainsKey(key))
                                            //{
                                            //    cell_list_for_judge[key]++;
                                            //}
                                            //else
                                            //{
                                            //    cell_list_for_judge.Add(key, 1);
                                            //}
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }

            //上のループでkeyをポップして消したら計算量が減るかも...？
            //for (int i = pre_x - range; i < pre_x + range; i++)
            //{
            //    for (int j = pre_y - range; j < pre_y + range; j++)
            //    {
            //        for (int k = pre_z - range; k < pre_z + range; k++)
            //        {
            //            if (cell_location_matrix[i, j, k] == 1)
            //            {
            //                //var remove_key = new Tuple<int, int, int>(i, j, k);
            //                cell_location_matrix[i, j, k] = 0;
            //                current_cell_list.Remove(idx(i,j,k,t_n));
            //            }
            //        }
            //    }
            //}

            //add current cell's location **this takes a minute (means heavy process)**
            foreach (KeyValuePair<ulong, byte> e in cell_list_for_judge)
            {
                int[] tmp = iidx(e.Key, t_n);
                if (e.Value > t_r3 || e.Value < t_r4)
                {
                    cell_location_matrix[tmp[0], tmp[1], tmp[2]] = 0;
                    current_cell_list.Remove(e.Key);
                }
                if (e.Value <= t_r2 && e.Value >= t_r1)
                {
                    //flag                    
                    cell_location_matrix[tmp[0], tmp[1], tmp[2]] = 1;
                    current_cell_list.Add(e.Key);
                }
                //if (e.Value > t_r3 || e.Value < t_r4)
                //{
                //    cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 0;
                //    current_cell_list.Remove(e.Key);
                //}
                //if (e.Value <= t_r2 && e.Value >= t_r1)
                //{
                //    //flag
                //    cell_location_matrix[e.Key.Item1, e.Key.Item2, e.Key.Item3] = 1;
                //    current_cell_list.Add(e.Key, 0);
                //}

            }

            //clear cells in cell_list_for_judge
            cell_list_for_judge.Clear();

        }

        // //clear cells in cell_list_for_judge
        // cell_list_for_judge.Clear();
        //UnityEngine.Debug.Log("finish thread");
        lock (pool_locations)
        {
            pool_locations.Enqueue(new Dictionary<Tuple<int, int, int>, byte>(cell_location_matrix.data));
        }
    }

    // using 'await' because tasks have to wait its finish
    //async Task Update()
    void Update()
    {
        if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu) || ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Menu)) {
            setMenu();
        }

        if (!updating)
        {
            //if (follower_position < -n / 2-1) follower_position = -(n / 2);
            //Moving a head
            head_location = head_pref.transform.position;
            head_pref.transform.position = new Vector3(head_location.x, head_location.y, head_location.z);
            //udpate mini map
            //UnityEngine.Debug.Log(new Vector3(head_location.x, head_location.y, head_location.z));
            Vector3 fix_head = new Vector3(head_location.x + (n / 2), head_location.y + (n / 2), head_location.z + (n / 2)) / n;
            fix_head = (-2 * fix_head) + Vector3.up;
            mini_pos.transform.localPosition = fix_head;
            mini_pos.transform.localScale = (0.1f * Vector3.one) + Vector3.one * (1.9f * (1 - ((float)(n - 12) / (2048 - 12))));
            mini_follower.GetComponent<Renderer>().enabled = isSequential && isRun;
            mini_follower.transform.localPosition = new Vector3(-1, 0, -2 * ((follower_position + (float)n / 2) / n));
        }

        BAR = 4f / (bpm / 60f);
        BEAT = 1f / ((bpm / 60f) * 2f);

        if (isRun)
        {

            every_beat += Time.deltaTime;
            sequence_slider.interactable = false;

            //View update
            //Updates the view when the player's position changes in integer increments            

            if (every_bar == 0 && !isSequential)
            {
                every_bar++;
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
                follower_position++;
                if (follower_position > n / 2) { follower_position = (-n / 2) - 2; }
                if (follower_position >= (n / 2)) { follower_end = true; }
                int cell_z = (int)Cells.transform.position.z + n / 2;
                int foll_z = follower_position + n / 2;
                if (foll_z >= cell_z - range && foll_z < cell_z + range && foll_z < n && foll_z >= 0)
                {
                    int tmp = foll_z - (cell_z - range);
                    //UnityEngine.Debug.Log(tmp);
                    foreach (int e in seq_play[tmp]) {
                        playing_dots[e].GetComponent<AudioSource>().enabled = true;
                        playing_dots[e].GetComponent<HRTFu>().enabled = true;
                        playing_dots[e].GetComponent<AudioSource>().volume = 1f / (float)seq_play[tmp].Count;
                        playing_dots[e].GetComponent<AudioSource>().Play();
                    }
                }
                //play_now = true;
            }
            follower.transform.localPosition = new Vector3(head_pref.transform.position.x, head_pref.transform.position.y, follower_position);
            /**
            * ---calc location of follower cube---
            * +range: set start point.
            * -0.5f: fix head position temporary
            * +delta_time: steps
            */

            /**
            * calc time for chord mode.
            */
            if (every_beat >= (BAR / 4.0) && !isSequential)
            {
                // attach sounds to cells displayed
                every_bar = 0;
                every_beat = 0;
                get_next = true;
                //foreach (int e in current_alive)
                //{
                //    playing_dots[e].GetComponent<AudioSource>().enabled = true;
                //    playing_dots[e].GetComponent<HRTFu>().enabled = true;
                //    playing_dots[e].GetComponent<AudioSource>().volume = 1f / (float)current_alive.Count;
                //    playing_dots[e].GetComponent<AudioSource>().Play();
                //}                                

            }

        }
        if (isSequential && !isRun)
        {
            sequence_slider.interactable = true;
            float tmp = follower_position + n / 2;
            if (tmp < 0) tmp = 0;
            sequence_slider.value = tmp / (float)n;
        }

        if (isSequential && isRun)
        {
            follower.GetComponent<Renderer>().enabled = true;
            if (follow)
            {
                //if (head_location.z <= follower_position + (range-2))
                //{
                //    head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position + (range-2));
                //}
                //else if (head_location.z > follower_position - (range-2))
                //{
                //    head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position + (range-2));
                //}
                head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position - 3);
            }
            else
                if ((head_location.z < follower_position - range) || (head_location.z > follower_position + range))
                follower.GetComponent<Renderer>().enabled = false;
        }
        else follower.GetComponent<Renderer>().enabled = false;

        if (isRun && isSequential && follower_end && updating)
        {
            StopCoroutine(update_coroutine);
            updating = false;
        }

        if (!updating)
        {
            // pooling して順次アプデしてく
            lock (pool_locations)
            {
                //UnityEngine.Debug.Log(play_now.ToString() + pool_locations.Count);
                if (pool_locations.Count > 0 && isRun)
                {
                    // dequeue a cell_locations_matrix
                    if (!isSequential && get_next) play_now = true;
                    if ((!isSequential && get_next) || (isSequential && follower_end))
                    {
                        //painting_matrix.data = new Dictionary<Tuple<int, int, int>, byte>(pool_locations.Dequeue());
                        painting_matrix.data = pool_locations.Dequeue();
                        get_next = false;
                    }
                }
            }
            follower_end = false;
            update_coroutine = StartCoroutine(UpdateDotView());
            //UpdateDotView(play_now);            
        }
        int how_many = 0;
        lock (pool_locations)
        {
            how_many = pool_locations.Count;
        }
        if (parallel == null)
        {
            parallel = new Thread(GoL);
            parallel.Priority = System.Threading.ThreadPriority.Highest;
            parallel.Start();
        }
        if (!parallel.IsAlive && how_many < 50)
        {
            t_n = n; t_r1 = r1; t_r2 = r2; t_r3 = r3; t_r4 = r4;
            parallel = new Thread(GoL);
            parallel.Priority = System.Threading.ThreadPriority.Highest;
            parallel.Start();
        }

    }
    ulong idx(int x, int y, int z, int m) { return (ulong)x * (ulong)Math.Pow(m, 2) + (ulong)y * (ulong)m + (ulong)z; }
    int[] iidx(ulong x, int m) { return new int[] { (int)(x / (ulong)Math.Pow(m, 2)), (int)((x % (ulong)Math.Pow(m, 2)) / (ulong)m), (int)(x % (ulong)m) }; }

    public IEnumerator UpdateDotView()
    {
        updating = true;
        int count = 0;
        float delta_update = Time.realtimeSinceStartup + threshold_update;
        List<int> alive = new List<int>();
        List<List<int>> tmp_seq_alive = new List<List<int>>();

        Vector3 pre = new Vector3((int)head_location.x + (n / 2), (int)head_location.y + (n / 2), (int)head_location.z + (n / 2));
        Vector3 actual_center = new Vector3((int)head_location.x, (int)head_location.y, (int)head_location.z);
        Vector3 new_loc = new Vector3((int)Cells.transform.position.x, (int)Cells.transform.position.y, (int)Cells.transform.position.z);
        //UnityEngine.Debug.Log(head_location.ToString()+"\t"+actual_center.ToString());
        if (actual_center != new_loc) {
            new_loc = actual_center;
            if (range > pre.x) new_loc.x = -n / 2 + range;
            if (n - range < pre.x) new_loc.x = n / 2 - range;
            if (range > pre.y) new_loc.y = -n / 2 + range;
            if (n - range < pre.y) new_loc.y = n / 2 - range;
            if (range > pre.z) new_loc.z = -n / 2 + range;
            if (n - range < pre.z) new_loc.z = n / 2 - range;
        }
        if (range > pre.x) pre.x = range;
        if (n - range < pre.x) pre.x = n - range;
        if (range > pre.y) pre.y = range;
        if (n - range < pre.y) pre.y = n - range;
        if (range > pre.z) pre.z = range;
        if (n - range < pre.z) pre.z = n - range;
        int k2 = 0;
        Vector3 center = new Vector3((float)n / 2, (float)n / 2, (float)n / 2);
        for (int k = (int)(pre.z - range); k < (pre.z + range); k++)
        {
            List<int> tmp_seq = new List<int>();
            //seq_play[k2].Clear();
            for (int j = (int)(pre.y - range); j < (pre.y + range); j++)
            {
                for (int i = (int)(pre.x - range); i < (pre.x + range); i++)
                {

                    // 二重リストの一番ケツのリストの中に入れる                    
                    if (painting_matrix[i, j, k] == 1)
                    {
                        //appear alive
                        alive_dots[current_display][count].SetActive(true);
                        //alive_dots[current_display][count].GetComponent<Renderer>().material.color = Color.HSVToRGB((float)((double)idx(i, j, k, n) / Math.Pow(n, 3)), 1f, 1f);
                        Color col = Color.HSVToRGB(Vector3.Distance(new Vector3(i, j, k), center) / (((float)n / 2f) + 3f), 1f, 1f);
                        if (in_menu) col.a = 0.05f;
                        alive_dots[current_display][count].GetComponent<Renderer>().material.color = col;
                        if (isRun)
                        {
                            //playing_dots[count].GetComponent<AudioSource>().clip = pitch[i % 12, clips[i % 12]];                            
                            //playing_dots[count].GetComponent<FaustPlugin_BPF>().enabled = true;
                            //clips[i % 12] = (clips[i % 12] + 1) % (12 * 12);
                            //playing_dots[count].GetComponent<AudioSource>().enabled = true;
                            //playing_dots[count].GetComponent<HRTFu>().enabled = true;
                            float freq = Mathf.Pow(10f, (2f + i * ((4f - 2f) / (n - 1f))));
                            float q = 1f + 99f * ((float)k / n);
                            playing_dots[count].GetComponent<HRTFu>().setParameter(0, freq);
                            playing_dots[count].GetComponent<HRTFu>().setParameter(1, q);
                            playing_dots[count].GetComponent<AudioSource>().clip.SetData(pitch[i % 12], 0);
                            //playing_dots[count].GetComponent<FaustPlugin_BPF>().setParameter(0, freq);
                            //playing_dots[count].GetComponent<FaustPlugin_BPF>().setParameter(1, q);
                            //playing_dots[count].GetComponent<AudioLowPassFilter>().cutoffFrequency = freq;
                            //playing_dots[count].GetComponent<AudioLowPassFilter>().lowpassResonanceQ = q;
                            //playing_dots[count].GetComponent<AudioHighPassFilter>().cutoffFrequency = freq;
                            //playing_dots[count].GetComponent<AudioHighPassFilter>().highpassResonanceQ = q;
                            //displaying_dots[count].GetComponent<AudioSource>().clip = AudioClip.Create("p_"+(i%12).ToString(),pitch[i%12].Item1.Length/ pitch[i % 12].Item3, pitch[i % 12].Item3, pitch[i % 12].Item2,false);                            
                            if (isSequential)
                            {
                                //seq_play[k2].Add(count);
                                tmp_seq.Add(count);
                                //if (k == follower_position + n / 2)
                                //    displaying_dots[count].GetComponent<AudioSource>().Play();
                            }
                            else alive.Add(count);
                        }
                        else {
                            if (!playing_dots[count].GetComponent<AudioSource>().isPlaying)
                            {
                                playing_dots[count].GetComponent<AudioSource>().Stop();
                                playing_dots[count].GetComponent<AudioSource>().enabled = false;
                                playing_dots[count].GetComponent<HRTFu>().reset_buffer();
                                playing_dots[count].GetComponent<HRTFu>().enabled = false;
                            }
                        }
                    }
                    else
                    {
                        //appear dead
                        //if (playing_dots[count].GetComponent<AudioSource>().isPlaying) playing_dots[count].GetComponent<AudioSource>().Stop();
                        //if (!playing_dots[count].GetComponent<AudioSource>().isPlaying)
                        //{
                        //playing_dots[count].GetComponent<AudioSource>().Stop();
                        //playing_dots[count].GetComponent<AudioSource>().clip = null;
                        if (!playing_dots[count].GetComponent<AudioSource>().isPlaying)
                        {
                            playing_dots[count].GetComponent<AudioSource>().Stop();
                            playing_dots[count].GetComponent<AudioSource>().enabled = false;
                            playing_dots[count].GetComponent<HRTFu>().reset_buffer();
                            playing_dots[count].GetComponent<HRTFu>().enabled = false;
                        }
                        //playing_dots[count].GetComponent<FaustPlugin_BPF>().enabled = false;
                        //}
                        alive_dots[current_display][count].SetActive(false);
                    }
                    count++;
                }
            }
            tmp_seq_alive.Add(tmp_seq);
            k2++;
            if (Time.realtimeSinceStartup > delta_update)
            {
                yield return null;
                delta_update = Time.realtimeSinceStartup + threshold_update;
            }
        }
        if (isSequential && isRun)
        {
            seq_play.Clear();
            seq_play = new List<List<int>>(tmp_seq_alive);
        }
        if (!isSequential && isRun)
        {
            current_alive.Clear();
            current_alive = new List<int>(alive);
            if (play_now)
            {
                foreach (int e in alive)
                {
                    playing_dots[e].GetComponent<AudioSource>().enabled = true;
                    playing_dots[e].GetComponent<HRTFu>().enabled = true;
                    playing_dots[e].GetComponent<AudioSource>().volume = 1f / (float)alive.Count;
                    playing_dots[e].GetComponent<AudioSource>().Play();
                }
                play_now = false;
            }
        }
        //if (play_now) {
        //    //foreach (var e in alive) displaying_dots[e].GetComponent<AudioSource>().Play();
        //    //current_alive.Clear();
        //    //current_alive = new List<int>(alive);
        //    play_now = false;
        //    get_next = true;
        //}
        //if(!isSequential && play_sound)
        //    foreach(List<DotManageSparse> e1 in displaying_dots_list)            
        //        foreach(DotManageSparse e2 in e1)
        //            if(e2.transform.GetChild(0).gameObject.active) e2.GetComponent<AudioSource>().Play();
        //if (isSequential) seq_play = new List<List<int>>(tmp_seq_alive);
        if (current_display == 1)
        {
            Alive_Cells1.SetActive(false);
            Alive_Cells2.SetActive(true);
        }
        else
        {
            Alive_Cells1.SetActive(true);
            Alive_Cells2.SetActive(false);
        }
        seq_start = (int)(pre.z - range);
        seq_end = (int)(pre.z + range - 1);
        current_display = (current_display + 1) % 2;
        Cells.transform.position = new_loc;
        updating = false;
    }
    public void setMenu()
    {
        Vector3 new_loc = main_camera.transform.position + Vector3.forward * 7;
        canvas.transform.position = new_loc;
        if (in_menu)
        {
            foreach (GameObject e in playing_dots)
            {
                e.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0.219f);
                e.GetComponent<SphereCollider>().enabled = true;
            }
            foreach (List<GameObject> e in alive_dots)
            {
                foreach (GameObject e2 in e)
                {
                    e2.SetActive(true);
                    e2.GetComponent<SphereCollider>().enabled = true;
                    e2.SetActive(false);
                }
            }
            in_menu = false;
            canvas.SetActive(false);
        }
        else
        {
            foreach (GameObject e in playing_dots)
            {
                e.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0.01f);
                e.GetComponent<SphereCollider>().enabled = false;
            }
            foreach (List<GameObject> e in alive_dots)
            {
                foreach (GameObject e2 in e)
                {
                    e2.SetActive(true);
                    e2.GetComponent<SphereCollider>().enabled = false;
                    e2.SetActive(false);
                }
            }
            in_menu = true;
            canvas.SetActive(true);
        }
    }
    // setters
    public void setR1()
    {
        if (r1 != (int)r1_slider.value)
        {
            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
            current_cell_list.Clear();
            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
                current_cell_list.Add(idx(e.Item1, e.Item2, e.Item3, n));
        }
        r1 = (int)r1_slider.value;
        r1Input.text = r1.ToString();
    }

    public void setR2()
    {
        if (r2 != (int)r2_slider.value)
        {
            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
            current_cell_list.Clear();
            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
                current_cell_list.Add(idx(e.Item1, e.Item2, e.Item3, n));
        }
        r2 = (int)r2_slider.value;
        r2Input.text = r2.ToString();
    }

    public void setR3()
    {
        if (r3 != (int)r3_slider.value)
        {
            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
            current_cell_list.Clear();
            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
                current_cell_list.Add(idx(e.Item1, e.Item2, e.Item3, n));
        }
        r3 = (int)r3_slider.value;
        r3Input.text = r3.ToString();
    }

    public void setR4()
    {
        if (r4 != (int)r4_slider.value) {
            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
        }
        r4 = (int)r4_slider.value;
        r4Input.text = r4.ToString();
    }

    public void setN()
    {
        if (n != 2 * (int)n_slider.value)
        {
            n = 2 * (int)n_slider.value;
            //if (head_pref.transform.position.x+n/2 < 0 || head_pref.transform.position.x+n/2 > n || head_pref.transform.position.y+n/2 < 0 || head_pref.transform.position.y+n/2 > n || head_pref.transform.position.z+n/2 < 0 || head_pref.transform.position.z+n/2 > n)            
            //head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);
            Vector3 pre = new Vector3((int)head_location.x + (n / 2), (int)head_location.y + (n / 2), (int)head_location.z + (n / 2));
            Vector3 new_loc = head_location;
            if (range > pre.x) new_loc.x = -n / 2 + range;
            if (n - range < pre.x) new_loc.x = n / 2 - range;
            if (range > pre.y) new_loc.y = -n / 2 + range;
            if (n - range < pre.y) new_loc.y = n / 2 - range;
            if (range > pre.z) new_loc.z = -n / 2 + range;
            if (n - range < pre.z) new_loc.z = n / 2 - range;
            head_location = new_loc;
            //follower_position = (-n / 2)-1;
            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
            current_cell_list.Clear();
            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
                current_cell_list.Add(idx(e.Item1, e.Item2, e.Item3, n));
            t_n = n;
            follower_position = (-n / 2) - 2;
        }
        n = 2 * (int)n_slider.value;
        nInput.text = n.ToString();
    }

    public void setCell()
    {
        int x, y, z;
        if (coor_x.text == "") x = 0;
        else x = Int32.Parse(coor_x.text);
        if (coor_y.text == "") y = 0;
        else y = Int32.Parse(coor_y.text);
        if (coor_z.text == "") z = 0;
        else z = Int32.Parse(coor_x.text);
        if (x >= 0 && x < n && y >= 0 && y < n && z >= 0 && z < n) {
            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();
            cell_location_matrix.dataClear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
            current_cell_list.Clear();
            painting_matrix[x, y, z] = 1;
            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
                current_cell_list.Add(idx(e.Item1, e.Item2, e.Item3, n));
        }
    }

    public void Go_to_cell()
    {
        int x, y, z;
        if (coor_x.text == "") x = 0;
        else x = Int32.Parse(coor_x.text);
        if (coor_y.text == "") y = 0;
        else y = Int32.Parse(coor_y.text);
        if (coor_z.text == "") z = 0;
        else z = Int32.Parse(coor_z.text);
        if (x >= 0 && x < n && y >= 0 && y < n && z >= 0 && z < n) {
            head_pref.transform.position = new Vector3(x - n / 2, y - n / 2, z - n / 2);
            //Vector3 tmp = head_pref.transform.position;
            //if (range > x) tmp.x = range-n/2;
            //if (n - range < x) tmp.x = n/2 - range;
            //if (range > y) tmp.y = range-n/2;
            //if (n - range < y) tmp.y = n/2 - range;
            //if (range > z) tmp.z = range-n/2;
            //if (n - range < z) tmp.z = n/2 - range;
            //Cells.transform.position = tmp;
        }
    }

    public void center_camera()
    {
        head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);
        //Vector3 tmp = head_pref.transform.position;
        //if (range > tmp.x) tmp.x = range - n / 2;
        //if (n - range < tmp.x) tmp.x = n / 2 - range;
        //if (range > tmp.y) tmp.y = range - n / 2;
        //if (n - range < tmp.y) tmp.y = n / 2 - range;
        //if (range > tmp.z) tmp.z = range - n / 2;
        //if (n - range < tmp.z) tmp.z = n / 2 - range;
        //Cells.transform.position = tmp;
    }

    // public void setRangeEight()
    // {
    //     range = 4;
    //     rangeInput.text = range.ToString();
    //     follower.transform.localScale = new Vector3(1.2f, 10, 10);
    // }

    // public void setRangeTwelve()
    // {
    //     range = 6;
    //     rangeInput.text = range.ToString();
    //     follower.transform.localScale = new Vector3(1.2f, 14, 14);
    // }

    // public void change_range()
    // {
    //     range = (int)range_slider.value;
    //     rangeInput.text = range.ToString();
    // }

    public void change_bpm()
    {
        bpm = (int)bpm_slider.value;
        bpmInput.text = bpm.ToString();
    }

    public void move_seq()
    {
        float tmp = sequence_slider.value;
        follower_position = (int)(tmp * n) - n / 2;
    }

    public void on_periodic()
    {
        isPeriodic = !isPeriodic;
    }

    public void on_sequential()
    {
        //head_pref.transform.position = new Vector3(0, 0, (-1) * (n / 2) + range);
        if ((head_location.z < follower_position - range) && isSequential)
        {
            head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position - range);
        }
        else if ((head_location.z > follower_position + range) && isSequential)
        {
            head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position + range);
        }
        isSequential = !isSequential;
        if (isSequential)
        {
            bpm = 100;
            bpm_slider.value = 100f;
            bpmInput.text = (100).ToString();
            bpm_slider.interactable = true;
        }
        else
        {
            bpm = 40;
            bpm_slider.value = 40f;
            bpmInput.text = (40).ToString();
            sequence_slider.interactable = true;
            bpm_slider.interactable = false;
        }
    }

    public void on_follow()
    {
        follow = !follow;
    }

    public void RunStop()
    {
        isRun = !isRun;
        if (isRun)
        {
            GameObject.Find("Run").GetComponentInChildren<Text>().text = "Stop";
            n_slider.interactable = false;
            r1_slider.interactable = false;
            r2_slider.interactable = false;
            r3_slider.interactable = false;
            r4_slider.interactable = false;
            if (!isSequential) play_now = true;
        }
        else {
            GameObject.Find("Run").GetComponentInChildren<Text>().text = "Run";
            n_slider.interactable = true;
            r1_slider.interactable = true;
            r2_slider.interactable = true;
            r3_slider.interactable = true;
            r4_slider.interactable = true;
        }
    }

    public void setRandom()
    {
        if (parallel.IsAlive) parallel.Join();
        pool_locations.Clear();
        cell_location_matrix.dataClear();
        current_cell_list.Clear();
        current_alive.Clear();
        for (int i = 0; i < 12; i++) seq_play[i].Clear();

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
                        //var random_key = new Tuple<int, int, int>(i, j, k);
                        //current_cell_list.Add(random_key, 0);
                        current_cell_list.Add(idx(i, j, k, t_n));
                    }
                }
            }

        }
        //pool_locations.Enqueue(cell_location_matrix);
        painting_matrix.data = new Dictionary<Tuple<int, int, int>, byte>(cell_location_matrix.data);
        parallel = new Thread(GoL);
        parallel.Start();
        //StopCoroutine(UpdateDotView(false));
        //displaying_dots_list.Clear();
        //updating = false;
        //UpdateDotView();        
    }

    public void PresetGenerate()
    {
        cell_location_matrix.dataClear();
        current_cell_list.Clear();

        FileBrowser.RequestPermission();
        StartCoroutine(ShowLoadDialog());

        //UpdateDotView();
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
            if (updating)
            {
                StopCoroutine(update_coroutine);
                updating = false;
            }
            center_camera();
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
            }
            nums = lists.ConvertAll(int.Parse);

            if (parallel.IsAlive) parallel.Join();
            pool_locations.Clear();

            // List<int> rules_setter = new List<int>();
            // for (int i = 0; i < 5; i++)
            // {
            //     rules_setter.Add(nums[i]);
            //     UnityEngine.Debug.Log(rules_setter[i]);
            // }

            // UnityEngine.Debug.Log();

            n = nums[0];
            r1 = nums[1];
            r2 = nums[2];
            r3 = nums[3];
            r4 = nums[4];

            // UnityEngine.Debug.Log(n);
            // UnityEngine.Debug.Log(r1);
            // UnityEngine.Debug.Log(r2);

            n_slider.value = (float)n / 2;
            r1_slider.value = (float)r1;
            r2_slider.value = (float)r2;
            r3_slider.value = (float)r3;
            r4_slider.value = (float)r4;

            nInput.text = n.ToString();
            r1Input.text = r1.ToString();
            r2Input.text = r2.ToString();
            r3Input.text = r3.ToString();
            r4Input.text = r4.ToString();
            head_location = head_pref.transform.position;
            int pre_x = (int)head_location.x + (n / 2);
            int pre_y = (int)head_location.y + (n / 2);
            int pre_z = (int)head_location.z + (n / 2);
            cell_location_matrix.dataClear();
            current_cell_list.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
            painting_matrix.dataClear();
            for (int i = 5; i < nums.Count - 2; i += 3)
            {
                //cell_location_matrix[nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range] = 1;
                //var key1 = new Tuple<int, int, int>(nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range);
                ////current_cell_list.Add(key1, 0);
                //current_cell_list.Add(idx(key1.Item1,key1.Item2,key1.Item3,n));
                //painting_matrix[nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range] = 1;                                
                //current_cell_list.Add(key1, 0);
                cell_location_matrix[nums[i], nums[i + 1], nums[i + 2]] = 1;
                current_cell_list.Add(idx(nums[i], nums[i + 1], nums[i + 2], n));
                painting_matrix[nums[i], nums[i + 1], nums[i + 2]] = 1;
                //UnityEngine.Debug.Log(key1.Item1 + " " + key1.Item2 + " " + key1.Item3);

            }
        }
        follower_position = (-n / 2) - 1;
        center_camera();
        populate_display();
        t_n = n; t_r1 = r1; t_r2 = r2; t_r3 = r3; t_r4 = r4;
        parallel = new Thread(GoL);
        parallel.Start();
        //StopCoroutine(UpdateDotView(false));
        //displaying_dots_list.Clear();
        //updating = false;
    }

    public void Save()
    {
        FileBrowser.RequestPermission();
        StartCoroutine(ShowSaveDialog());
    }

    private IEnumerator ShowSaveDialog()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForSaveDialog(false, false, Application.streamingAssetsPath + "/Save/", "Save File", "Save");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        //Debug.Log(FileBrowser.Result[0]);

        if (FileBrowser.Success)
        {
            ////DO SOMETHING, IN
            Encoding enc = Encoding.GetEncoding("utf-8");
            writer = new StreamWriter(FileBrowser.Result[0], false, enc); //<<--- file to save the data

            writer.WriteLine("{0},{1},{2},{3},{4}", n, r1, r2, r3, r4);

            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
            {
                writer.WriteLine("{0},{1},{2}", e.Item1, e.Item2, e.Item3);
                writer.Flush();
            }

            writer.Close();
        }
    }
    public void Exit_game()
    {
        Application.Quit();
    }

}