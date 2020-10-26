using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEditor;

using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SimpleFileBrowser;
using System;

public class LoadPreset : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void one()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/Save/" + "patterns_1.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void two()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_2.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void three()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_3.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void four()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_4.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void five()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_5.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void six()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_6.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void seven()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_7.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void eight()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_8.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void nine()
    {
        StreamReader sr = new StreamReader(Application.streamingAssetsPath+"/Save/" + "patterns_9.csv");

        List<string> lists = new List<string>();

        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            string[] values = line.Split(',');

            // array to list
            lists.AddRange(values);

            Data.Instance.alives_cp = new List<int>(lists.ConvertAll(int.Parse));
        }

        //for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        //{
        //    GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]].GetComponent<DotManage>().dotGenerate();
        //    GameManageNormal.alives.Add(GameManageNormal.dots[Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]]);
        //}

        if (Data.Instance.referer == "GoL")
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Normal");
        }
        else
        {
            Data.Instance.referer = "Load";
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void Return()
    {
        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

}
