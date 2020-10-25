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

public class SavePreset : MonoBehaviour
{

    StreamWriter writer = null;

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
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_1.csv", false, enc);

        for (int i=0; i<Data.Instance.alives_cp.Count - 2; i+=3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i+1], Data.Instance.alives_cp[i+2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void two()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_2.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void three()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_3.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void four()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_4.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void five()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_5.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void six()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_6.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void seven()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_7.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void eight()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_8.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
            SceneManager.LoadScene("Sandpile");
        }
    }

    public void nine()
    {
        Encoding enc = Encoding.GetEncoding("utf-8");
        writer = new StreamWriter(Application.dataPath + "/Resources/Data/StreamingAssets/Save/" + "patterns_9.csv", false, enc);

        for (int i = 0; i < Data.Instance.alives_cp.Count - 2; i += 3)
        {
            writer.WriteLine("{0},{1},{2}", Data.Instance.alives_cp[i], Data.Instance.alives_cp[i + 1], Data.Instance.alives_cp[i + 2]);
            writer.Flush();
        }

        writer.Close();

        if (Data.Instance.referer == "GoL")
        {
            SceneManager.LoadScene("Normal");
        }
        else
        {
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
