using HTC.UnityPlugin.Vive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ExperimenManger : MonoBehaviour
{    
    //private Sparse3DArray<byte> Cubes= new Sparse3DArray<byte>();
    public GameObject cube_prefab;
    public GameObject cube_prefab_miss;
    public GameObject r_hand;
    public GameObject l_hand;
    public string stundet_id;
    // Start is called before the first frame update
    RaycastHit hit;
    Ray ray;
    public int points = 0;
    public int miss = 0;
    public float timing=600;    
    public Text points_text;
    public Text timing_text;
    int count = 0,count2=0;
    bool ended = false,recored=false,started=false;
    StringBuilder log = new StringBuilder();
    void Start()
    {
        //initiailization variables  
        List<List<int>> uni = new List<List<int>>();        
        List<List<List<int>>> patterns = new List<List<List<int>>>();
        List<List<int>> tmp = new List<List<int>>();        
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 1, 0, 0 }); tmp.Add(new List<int> { 1, 1, 0 }); tmp.Add(new List<int> { 1, 1, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        //1 is the pattern.
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 0, 0, 1 }); tmp.Add(new List<int> { 1, 1, 0 }); tmp.Add(new List<int> { 1, 1, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 1, 0, 0 }); tmp.Add(new List<int> { 0, 1, 0 }); tmp.Add(new List<int> { 0, 0, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 1, 0, 1 }); tmp.Add(new List<int> { 1, 1, 0 }); tmp.Add(new List<int> { 0, 1, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        /////////////////
        //create universe
        /////////////////
        //uni.Add(new List<int> { 36, 4, 4, 0, 0 });
        //List<Vector3> pos = new List<Vector3>();
        //for (int i = -uni[0][0] / 2 + 2; i < uni[0][0] / 2 - 2; i += 4)
        //{
        //    for (int j = -uni[0][0] / 2 + 2; j < uni[0][0] / 2 - 2; j += 4)
        //    {
        //        for (int k = -uni[0][0] / 2 + 2; k < uni[0][0] / 2 - 2; k += 4)
        //        {
        //            pos.Add(new Vector3(i, j, k));
        //        }
        //    }
        //}
        //UnityEngine.Debug.Log(pos.Count);
        //List<int> n_p = new List<int>();
        //for (int i = 0; i < patterns.Count; i++) for (int j = 0; j < 100; j++) n_p.Add(i);
        //var sb = new StringBuilder();
        //sb.AppendLine($"{uni[0][0]},{uni[0][1]},{uni[0][2]},{uni[0][3]},{uni[0][4]}");
        //while (n_p.Count > 0)
        //{
        //    int p = (int)Mathf.Floor(UnityEngine.Random.Range(0, n_p.Count));
        //    int p2 = (int)Mathf.Floor(UnityEngine.Random.Range(0, pos.Count));
        //    sb.AppendLine($"{pos[p2].x},{pos[p2].y},{pos[p2].z},{n_p[p]}");
        //    //v = new Vector3(pos[p2].x + uni[0][0] / 2, pos[p2].y + uni[0][0] / 2, pos[p2].z + uni[0][0] / 2);
        //    //if (n_p[p] == 1) count++;
        //    //else count2++;
        //    //Instantiate<GameObject>(n_p[p] == 1 ? cube_prefab : cube_prefab_miss, pos[p2] + Vector3.one * 0.5f, Quaternion.identity);
        //    //foreach (var e in patterns[n_p[p]]) uni.Add(new List<int> { e[0] + (int)v.x, e[1] + (int)v.y, e[2] + (int)v.z });
        //    n_p.RemoveAt(p);
        //    pos.RemoveAt(p2);
        //}
        //File.WriteAllText(Application.streamingAssetsPath + "/Experiment/universe.csv", sb.ToString());
        /////////////////
        //load universe
        /////////////////
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Experiment/universe.csv");
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
        uni.Add(new List<int> { nums[0], nums[1], nums[2], nums[3], nums[4] });
        Vector3 v;
        for (int i = 5; i < nums.Count; i += 4)
        {
            v = new Vector3(nums[i] + uni[0][0] / 2, nums[i + 1] + uni[0][0] / 2, nums[i + 2] + uni[0][0] / 2);
            if (nums[i + 3] == 1) count++;
            else count2++;
            Instantiate<GameObject>(nums[i + 3] == 1 ? cube_prefab : cube_prefab_miss, new Vector3(nums[i], nums[i + 1], nums[i + 2]) + Vector3.one * 0.5f, Quaternion.identity);
            foreach (var e in patterns[nums[i + 3]]) uni.Add(new List<int> { e[0] + (int)v.x, e[1] + (int)v.y, e[2] + (int)v.z });
        }
        this.GetComponent<VRGameManageSparse>().LoadUniverse(uni);
        log.AppendLine("is_target,is_miss,user_x,user_y,user_z,obj_x,obj_y,obj_z,n_targets,n_distractors,total_points,total_miss,time");
    }
    void RaycastFunc(RaycastHit hit)
    {
        if (hit.collider.gameObject.tag == "Player" && hit.distance <= 6 && timing > 0)
        {
            if (hit.collider.gameObject.GetComponent<ParticleSystem>().isPlaying) return;
            foreach (Transform t in hit.collider.gameObject.transform.parent)
            {
                if (t.gameObject.tag != "Player") t.gameObject.SetActive(true);
            }
            hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
            points++;
            Vector3 cam_pos = this.GetComponent<VRGameManageSparse>().main_camera.transform.position;
            Vector3 obj_pos = hit.collider.gameObject.transform.position;
            log.AppendLine($"1,0,{cam_pos.x},{cam_pos.y},{cam_pos.z},{obj_pos.x},{obj_pos.y},{obj_pos.z},{count},{count2},{points},{miss}," + string.Format("{0:00}:{1:00}", (int)timing / 60, (int)timing % 60));
        }
        else if (hit.collider.gameObject.tag == "GameController" && hit.distance <= 6 && timing > 0)
        {
            if (hit.collider.gameObject.GetComponent<ParticleSystem>().isPlaying) return;            
            hit.collider.gameObject.GetComponent<ParticleSystem>().Play();            
            Vector3 cam_pos = this.GetComponent<VRGameManageSparse>().main_camera.transform.position;
            Vector3 obj_pos = hit.collider.gameObject.transform.position;
            Instantiate<GameObject>(cube_prefab_miss,obj_pos, Quaternion.identity);
            miss++;            
            log.AppendLine($"0,1,{cam_pos.x},{cam_pos.y},{cam_pos.z},{obj_pos.x},{obj_pos.y},{obj_pos.z},{count},{count2},{points},{miss}," + string.Format("{0:00}:{1:00}", (int)timing / 60, (int)timing % 60));
        }
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_STANDALONE_WIN
                if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu) || ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Menu)) started = true;
                //UnityEngine.Debug.DrawLine(r_hand.transform.position, r_hand.transform.position + r_hand.transform.forward,Color.black);
                if (ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Trigger) && !this.GetComponent<VRGameManageSparse>().is_in_menu())
                {
                    if (Physics.Raycast(r_hand.transform.position, r_hand.transform.forward, out hit))
                    {
                        RaycastFunc(hit);
                    }
                }
                if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger) && !this.GetComponent<VRGameManageSparse>().is_in_menu())
                {
                    if (Physics.Raycast(l_hand.transform.position, l_hand.transform.forward, out hit))
                    {
                        RaycastFunc(hit);
                    }
                }
        #endif
        if (Input.GetKeyDown(KeyCode.Escape)) started = true ;
        if (Input.GetMouseButtonDown(1) && !this.GetComponent<VRGameManageSparse>().is_in_menu())
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);            

            if (Physics.Raycast(ray, out hit))
            {
                RaycastFunc(hit);
            }
        }
        if (timing > 0 && points < count &&started) timing -= Time.deltaTime;
        if(points >= count || timing<=0) ended = true;
        if (timing<0) timing = 0;        
        if ((timing / 60) <= 1) timing_text.color = Color.red;
        if (points >= count) { timing_text.color = Color.green; points_text.color = Color.green; }
        timing_text.text = string.Format("{0:00}:{1:00}", (int)timing/60, (int)timing%60);
        points_text.text = "Points: " + points.ToString();
        if (ended&&!recored)
        {
            Vector3 cam_pos = this.GetComponent<VRGameManageSparse>().main_camera.transform.position;
            log.AppendLine($"0,0,{cam_pos.x},{cam_pos.y},{cam_pos.z},null,null,null,{count},{count2},{points},{miss}," + string.Format("{0:00}:{1:00}", (int)timing / 60, (int)timing % 60));
            long timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            File.WriteAllText((stundet_id==""?timestamp.ToString():stundet_id)+ ".csv", log.ToString());
            recored = true;
        }
    }
    private void OnDestroy()
    {
        if (!recored)
        {            
            log.AppendLine($"0,0,null,null,null,null,null,null,{count},{count2},{points},{miss}," + string.Format("{0:00}:{1:00}", (int)timing / 60, (int)timing % 60));
            long timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            File.WriteAllText((stundet_id == "" ? timestamp.ToString() : stundet_id) + ".csv", log.ToString());
            recored = true;
        }
    }
}
