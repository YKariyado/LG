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

public class GameManageSparse : MonoBehaviour
{
    [SerializeField] int n = 2048; //infinite universe
    private int t_n=2048;
    [SerializeField] int r1 = 4, r2 = 4, r3 = 0, r4 = 0; //rules
    private int t_r1 = 4, t_r2 = 4, t_r3 = 0, t_r4 = 0;
    [SerializeField] int range = 6; //apprear range

    int follower_position;

    Vector3 head_location; //change pos to location
    // public GameObject follower;

    [SerializeField] GameObject head_pref = default;
    [SerializeField] GameObject dot_pref = default;
    public GameObject follower;
    public GameObject mini_follower;
    public GameObject mini_pos;
    public GameObject Cells;

    // the list of dots that's displaying now
    // I need this when I delete all cells in a scene
    // 二重リスト
    //List<List<DotManageSparse>> displaying_dots_list = new List<List<DotManageSparse>>();
    List<GameObject> displaying_dots = new List<GameObject>();

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
    //private int pitch_freq;
    //private int pitch_channels;
    private AudioClip[,] pitch;
    int[] clips = new int[12];
    List<List<int>> seq_play;
    List<int> current_alive=new List<int>();
    int seq_start=0;

    float dotInterval = 1;
    public float bpm;
    //These are not constant.
    float BAR, BEAT;
    //every_bar is the time to refresh model with chords, every_beat is the time to refresh model with sequential, and delta_beat for calc every beat.
    float every_bar = 1, every_beat = 0, delta_time = 0;

    bool isRun = false, isPeriodic = true, isSequential = false, follow=false,follower_end=false;

    public InputField bpmInput, rangeInput, r1Input, r2Input, r3Input, r4Input, nInput,coor_x,coor_y,coor_z;
    public Slider bpm_slider, range_slider, r1_slider, r2_slider, r3_slider, r4_slider, n_slider;
    StreamWriter writer = null;
    public static string path = null;

    Thread parallel;
    bool updating = false,play_now=false,get_next=false;
    float threshold_update = 0.025f;
    Coroutine update_coroutine;

    // Awake is called before Start
    void Awake()
    {
        FileBrowser.SetDefaultFilter(".csv");
        DotManageSparse.SetOriginal(dot_pref);

        pitch = new AudioClip[12, 12 * 12];
        for (int i = 0; i < 12; ++i)
            for (int j = 0; j < 12 * 12; ++j)
                pitch[i, j] = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab_sparse"), "pitch_" + (i + 1).ToString()));
        //for (int i = 0; i < 12; i++)
        //{
        //    tmp = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab_sparse"), "pitch_" + (i + 1).ToString()));
        //    float[] temp = new float[tmp.samples*tmp.channels];
        //    tmp.GetData(temp, 0);            
        //    //pitch.Add(temp);
        //    //pitch_channels = tmp.channels;
        //    //pitch_freq = tmp.frequency;
        //}

        follower_position = (-n / 2)-1;

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

        UpdateDotView();
        if (isSequential && isRun) follower.GetComponent<Renderer>().enabled = true;
        else follower.GetComponent<Renderer>().enabled = false;

        parallel = new Thread(GoL);
    }

    void populate_display()
    {
        seq_play = new List<List<int>>();
        foreach (var e in displaying_dots) Destroy(e);
        displaying_dots.Clear();
        int count = 0;
        for (int k = -range; k < range; k++)
        {
            for (int j = -range; j < range; j++)
                for (int i = -range; i < range; i++)
                {
                    GameObject tmp = Instantiate<GameObject>(dot_pref, Cells.transform);
                    tmp.transform.localPosition = new Vector3(i * dotInterval, j * dotInterval, k * dotInterval);
                    tmp.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.HSVToRGB((float)count / ((Int64)Math.Pow(range * 2, 3)), 1f, 1f);
                    //tmp.GetComponent<AudioSource>().clip = AudioClip.Create("p_" + i.ToString()+ j.ToString()+ k.ToString(), pitch[0].Length / pitch_channels, pitch_channels, pitch_freq, false);
                    displaying_dots.Add(tmp);
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
                            ulong tmp2 = idx(x,y,z,t_n);
                            cell_list_for_judge.TryGetValue(tmp2,out temp);
                            cell_list_for_judge[tmp2] = (byte)(temp+1);
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
            cell_location_matrix.dataClear();
            current_cell_list.Clear();

            //add current cell's location **this takes a minute (means heavy process)**
            foreach (KeyValuePair<ulong,byte> e in cell_list_for_judge)
            {
                //this one commented
                //UnityEngine.Debug.Log(e.Key.Item1 + " " + e.Key.Item2 + " " + e.Key.Item3 + " " + e.Value);
                int[] tmp = iidx(e.Key, t_n);
                if (e.Value > t_r3 || e.Value < t_r4)
                {                    
                    cell_location_matrix[tmp[0],tmp[1],tmp[2]] = 0;
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
            for (int i = pre_x - range; i < pre_x + range; i++)
            {
                for (int j = pre_y - range; j < pre_y + range; j++)
                {
                    for (int k = pre_z - range; k < pre_z + range; k++)
                    {
                        if (cell_location_matrix[i, j, k] == 1)
                        {
                            //var remove_key = new Tuple<int, int, int>(i, j, k);
                            cell_location_matrix[i, j, k] = 0;
                            current_cell_list.Remove(idx(i,j,k,t_n));
                        }
                    }
                }
            }

            //add current cell's location **this takes a minute (means heavy process)**
            foreach (KeyValuePair<ulong,byte> e in cell_list_for_judge)
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
        if (!updating)
        {
            if (follower_position < -n / 2-1) follower_position = -(n / 2);
            //Moving a head
            head_location = head_pref.transform.position;
            //head_location.x = Mathf.Clamp(head_location.x, (-n / 2) + range, (n / 2) - range);
            //head_location.y = Mathf.Clamp(head_location.y, (-n / 2) + range, (n / 2) - range);
            //head_location.z = Mathf.Clamp(head_location.z, (-n / 2) + range, (n / 2) - range);
            head_pref.transform.position = new Vector3(head_location.x, head_location.y, head_location.z);
            //udpate mini map
            //UnityEngine.Debug.Log(new Vector3(head_location.x, head_location.y, head_location.z));
            Vector3 fix_head = new Vector3(head_location.x + (n / 2), head_location.y + (n / 2), head_location.z + (n / 2))/n;
            fix_head = (-2 * fix_head)+Vector3.up;
            mini_pos.transform.localPosition = fix_head;
            mini_pos.transform.localScale = (0.1f * Vector3.one) + Vector3.one*(1.9f*(1 - ((float)(n - 12) / (2048 - 12))));
            mini_follower.GetComponent<Renderer>().enabled = isSequential && isRun;
            mini_follower.transform.localPosition = new Vector3(-1, 0, -2*((follower_position+(float)n/2)/n));
        }               

        BAR = 4f / (bpm / 60f);
        BEAT = 1f / ((bpm / 60f) * 2f);        

        if (isRun)
        {            

            every_beat += Time.deltaTime;

            //View update
            //Updates the view when the player's position changes in integer increments            

            if (every_bar == 0 && !isSequential)
            {
                every_bar++;

                //await Task.Run(() =>
                //{
                //});                
                play_now = true;
                foreach (int e in current_alive) displaying_dots[e].GetComponent<AudioSource>().Play();
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
                    //follower_position = -n / 2;
                    //follower_end = true;
                }

                every_beat = 0;

                delta_time++;
                follower_position++;
                if (follower_position > n / 2) { follower_position = -n / 2;  }
                if (follower_position >= n / 2) { follower_end = true; }                
                if ((((int)head_location.z >= follower_position - range) || ((int)head_location.z <= follower_position + range)) && follower_position < n / 2)
                {
                    int tmp = (follower_position + n / 2) - seq_start;
                    foreach (int e in seq_play[tmp]) displaying_dots[e].GetComponent<AudioSource>().Play();
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
                every_bar = 0;
                every_beat = 0;

                // attach sounds to cells displayed
                // for (int i = (int)head_location.x + (n / 2) - range; i < (int)head_location.x + (n / 2) + range; i++)
                // {
                //     for (int j = (int)head_location.y + (n / 2) - range; j < (int)head_location.y + (n / 2) + range; j++)
                //     {
                //         for (int k = (int)head_location.z + (n / 2) - range; k < (int)head_location.z + (n / 2) + range; k++)
                //         {
                //             e.GetComponent<AudioSource>().Play();
                //         }
                //     }
                // }

            }

        }

        if (isSequential && isRun)
        {
            follower.GetComponent<Renderer>().enabled = true;
            if (follow)
            {
                if (head_location.z < follower_position - range)
                {
                    head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position - range);
                }
                else if (head_location.z > follower_position + range)
                {
                    head_pref.transform.position = new Vector3(head_location.x, head_location.y, follower_position + range);
                }
            }
            else
                if ((head_location.z < follower_position - range) || (head_location.z > follower_position + range))
                    follower.GetComponent<Renderer>().enabled = false;
        }
        else follower.GetComponent<Renderer>().enabled = false;

        if(isRun && isSequential && follower_end && updating)
        {
            StopCoroutine(update_coroutine);
            updating = false;
        }

        if (!updating)
        {
            // pooling して順次アプデしてく
            lock (pool_locations)
            {                
                if (pool_locations.Count > 0 && isRun)
                {
                    // dequeue a cell_locations_matrix
                    if ((!isSequential && get_next) || (isSequential && follower_end))
                    {
                        //painting_matrix.data = new Dictionary<Tuple<int, int, int>, byte>(pool_locations.Dequeue());
                        painting_matrix.data = pool_locations.Dequeue();
                        get_next = false;
                    }                    
                }
                UnityEngine.Debug.Log(play_now.ToString() + pool_locations.Count);
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
        if (!parallel.IsAlive && how_many < 20)
        {
            t_n = n; t_r1 = r1; t_r2 = r2; t_r3 = r3; t_r4 = r4;
            parallel = new Thread(GoL);
            parallel.Priority = System.Threading.ThreadPriority.Highest;
            parallel.Start();
        }

    }
    ulong idx(int x, int y, int z, int m) { return (ulong)x * (ulong)Math.Pow(m,2) + (ulong)y*(ulong)m+(ulong)z; }
    int[] iidx(ulong x, int m) { return new int[] { (int)(x / (ulong)Math.Pow(m, 2)), (int)((x % (ulong)Math.Pow(m, 2))/(ulong)m), (int)(x % (ulong)m) }; }

    public IEnumerator UpdateDotView()
    {
        updating = true;
        int count = 0;
        float delta_update = Time.realtimeSinceStartup + threshold_update;
        List<int> alive = new List<int>();        

        Vector3 pre= new Vector3 ( (int)head_location.x + (n / 2), (int)head_location.y + (n / 2), (int)head_location.z + (n / 2) );
        Vector3 actual_center = new Vector3((int)head_location.x, (int)head_location.y, (int)head_location.z);
        //UnityEngine.Debug.Log(head_location.ToString()+"\t"+actual_center.ToString());
        if (actual_center != Cells.transform.position) {
            Vector3 tmp = Cells.transform.position;
            if (range < pre.x && pre.x < n-range)
                tmp.x = actual_center.x;
            if(range < pre.y && pre.y < n-range)
                tmp.y = actual_center.y;
            if (range < pre.z && pre.z < n-range)
                tmp.z = actual_center.z;
            Cells.transform.position = tmp;
        }
        if (range > pre.x) pre.x = range;
        if (n-range < pre.x) pre.x = n-range;
        if (range > pre.y) pre.y = range;
        if (n - range < pre.y) pre.y = n - range;
        if (range > pre.z) pre.z = range;
        if (n - range < pre.z) pre.z = n - range;
        int k2 = 0;
        seq_start = (int)(pre.z - range);
        for (int k = (int)(pre.z - range); k < (pre.z + range); k++) 
        {
            seq_play[k2].Clear();
            for (int j = (int)( pre.y - range); j < ( pre.y + range); j++)
            {
                for (int i = (int)(pre.x - range); i < (pre.x + range); i++)
                {                                        

                    // 二重リストの一番ケツのリストの中に入れる                    
                    if (painting_matrix[i, j, k] == 1)
                    {                        
                        //appear alive
                        displaying_dots[count].transform.GetChild(0).gameObject.SetActive(true);
                        if (isRun)
                        {
                            displaying_dots[count].GetComponent<AudioSource>().clip = pitch[i % 12, clips[i % 12]];
                            clips[i % 12] = (clips[i % 12] + 1) % (12 * 12);
                            float freq = Mathf.Pow(10, (2 + i * ((4f - 2f) / (n - 1))));
                            float q = 1 + 9f*(float)k / n;
                            displaying_dots[count].GetComponent<AudioLowPassFilter>().cutoffFrequency = freq;
                            displaying_dots[count].GetComponent<AudioLowPassFilter>().lowpassResonanceQ = q;
                            displaying_dots[count].GetComponent<AudioHighPassFilter>().cutoffFrequency = freq;
                            displaying_dots[count].GetComponent<AudioHighPassFilter>().highpassResonanceQ = q;
                            //displaying_dots[count].GetComponent<AudioSource>().clip = AudioClip.Create("p_"+(i%12).ToString(),pitch[i%12].Item1.Length/ pitch[i % 12].Item3, pitch[i % 12].Item3, pitch[i % 12].Item2,false);
                            //displaying_dots[count].GetComponent<AudioSource>().clip.SetData(pitch[i%12],0);                            
                            if (isSequential)
                            {
                                seq_play[k2].Add(count);
                                //if (k == follower_position + n / 2)
                                //    displaying_dots[count].GetComponent<AudioSource>().Play();
                            }
                            else alive.Add(count);
                        }                   
                    }
                    else
                    {
                        //appear dead
                        //if(displaying_dots[count].GetComponent<AudioSource>().isPlaying) displaying_dots[count].GetComponent<AudioSource>().Stop();                        
                        //displaying_dots[count].GetComponent<AudioSource>().clip = null;
                        displaying_dots[count].transform.GetChild(0).gameObject.SetActive(false);                        
                    }
                    count++;
                }
            }
            k2 ++;
            if (Time.realtimeSinceStartup > delta_update)
            {
                yield return null;
                delta_update = Time.realtimeSinceStartup + threshold_update;
            }
        }
        if (play_now) {
            //foreach (var e in alive) displaying_dots[e].GetComponent<AudioSource>().Play();
            current_alive.Clear();
            current_alive = new List<int>(alive);
            play_now = false;
            get_next = true;
        }
        //if(!isSequential && play_sound)
        //    foreach(List<DotManageSparse> e1 in displaying_dots_list)            
        //        foreach(DotManageSparse e2 in e1)
        //            if(e2.transform.GetChild(0).gameObject.active) e2.GetComponent<AudioSource>().Play();        
        updating = false;
    }

    //public IEnumerator UpdateDotView(bool play_sound)
    //{
    //    float count = 0f;
    //    float delta_update = Time.realtimeSinceStartup + threshold_update;
    //    List<(int,int)> alive = new List<(int,int)>();
    //    int id_0 = 0;
    //    int id_1 = 0;

    //    int pre_x = (int)head_location.x + (n / 2);
    //    int pre_y = (int)head_location.y + (n / 2);
    //    int pre_z = (int)head_location.z + (n / 2);

    //    updating = true;
    //    //UnityEngine.Debug.Log(new Vector3(pre_x, pre_y, pre_z));
    //    // try-catch to pool_locations


    //    for (int i = (pre_x - range<0?0: pre_x - range) ; i < (pre_x + range >n?n: pre_x + range); i++)
    //    {
    //        // why????? := 12以上で一番最初のを消してケツに新しいのを入れるらしい
    //        if (displaying_dots_list.Count >= range * 2)
    //        {
    //            // displaying_dots_list[0] is one of lists that contains cells displayed
    //            foreach (DotManageSparse e in displaying_dots_list[0])
    //            {
    //                if (e.GetComponent<AudioSource>().isPlaying)
    //                {
    //                    yield return new WaitForSeconds(0.01f);
    //                    delta_update = Time.realtimeSinceStartup + threshold_update;
    //                }
    //                DotManageSparse.Pool(e);
    //            }
    //            displaying_dots_list[0].Clear();
    //            displaying_dots_list.RemoveAt(0);
    //        }

    //        // オンにしたらめちゃ更新してた...どうすればええんや...
    //        // UnityEngine.Debug.Log(displaying_dots_list.Count());

    //        displaying_dots_list.Add(new List<DotManageSparse>());
    //        id_1 = 0;
    //        for (int j = (pre_y - range < 0 ? 0 : pre_y - range); j < (pre_y + range > n ? n : pre_y + range); j++)
    //        {
    //            for (int k = (pre_z - range < 0 ? 0 : pre_z - range); k < (pre_z + range > n ? n : pre_z + range); k++)
    //            {
    //                count++;
    //                DotManageSparse displaying_dot = DotManageSparse.Create();
    //                displaying_dot.transform.position = new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k));
    //                displaying_dot.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.HSVToRGB(count / ((Int64)Math.Pow(range * 2, 3)), 1f, 1f);

    //                // 二重リストの一番ケツのリストの中に入れる                    
    //                if (painting_matrix[i, j, k] == 1)
    //                {
    //                    //appear alive
    //                    displaying_dot.transform.GetChild(0).gameObject.SetActive(true);

    //                    //UnityEngine.Debug.Log((int)Math.Abs((dotInterval * (-n / 2.0f + j)) % 12));
    //                    displaying_dot.GetComponent<AudioSource>().clip = pitch[j % 12];
    //                    float freq = Mathf.Pow(10,(2 + i * ((4f - 2f) / (n-1))));                        
    //                    displaying_dot.GetComponent<AudioLowPassFilter>().cutoffFrequency = freq;
    //                    displaying_dot.GetComponent<AudioHighPassFilter>().cutoffFrequency = freq;
    //                    //displaying_dot.GetComponent<AudioReverbFilter>().dryLevel = -10000+10000*(float)(k / n);
    //                    if (play_sound)
    //                    {
    //                        if (isSequential) {
    //                            if (k == follower_position + n / 2)
    //                                displaying_dot.GetComponent<AudioSource>().Play();
    //                        }
    //                        else alive.Add((id_0, id_1));
    //                        //else displaying_dot.GetComponent<AudioSource>().Play();                            
    //                    }                        
    //                    // // attach sounds
    //                    // displaying_dot.GetComponent<AudioSource>().clip = pitch[(int)Math.Abs(((dotInterval * (-n / 2.0f + j)) + 6) % 12)];
    //                    // if (every_bar == 0)
    //                    //     displaying_dot.GetComponent<AudioSource>().Play();
    //                }
    //                else
    //                {
    //                    //appear dead
    //                    displaying_dot.transform.GetChild(0).gameObject.SetActive(false);
    //                }
    //                displaying_dots_list[displaying_dots_list.Count - 1].Add(displaying_dot);
    //                id_1 += 1;
    //            }                
    //        }
    //        id_0 += 1;
    //        if (Time.realtimeSinceStartup > delta_update)
    //        {
    //            yield return null;
    //            delta_update = Time.realtimeSinceStartup + threshold_update;
    //        }
    //    }
    //    //if(alive.Count>0)UnityEngine.Debug.Log(alive.Count());
    //    foreach (var e in alive)
    //    {
    //        displaying_dots_list[e.Item1][e.Item2].GetComponent<AudioSource>().Play();
    //    }
    //    //if(!isSequential && play_sound)
    //    //    foreach(List<DotManageSparse> e1 in displaying_dots_list)            
    //    //        foreach(DotManageSparse e2 in e1)
    //    //            if(e2.transform.GetChild(0).gameObject.active) e2.GetComponent<AudioSource>().Play();
    //    updating = false;
    //}

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
        if (n != 2 * (int)n_slider.value)
        {
            n = 2 * (int)n_slider.value;
            if (head_pref.transform.position.x<0 || head_pref.transform.position.x > n || head_pref.transform.position.y < 0 || head_pref.transform.position.y > n || head_pref.transform.position.z < 0 || head_pref.transform.position.z > n)            
                head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);
            follower_position = (-n / 2)-1;
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();
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
            foreach (Tuple<int, int, int> e in painting_matrix.data.Keys)
                current_cell_list.Add(idx(e.Item1, e.Item2, e.Item3, n));
            painting_matrix[x, y, z] = 1;
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
        else z = Int32.Parse(coor_x.text);
        if (x >= 0 && x < n && y >= 0 && y < n && z >= 0 && z < n) {
            head_pref.transform.position = new Vector3(x - n / 2, y - n / 2, z - n / 2);
            Vector3 tmp = head_pref.transform.position;
            if (range > x) tmp.x = range-n/2;
            if (n - range < x) tmp.x = n/2 - range;
            if (range > y) tmp.y = range-n/2;
            if (n - range < y) tmp.y = n/2 - range;
            if (range > z) tmp.z = range-n/2;
            if (n - range < z) tmp.z = n/2 - range;
            Cells.transform.position = tmp;
        }
    }

    public void center_camera()
    {
        head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);
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
    }

    public void on_follow()
    {        
        follow = !follow;
    }

    public void RunStop()
    {
        isRun = !isRun;
        if (isRun) GameObject.Find("Run").GetComponentInChildren<Text>().text = "Stop";
        else GameObject.Find("Run").GetComponentInChildren<Text>().text = "Run";
    }

    public void setRandom()
    {
        if(parallel.IsAlive) parallel.Join();
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
                        current_cell_list.Add(idx(i,j,k,t_n));
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

            int pre_x = (int)head_location.x + (n / 2);
            int pre_y = (int)head_location.y + (n / 2);
            int pre_z = (int)head_location.z + (n / 2);
            cell_location_matrix.dataClear();
            current_cell_list.Clear();
            current_alive.Clear();
            for (int i = 0; i < 12; i++) seq_play[i].Clear();

            for (int i = 5; i < nums.Count - 2; i += 3)
            {
                cell_location_matrix[nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range] = 1;
                var key1 = new Tuple<int, int, int>(nums[i] + pre_x - range, nums[i + 1] + pre_y - range, nums[i + 2] + pre_z - range);
                //current_cell_list.Add(key1, 0);
                current_cell_list.Add(idx(key1.Item1,key1.Item2,key1.Item3,n));

                //UnityEngine.Debug.Log(key1.Item1 + " " + key1.Item2 + " " + key1.Item3);

            }
        }
        painting_matrix.data = new Dictionary<Tuple<int, int, int>, byte>(cell_location_matrix.data);
        follower_position = (-n / 2)-1;
        populate_display();
        head_pref.transform.position = new Vector3(((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f, ((-n / 2.0f) + ((n / 2.0f) - 1)) / 2.0f);
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

            writer.WriteLine("{0},{1},{2},{3},{4}",n, r1, r2, r3, r4);

            foreach (Tuple<int,int,int> e in  painting_matrix.data.Keys)
            {
                writer.WriteLine("{0},{1},{2}", e.Item1, e.Item2, e.Item3);
                writer.Flush();
            }

            writer.Close();
        }
    }

}
