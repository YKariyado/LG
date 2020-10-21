using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class GameManageNormal : MonoBehaviour
{

    int n = 8, time = 0; //a side
    public int r1, r2, r3, r4;

    public float dotInterval;
    public float bpm;
    float delay, beat;
    float timeRecent = 0, timeRecent2 = 0;

    public GameObject dotPref; //dot prefab
    public GameObject[,,] dots; //dots array

    List<GameObject> alives = new List<GameObject>();
    List<GameObject> deads = new List<GameObject>();

    public AudioClip kick, snare, clap, tom, chats, ohats, crash, bass;
    private AudioClip[] audio_list;
    int[] counter;
    AudioSource audioSource;

    bool isRun;
    bool isOn;

    private void Awake()
    {
        audio_list = new AudioClip[8] { kick, snare, clap, tom, chats, ohats, crash, bass};        
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject all = GameObject.Find("AllDots");
        dots = new GameObject[n, n, n];

        audioSource = GetComponent<AudioSource>();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    GameObject obj = Instantiate(dotPref, new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k)), Quaternion.identity); // Generate dot prefabs from -n/2
                    obj.transform.parent = all.transform;
                    obj.GetComponent<AudioSource>().volume = 1f / 8;
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


            //sort
            if (timeRecent2 > beat)
            {

                if (time == n)
                {
                    time = 0;

                }

                counter = new int[8];

                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (dots[k, j, time].GetComponent<DotManage>().isAlive)
                        {
                            dots[k, j, time].GetComponent<AudioSource>().clip = audio_list[k];
                            dots[k, j, time].GetComponent<AudioSource>().Play();
                            counter[k]++;
                        }
                    }

                }

                //sounds
                //if (counter[0] > 0)
                //    audioSource.PlayOneShot(kick);
                //if (counter[1] > 0)
                //    audioSource.PlayOneShot(snare);
                //if (counter[2] > 0)
                //    audioSource.PlayOneShot(clap);
                //if (counter[3] > 0)
                //    audioSource.PlayOneShot(tom);
                //if (counter[4] > 0)
                //    audioSource.PlayOneShot(chats);
                //if (counter[5] > 0)
                //    audioSource.PlayOneShot(ohats);
                //if (counter[6] > 0)
                //    audioSource.PlayOneShot(crash);
                //if (counter[7] > 0)
                //    audioSource.PlayOneShot(bass);


                if (time < n)
                {
                    time++;

                }

                timeRecent2 = 0;

            }


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
                    if (Random.Range(0, 5) == 0)
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

        string path = EditorUtility.OpenFilePanel("Open pattern file", "", "csv");
        StreamReader sr = new StreamReader(path);

        List<string> lists = new List<string>();
        List<int> nums = new List<int>();

        while (!sr.EndOfStream)
        {
            // CSVファイルの一行を読み込む
            string line = sr.ReadLine();
            // 読み込んだ一行をカンマ毎に分けて配列に格納する
            string[] values = line.Split(',');

            // 配列からリストに格納する
            lists.AddRange(values);
            nums = lists.ConvertAll(int.Parse);
        }

        for (int i = 0; i < nums.Count - 2; i += 3)
        {
            dots[nums[i], nums[i + 1], nums[i + 2]].GetComponent<DotManage>().dotGenerate();
            alives.Add(dots[nums[i], nums[i + 1], nums[i + 2]]);
            //Debug.Log(nums[i] + ", " + nums[i + 1] + ", " + nums[i + 2]);
        }

        //lives[0, 0, 1].GetComponent<DotManage>().LifeGenerate();
        //aliveLife.Add(lives[0, 0, 1]);
        //lives[0, 1, 0].GetComponent<DotManage>().LifeGenerate();
        //aliveLife.Add(lives[0, 1, 0]);
        //lives[1, 0, 0].GetComponent<DotManage>().LifeGenerate();
        //aliveLife.Add(lives[1, 0, 0]);

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

    IEnumerator Stop(float beat)
    {
        yield return new WaitForSeconds(beat);
        yield break;
    }

}
