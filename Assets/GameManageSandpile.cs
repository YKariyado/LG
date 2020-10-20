using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GameManageSandpile : MonoBehaviour
{

    public int n; //a side
    public int init;

    public float dotInterval;
    public float bpm;
    float delay;
    float timeRecent = 0;

    public GameObject dotPref; //dot prefab
    public GameObject[,,] dots; //dots array
    int[,,] cp_dots; //copy dots array

    List<GameObject> alives = new List<GameObject>();
    List<GameObject> deads = new List<GameObject>();


    bool isRun = false;

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
                    GameObject obj = Instantiate(dotPref, new Vector3(dotInterval * (-n / 2.0f + i), dotInterval * (-n / 2.0f + j), dotInterval * (-n / 2.0f + k)), Quaternion.identity); // Generate dot prefabs from -n/2
                    obj.transform.parent = all.transform;
                    dots[i, j, k] = obj;
                    dots[i, j, k].GetComponent<DotManage>().x = i;
                    dots[i, j, k].GetComponent<DotManage>().y = j;
                    dots[i, j, k].GetComponent<DotManage>().z = k;
                }
            }
        }

        //set init
        dots[n / 2, n / 2, n / 2].GetComponent<DotManage>().dotGenerate();
        dots[n / 2, n / 2, n / 2].GetComponent<DotManage>().state = init;
        alives.Add(dots[n / 2, n / 2, n / 2]);

        switch (dots[n / 2, n / 2, n / 2].GetComponent<DotManage>().state)
        {

            case 1:
                dots[n / 2, n / 2, n / 2].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(128, 0, 128, 1);
                break;
            case 2:
                dots[n / 2, n / 2, n / 2].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                break;
            case 3:
                dots[n / 2, n / 2, n / 2].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.green;
                break;
            case 4:
                dots[n / 2, n / 2, n / 2].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case 5:
                dots[n / 2, n / 2, n / 2].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(255, 165, 0, 1);
                break;
            default:
                dots[n / 2, n / 2, n / 2].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
                break;

        }

    }

    // Update is called once per frame
    void Update()
    {

        delay = 4 / (bpm / 60);

        if (isRun)
        {

            timeRecent += Time.deltaTime; //add time every frame;

            if (timeRecent > delay)
            {

                cp_dots = new int[n, n, n];

                foreach (GameObject e in alives)
                {

                    DotManage temp = e.GetComponent<DotManage>();

                    if (temp.state < 6)
                        cp_dots[temp.x, temp.y, temp.z] += temp.state;

                    if (temp.state >= 6)
                    {
                        cp_dots[temp.x, temp.y, temp.z] += (temp.state - 6);
                        if (temp.x + 1 < n)
                            cp_dots[temp.x + 1, temp.y, temp.z]++;
                        if (temp.y + 1 < n)
                            cp_dots[temp.x, temp.y + 1, temp.z]++;
                        if (temp.z + 1 < n)
                            cp_dots[temp.x, temp.y, temp.z + 1]++;
                        if (temp.x - 1 >= 0)
                            cp_dots[temp.x - 1, temp.y, temp.z]++;
                        if (temp.y - 1 >= 0)
                            cp_dots[temp.x, temp.y - 1, temp.z]++;
                        if (temp.z - 1 >= 0)
                            cp_dots[temp.x, temp.y, temp.z - 1]++;
                    }
                }

                /*
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < n; k++)
                        {
                            DotManage temp = dots[i, j, k].GetComponent<DotManage>();


                            if (temp.state < 6)
                            {
                                cp_dots[i, j, k] += temp.state;
                            }

                            if (temp.state >= 6)
                            {
                                cp_dots[i, j, k] += (temp.state - 6);
                                if (i + 1 < n)
                                    cp_dots[i + 1, j, k]++;
                                if (j + 1 < n)
                                    cp_dots[i, j + 1, k]++;
                                if (k + 1 < n)
                                    cp_dots[i, j, k + 1]++;
                                if (i - 1 >= 0)
                                    cp_dots[i - 1, j, k]++;
                                if (j - 1 >= 0)
                                    cp_dots[i, j - 1, k]++;
                                if (k - 1 >= 0)
                                    cp_dots[i, j, k - 1]++;
                            }
                        }
                    }
                }
                */

                /* asymmetric
                if (temp.state >= 4)
                {
                    temp.state -= 4;
                    dots[i + 1, j].GetComponent<DotManage>().state++;
                    dots[i, j + 1].GetComponent<DotManage>().state++;
                    dots[i - 1, j].GetComponent<DotManage>().state++;
                    dots[i, j - 1].GetComponent<DotManage>().state++;
                }*/

                alives.Clear();

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        for (int k = 0; k < n; k++)
                        {

                            dots[i, j, k].GetComponent<DotManage>().state = cp_dots[i, j, k];

                            if (dots[i, j, k].GetComponent<DotManage>().state > 0)
                            {
                                dots[i, j, k].GetComponent<DotManage>().dotGenerate();
                                alives.Add(dots[i, j, k]);

                                switch (dots[i, j, k].GetComponent<DotManage>().state)
                                {

                                    case 1:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 0.5f);
                                        break;
                                    case 2:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                                        break;
                                    case 3:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.green;
                                        break;
                                    case 4:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                                        break;
                                    case 5:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0.64f, 0f);
                                        break;
                                    default:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
                                        break;

                                }
                            }
                        }
                    }
                }

                timeRecent = 0;
            }

        }

    }


    public void RunStop()
    {
        isRun = !isRun;
    }

}
