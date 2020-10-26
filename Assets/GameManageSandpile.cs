using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameManageSandpile : MonoBehaviour
{

    int n = 8; //a side
    int init = 1000; //grain size
    int init_x = 0, init_y = 0, init_z = 0;
    bool init_flag = false;

    public float dotInterval;
    public float bpm;
    float bar;
    float timeRecent = 0;

    public GameObject dotPref; //dot prefab
    public GameObject[,,] dots; //dots array
    int[,,] cp_dots; //copy dots array
    public GameObject head;

    public Slider bpm_slider;

    List<GameObject> alives = new List<GameObject>();
    List<GameObject> deads = new List<GameObject>();

    public InputField xInput, yInput, zInput, gInput;

    bool isRun = false, isDone = false;

    // Start is called before the first frame update
    void Start()
    {

        Data.Instance.referer = "SP";
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

    }

    public void change_bpm()
    {
        bpm = bpm_slider.value;
    }

    // Update is called once per frame
    void Update()
    {
        //rotate head :D
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            head.transform.Rotate(0, -0.5f, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            head.transform.Rotate(0, 0.5f, 0);
        }
        if (Input.GetKey(KeyCode.UpArrow) && (head.transform.eulerAngles.x > 270 || head.transform.eulerAngles.x < 10))
        {
            head.transform.Rotate(-0.5f, 0, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow) && (head.transform.eulerAngles.x >= 260 && head.transform.eulerAngles.x < 360))
        {
            head.transform.Rotate(0.5f, 0, 0);
        }
        if (isDone == true && init_flag == false)
        {
            //set init
            dots[init_x, init_y, init_z].GetComponent<DotManage>().dotGenerate();
            dots[init_x, init_y, init_z].GetComponent<DotManage>().state = init;
            alives.Add(dots[init_x, init_y, init_z]);

            switch (dots[init_x, init_y, init_z].GetComponent<DotManage>().state)
            {

                case 1:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case 2:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0.64f, 0f);
                    break;
                case 3:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case 4:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.green;
                    break;
                case 5:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                    break;
                case 6:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 0.5f);
                    break;
                default:
                    dots[init_x, init_y, init_z].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
                    break;

            }
            init_flag = true;
        }

        bar = 4f / (bpm / 60f);

        if (isRun)
        {

            timeRecent += Time.deltaTime; //add time every frame;

            if (timeRecent > bar)
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
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
                                        break;
                                    case 2:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0.64f, 0f);
                                        break;
                                    case 3:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                                        break;
                                    case 4:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.green;
                                        break;
                                    case 5:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                                        break;
                                    case 6:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 0.5f);
                                        break;
                                    default:
                                        dots[i, j, k].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
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

    public void Clear()
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

    public void setDone()
    {
        isDone = true;
    }

    public void setx()
    {
        init_x = int.Parse(xInput.text);
    }
    public void sety()
    {
        init_y = int.Parse(yInput.text);
    }

    public void setz()
    {
        init_z = int.Parse(zInput.text);
    }

    public void setg()
    {
        init = int.Parse(gInput.text);
    }

    public void rand()
    {
        int randx = Random.Range(0, n), randy = Random.Range(0, n), randz = Random.Range(0, n);
        dots[randx, randy, randz].GetComponent<DotManage>().dotGenerate();
        dots[randx, randy, randz].GetComponent<DotManage>().state = init;
        alives.Add(dots[randx, randy, randz]);

        switch (dots[randx, randy, randz].GetComponent<DotManage>().state)
        {

            case 1:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
                break;
            case 2:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0.64f, 0f);
                break;
            case 3:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case 4:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.green;
                break;
            case 5:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.cyan;
                break;
            case 6:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0.5f, 0f, 0.5f);
                break;
            default:
                dots[randx, randy, randz].transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
                break;

        }
    }

}
