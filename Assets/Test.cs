using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AudioClip kick, snare;
    AudioSource audioSource;

    int j=0;
    int[,] counter = { {1, 0, 1, 0, 1, 0, 1, 0}, { 0, 1, 0, 1, 0, 1, 0, 1 } };
    public float bpm;

    float delay, beat;
    float timeRecent = 0, timeRecent2 = 0;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        delay = 4 / (bpm / 60);
        beat = 1 / ((bpm / 60)*2);

        timeRecent += Time.deltaTime;


            if (timeRecent > beat)
            {
                if (counter[0, j] > 0)
                {
                    audioSource.PlayOneShot(kick);
                }
                if (counter[1, j] > 0)
                {
                    audioSource.PlayOneShot(snare);
                }
                j++;
                timeRecent = 0;
            }

    }

}
