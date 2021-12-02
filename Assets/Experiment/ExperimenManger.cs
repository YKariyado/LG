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
    void Start()
    {

        List<List<int>> uni = new List<List<int>>();
        uni.Add(new List<int>{12,4,4,0,0});        
        List<List<List<int>>> patterns = new List<List<List<int>>>();
        List<List<int>> tmp = new List<List<int>>();        
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 1, 0, 0 }); tmp.Add(new List<int> { 1, 1, 0 }); tmp.Add(new List<int> { 1, 1, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        //1 is the pattern.
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 0, 0, 1 }); tmp.Add(new List<int> { 1, 1, 0 }); tmp.Add(new List<int> { 1, 1, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 1, 0, 0 }); tmp.Add(new List<int> { 0, 1, 0 }); tmp.Add(new List<int> { 0, 0, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        tmp.Add(new List<int> { 0, 0, 0 }); tmp.Add(new List<int> { 1, 0, 1 }); tmp.Add(new List<int> { 1, 1, 0 }); tmp.Add(new List<int> { 0, 1, 1 }); patterns.Add(new List<List<int>>(tmp)); tmp.Clear();
        List<Vector3> pos=new List<Vector3>();
        for (int i = -uni[0][0]/2+2; i < uni[0][0]/2-2; i+=4)
        {
            for (int j = -uni[0][0] / 2+2; j < uni[0][0] / 2-2; j+=4)
            {
                for (int k = -uni[0][0] / 2+2; k < uni[0][0] / 2-2; k+=4)
                {
                    pos.Add(new Vector3(i,j,k));              
                }
            }     
        }
        UnityEngine.Debug.Log(pos.Count);
        List<int> n_p = new List<int>();
        for (int i = 0; i < patterns.Count; i++) for (int j = 0; j < 1; j++) n_p.Add(i);
        int p,p2;
        Vector3 v;
        while (n_p.Count > 0)
        {            
            p = (int)Mathf.Floor(UnityEngine.Random.Range(0,n_p.Count));
            p2= (int)Mathf.Floor(UnityEngine.Random.Range(0, pos.Count));
            v = new Vector3(pos[p2].x+ uni[0][0] / 2, pos[p2].y+ uni[0][0] / 2, pos[p2].z+ uni[0][0] / 2);
            if (n_p[p] == 1) count++;
            else count2++;
            Instantiate<GameObject>(n_p[p]==1?cube_prefab:cube_prefab_miss, pos[p2]+Vector3.one*0.5f, Quaternion.identity);            
            foreach (var e in patterns[n_p[p]]) uni.Add(new List<int> { e[0]+(int)v.x ,e[1]+(int)v.y,e[2]+(int)v.z });
            n_p.RemoveAt(p);
            pos.RemoveAt(p2);
        }        
        this.GetComponent<VRGameManageSparse>().LoadUniverse(uni);
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
                if (hit.collider.gameObject.tag == "Player" && hit.distance <= 6 && timing > 0)
                {
                    foreach (Transform t in hit.collider.gameObject.transform.parent)
                    {
                        if (t.gameObject.tag != "Player") t.gameObject.SetActive(true);
                    }
                    hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
                    points++;
                }
                else if (hit.collider.gameObject.tag == "GameController" && hit.distance <= 6 && timing > 0)
                {
                    foreach (Transform t in hit.collider.gameObject.transform.parent)
                    {
                        if (t.gameObject.tag != "GameController") t.gameObject.SetActive(true);
                    }
                    hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
                    miss++;
                }
            }
        }
        if (ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Trigger) && !this.GetComponent<VRGameManageSparse>().is_in_menu())
        {
            if (Physics.Raycast(l_hand.transform.position, l_hand.transform.forward, out hit))
            {
                if (hit.collider.gameObject.tag == "Player" && hit.distance <= 6 && timing > 0)
                {
                    foreach (Transform t in hit.collider.gameObject.transform.parent)
                    {
                        if (t.gameObject.tag != "Player") t.gameObject.SetActive(true);
                    }
                    hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
                    points++;
                }
                else if (hit.collider.gameObject.tag == "GameController" && hit.distance <= 6 && timing > 0)
                {
                    foreach (Transform t in hit.collider.gameObject.transform.parent)
                    {
                        if (t.gameObject.tag != "GameController") t.gameObject.SetActive(true);
                    }
                    hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
                    miss++;
                }
            }
        }
#endif
        if (Input.GetKeyDown(KeyCode.Escape)) started = true ;
        if (Input.GetMouseButtonDown(1) && !this.GetComponent<VRGameManageSparse>().is_in_menu())
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);            

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.tag == "Player" && hit.distance<=6 && timing>0)
                {
                    foreach(Transform t in hit.collider.gameObject.transform.parent)
                    {
                        if (t.gameObject.tag != "Player") t.gameObject.SetActive(true);
                    }                    
                    hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
                    points++;
                }
                else if (hit.collider.gameObject.tag == "GameController" && hit.distance <= 6 && timing > 0)
                {
                    foreach (Transform t in hit.collider.gameObject.transform.parent)
                    {
                        if (t.gameObject.tag != "GameController") t.gameObject.SetActive(true);
                    }
                    hit.collider.gameObject.GetComponent<ParticleSystem>().Play();
                    miss++;
                }
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
            var sb = new StringBuilder();
            sb.AppendLine("points,miss,targets,distraction,time");
            sb.AppendLine($"{points},{miss},{count},{count2},"+ string.Format("{0:00}:{1:00}", (int)timing / 60, (int)timing % 60));
            long timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            File.WriteAllText(timestamp.ToString()+ ".csv", sb.ToString());
            recored = true;
        }
    }
}
