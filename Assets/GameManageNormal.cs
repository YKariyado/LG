using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;

using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SimpleFileBrowser;
using System;

public class GameManageNormal : MonoBehaviour
{

    int n = 8, time = 0, num = 0; //a side
    public int r1, r2, r3, r4;

    public float dotInterval;
    public float bpm;
    float delay, beat;
    float timeRecent = 0, timeRecent2 = 0;    

    public GameObject dotPref; //dot prefab
    public GameObject[,,] dots; //dots array
    public Slider bpm_slider;

    List<GameObject> alives = new List<GameObject>();
    List<GameObject> deads = new List<GameObject>();

    List<GameObject> alives_cp;

    StreamWriter writer = null;

    //public AudioClip kick, snare, clap, tom, chats, ohats, crash, bass;    
    public AudioClip[] drum_machine;
    private  AudioClip[,,] sounds_matlab;
    //AudioClip tone;

    bool isRun;
    bool isOn;
    public bool with_drum=false;    

    private void Awake()
    {
        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Preset data (CSV)", ".csv"));
        FileBrowser.SetDefaultFilter(".csv");
        drum_machine = new AudioClip[n];
        int c = 0;
        foreach (var i in new string[] { "kick", "snare", "clap", "tom", "chats", "ohats", "crash", "bass" }) {            
            drum_machine[c] = Resources.Load<AudioClip>(Path.Combine("Sounds",Path.Combine("drum_machine",i)));
            c++; //;)
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
        Debug.Log(Application.dataPath+"/Save/");

        GameObject all = GameObject.Find("AllDots");
        dots = new GameObject[n, n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    GameObject obj = Instantiate(dotPref, new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k)), Quaternion.identity); // Generate dot prefabs from -n/2
                    obj.transform.parent = all.transform;
                    obj.GetComponent<AudioSource>().volume = 1f / n;
                    //obj.GetComponent<AudioSource>().clip= Resources.Load<AudioClip>("sounds_matlab/" + (k+1).ToString() + "_" + (j+1).ToString() + "_" + (k+1).ToString());
                    dots[i, j, k] = obj;
                    dots[i, j, k].GetComponent<DotManage>().x = i;
                    dots[i, j, k].GetComponent<DotManage>().y = j;
                    dots[i, j, k].GetComponent<DotManage>().z = k;
                    float floati = i+1, floatj = j+1, floatk = k+1;
                    dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(floati/6f, floatj/6f, floatk/6f);
                }
            }
        }

    }
    public void change_bpm()
    {
        bpm = bpm_slider.value;
    }

    // Update is called once per frame
    void Update()
    {

        delay = 4 / (bpm / 60);
        beat = 1 / ((bpm / 60) * 2);

        if (isRun)
        {
            timeRecent += Time.deltaTime;
            timeRecent2 += Time.deltaTime;

            if (timeRecent > delay)
            {

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


                    } else {

                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                for (int k = -1; k < 2; k++)
                                {
                                    if ((temp.x + i < 0 || temp.x + i >= n || temp.y + j < 0 || temp.y + j >= n || temp.z + k < 0 || temp.z + k >= n)
                                        || (temp.x == 0 && temp.y == 0 && temp.z == 0))
                                    {
                                    }
                                    else
                                    {
                                        dots[temp.x + i, temp.y + j, temp.z + k].GetComponent<DotManage>().neighbor++;
                                    }
                                }
                            }
                        }

                    }

                    if (temp.neighbor > r4 || temp.neighbor < r3)
                    {
                        temp.dotDestroy();
                    }

                }

                foreach (GameObject e in alives)
                {
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

                timeRecent = 0;
                //Debug.Log(currentTime);

            }


            //matlab_sound
            if (timeRecent2 > beat && !with_drum)
            {

                time = time % n;

                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        if (dots[j, k, time].GetComponent<DotManage>().isAlive)
                        {
                            dots[j, k, time].GetComponent<AudioSource>().clip = sounds_matlab[j, k, time];
                            dots[j, k, time].GetComponent<AudioSource>().Play();
                        }
                    }

                }


                time++;

                timeRecent2 = 0;

            }

            //DRUMMMMMMSSS
            if (timeRecent2 > beat && with_drum)
            {

                time = time % n;

                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (dots[j, k, time].GetComponent<DotManage>().isAlive)
                        {
                            dots[j, k, time].GetComponent<AudioSource>().clip = drum_machine[j];
                            dots[j, k, time].GetComponent<AudioSource>().Play();                            
                        }
                    }

                }          

                time++;

                timeRecent2 = 0;

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

    }


    public void RandomGenerate()
    {
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

        alives_cp = new List<GameObject>(alives);

    }

    public void PresetOneGenerate()
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

        FileBrowser.RequestPermission();
        StartCoroutine(ShowLoadDialogCoroutine());

    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, false, null, "Load File", "Load");

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
        try
        {
            var di = new DirectoryInfo(Environment.CurrentDirectory);
            var tagName = "patterns";
            var max = di.GetFiles(tagName + "_???.csv") // パターンに一致するファイルを取得する
                .Select(fi => Regex.Match(fi.Name, @"(?i)_(\d{3})\.csv$")) // ファイルの中で数値のものを探す
                .Where(m => m.Success) // 該当するファイルだけに絞り込む
                .Select(m => Int32.Parse(m.Groups[1].Value)) // 数値を取得する
                .DefaultIfEmpty(0) // １つも該当しなかった場合は 0 とする
                .Max(); // 最大値を取得する
            var fileName = String.Format("{0}_{1:d3}.csv", tagName, max + 1);

            Encoding enc = Encoding.GetEncoding("utf-8");
            writer = new StreamWriter(fileName, true, enc);
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }

        foreach (GameObject e in alives_cp)
        {
            writer.WriteLine("{0},{1},{2}", e.GetComponent<DotManage>().x, e.GetComponent<DotManage>().y, e.GetComponent<DotManage>().z);
            writer.Flush();
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
    }

    public void periodicOn()
    {
        isOn = !isOn;
    }

    public void drum_change()
    {
        with_drum = !with_drum;
    }

    IEnumerator Stop(float beat)
    {
        yield return new WaitForSeconds(beat);
        yield break;
    }

}


