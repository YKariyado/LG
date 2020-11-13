using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.IO;
using System.Text;
using SimpleFileBrowser;
using System.Linq;

public class GameManageNormal : MonoBehaviour
{

    int n = 8, time = 0; //a side
    public int r1, r2, r3, r4;

    public float dotInterval;
    public float bpm;
    float bar, beat;
    float timeRecent = 1, timeRecent2 = 0;

    public GameObject dotPref; //dot prefab
    public GameObject[,,] dots; //dots array
    public Slider bpm_slider;
    public GameObject head;
    public GameObject follower;

    public List<GameObject> alives = new List<GameObject>();
    public List<GameObject> deads = new List<GameObject>();

    public List<GameObject> cpalives;

    StreamWriter writer = null;
    public static string path = null;

    public InputField r1Input, r2Input, r3Input, r4Input;

    private AudioClip[] drum_machine;
    private  AudioClip[,,] sounds_matlab;

    bool isRun;
    bool isOn;
    public bool sequential=false;    

    private void Awake()
    {
        FileBrowser.SetDefaultFilter(".csv");
        drum_machine = new AudioClip[n];
        int c = 0;
        foreach (var i in new string[] { "kick", "snare", "clap", "tom", "chats", "ohats", "crash", "bass" }) {            
            drum_machine[c] = Resources.Load<AudioClip>(Path.Combine("Sounds",Path.Combine("drum_machine",i)));
            c++;
        }
       sounds_matlab = new AudioClip[n, n, n];
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                for (int k = 1; k <= n; k++)
                {                    
                    sounds_matlab[i - 1, j - 1, k - 1] = Resources.Load<AudioClip>(Path.Combine(Path.Combine("Sounds", "sounds_matlab"), i.ToString() + "_" + j.ToString() + "_" + k.ToString()));
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject all = GameObject.Find("AllDots");
        dots = new GameObject[n, n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    GameObject obj = Instantiate(dotPref, new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k)), Quaternion.identity, all.transform); // Generate dot prefabs from -n/2
                    obj.transform.localPosition = new Vector3(dotInterval * (-n / 2.0f + i)+0.5f, dotInterval * (-n / 2.0f + j) + 0.5f, dotInterval * (-n / 2.0f + k) + 0.5f);
                    //obj.transform.parent = all.transform;
                    obj.GetComponent<AudioSource>().volume = 1f / n;
                    //obj.GetComponent<AudioSource>().clip= Resources.Load<AudioClip>("sounds_matlab/" + (k+1).ToString() + "_" + (j+1).ToString() + "_" + (k+1).ToString());
                    dots[i, j, k] = obj;
                    dots[i, j, k].GetComponent<DotManage>().x = i;
                    dots[i, j, k].GetComponent<DotManage>().y = j;
                    dots[i, j, k].GetComponent<DotManage>().z = k;

                    //coloring
                    float floati = i+1, floatj = j+1, floatk = k+1;
                    dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(floati/6f, floatj/6f, floatk/6f);
                }
            }

        }

        Data.Instance.referer = "GoL";
        if (sequential&&isRun) follower.GetComponent<Renderer>().enabled = true;
        else follower.GetComponent<Renderer>().enabled = false;
    }

    public void change_bpm()
    {
        bpm = bpm_slider.value;
    }

    // Update is called once per frame
    void Update()
    {

        Data.Instance.referer = "GoL";

        //rotate head :D
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            head.transform.Rotate(0,-0.5f, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            head.transform.Rotate(0,0.5f,0);
        }
        if (Input.GetKey(KeyCode.UpArrow) && (head.transform.eulerAngles.x>270 || head.transform.eulerAngles.x<10))
        {
            head.transform.Rotate(-0.5f, 0,  0);
        }
        if (Input.GetKey(KeyCode.DownArrow) && (head.transform.eulerAngles.x>=260 && head.transform.eulerAngles.x<360))
        {
            head.transform.Rotate(0.5f, 0,  0);
        }
        
        bar = 4f / (bpm / 60f);
        beat = 1f / ((bpm / 60f) * 2f);

        if (isRun)
        {
            if (sequential) follower.GetComponent<Renderer>().enabled = true;
            else follower.GetComponent<Renderer>().enabled = false;

            timeRecent2 += Time.deltaTime;

            if (timeRecent == 0)
            {
                timeRecent++;

                foreach (GameObject e in alives)
                {

                    DotManage temp = e.GetComponent<DotManage>();

                    if (isOn)
                    {
                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                for (int k = -1; k < 2; k++)
                                {

                                    int x = temp.x + i, y = temp.y + j, z = temp.z + k;

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

                                    dots[x, y, z].GetComponent<DotManage>().neighbor++;

                                }
                            }
                        }
                    }
                    else
                    {

                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                for (int k = -1; k < 2; k++)
                                {
                                    //if ((temp.x + i < 0 || temp.x + i >= n || temp.y + j < 0 || temp.y + j >= n || temp.z + k < 0 || temp.z + k >= n)
                                    //    || (temp.x + i == 0 && temp.y + j == 0 && temp.z + k == 0))

                                    if ((temp.x + i < 0 || temp.x + i >= n || temp.y + j < 0 || temp.y + j >= n || temp.z + k < 0 || temp.z + k >= n))
                                    {
                                        //DO NOTHING
                                    }
                                    else
                                    {
                                        dots[temp.x + i, temp.y + j, temp.z + k].GetComponent<DotManage>().neighbor++;
                                    }
                                }
                            }
                        }

                        //if (temp.neighbor > r3 || temp.neighbor < r4)
                        //{
                        //    temp.dotDestroy();
                        //}

                    }

                }

                foreach (GameObject e in alives)
                {

                    DotManage temp = e.GetComponent<DotManage>();

                    if (temp.neighbor > r3 || temp.neighbor < r4)
                    {
                        temp.dotDestroy();
                    }

                    e.GetComponent<DotManage>().neighbor = 0;　//次ループのために初期化
                }

                foreach (GameObject e in deads)
                {

                    DotManage tmp = e.GetComponent<DotManage>();

                    if (tmp.neighbor <= r2 && tmp.neighbor >= r1)
                    {
                        tmp.dotGenerate();
                    }

                    tmp.neighbor = 0; //次ループのために初期化

                }

                alives.Clear();
                deads.Clear();

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < n; k++)
                        {
                            if (dots[i, j, k].GetComponent<DotManage>().isAlive)
                            {
                                alives.Add(dots[i, j, k]); //List in
                            }
                            else
                            {
                                deads.Add(dots[i, j, k]); //List in
                            }

                        }
                    }
                }

            }


            //matlab_sound
            if (timeRecent2 >= beat && sequential)
            {

                time = time % n;

                if (time == 0) {
                    timeRecent = 0;
                }

                timeRecent2 = 0;

                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        if (dots[j, k, time].GetComponent<DotManage>().isAlive)
                        {
                            //PLEASE DONT CHNAGE THIS PART
                            dots[j, k, time].GetComponent<AudioSource>().clip = sounds_matlab[j, k, time];
                            dots[j, k, time].GetComponent<AudioSource>().Play();
                        }
                    }
                }

                //PLEASE DONT CHNAGE THIS PART
                follower.transform.localPosition = new Vector3(follower.transform.localPosition.x, follower.transform.localPosition.y, dots[0, 0, time].transform.localPosition.z);
                time++;

            }

            if (timeRecent2 >= bar && !sequential) //with sequential option
            {

                timeRecent = 0;
                timeRecent2 = 0;

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < n; k++)
                        {
                            if (dots[j, i, k].GetComponent<DotManage>().isAlive)
                            {
                                dots[j, i, k].GetComponent<AudioSource>().clip = sounds_matlab[j, i, k];
                                dots[j, i, k].GetComponent<AudioSource>().Play();
                            }
                        }
                    }
                }

                Debug.Log(alives.Count());

            }

            /**
            * For the brave souls who get this far: You are the chosen ones,
            * the valiant knights of programming who toil away, without rest,
            * fixing our most awful code. To you, true saviors, kings of men,
            * I say this: never gonna give you up, never gonna let you down,
            * never gonna run around and desert you. Never gonna make you cry,
            * never gonna say goodbye. Never gonna tell a lie and hurt you.
            */


        }
        else {
            follower.GetComponent<Renderer>().enabled = false;
        }

    }


    public void RandomGenerate()
    {
        cpalives = new List<GameObject>();

        alives.Clear();
        deads.Clear();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    if (UnityEngine.Random.Range(0, 5) == 0)
                    {
                        dots[i, j, k].GetComponent<DotManage>().dotGenerate();
                        alives.Add(dots[i, j, k]); //List in
                    }
                    else
                    {
                        dots[i, j, k].GetComponent<DotManage>().dotDestroy();
                        deads.Add(dots[i, j, k]); //List in
                    }

                }
            }
        }

        cpalives = new List<GameObject>(alives);

    }

    public void PresetOneGenerate()
    {

        alives.Clear();
        deads.Clear();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    dots[i, j, k].GetComponent<DotManage>().dotDestroy();
                    deads.Add(dots[i, j, k]); //List in

                }
            }
        }

        FileBrowser.RequestPermission();
        StartCoroutine(ShowLoadDialog());

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

            for (int i = 0; i < nums.Count - 2; i += 3)
            {
                dots[nums[i], nums[i + 1], nums[i + 2]].GetComponent<DotManage>().dotGenerate();
                alives.Add(dots[nums[i], nums[i + 1], nums[i + 2]]);
            }
        }
    }

    public void Save()
    {
        FileBrowser.RequestPermission();
        StartCoroutine(ShowSaveDialog());
    }

    public void setR1()
    {
        r1 = int.Parse(r1Input.text);
        if (r1 < 0)
        {
            r1 = -1;
        }
    }

    public void setR2()
    {
        r2 = int.Parse(r2Input.text);
        if (r2 < 0)
        {
            r2 = -1;
        }
    }

    public void setR3()
    {
        r3 = int.Parse(r3Input.text);
        if (r3 < 0)
        {
            r3 = -1;
        }
    }

    public void setR4()
    {
        r4 = int.Parse(r4Input.text);
        if (r4 < 0)
        {
            r4 = -1;
        }
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

            foreach (GameObject e in cpalives)
            {
                writer.WriteLine("{0},{1},{2}", e.GetComponent<DotManage>().x, e.GetComponent<DotManage>().y, e.GetComponent<DotManage>().z);
                writer.Flush();
            }

            writer.Close();
        }
    }

    public void Delete()
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    dots[i, j, k].GetComponent<DotManage>().dotDestroy();
                    deads.Add(dots[i, j, k]); //List in

                }
            }
        }

        alives.Clear();
        deads.Clear();

    }

    public void RunStop()
    {
        isRun = !isRun;
        if (isRun) GameObject.Find("Run").GetComponentInChildren<Text>().text = "Stop";
        else GameObject.Find("Run").GetComponentInChildren<Text>().text = "Run";
    }

    public void periodicOn()
    {
        isOn = !isOn;
    }

    public void drum_change()
    {
        sequential = !sequential;
    }

    //IEnumerator Stop(float beat)
    //{
    //    yield return new WaitForSeconds(beat);
    //    yield break;
    //}

    public void Back() {
        SceneManager.LoadScene("Title");
    }

}


