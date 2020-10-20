using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSand : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("Sandpile");
    }
}
