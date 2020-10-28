using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Eigen_HRTF_plugin;
using System.Text;
using System.IO;

public class HRTFu : MonoBehaviour
{
    private float[] filter_l;
    private float[] filter_r;
    private float[] buffer_l;
    private float[] buffer_r;
    private float[] buffer_signal;
    private bool first=true;
    //private float[][] previous;
    private int[] delays;
    private int[] prev_delays;    
    private int[] idxs;    
    public float distance = 20;
    public float elevation = 0;
    public float azimuth = 0;
    public float scale = 0.01f;
    public enum Pinnas : int { Small = 0, Large = 1 };
    public Pinnas pinna;
    public GameObject listener = null;
    private AudioSource audio_source;
    private bool _isPlaying=false;
    //private List<List<float>> positions;
    List<float> samplesl;
    List<float> samplesr;
    //Listener object
    //public AudioMixerGroup MixerChannel=null;
    // Start is called before the first frame update


    void Awake()
    {
        filter_l = new float[1024];
        filter_r = new float[1024];
        buffer_l = new float[1024];
        buffer_r = new float[1024];
        buffer_signal = new float[256];
        samplesl = new List<float>();
        samplesr = new List<float>();
        delays = new int[2];
        prev_delays = new int[2];        
        idxs = new int[2];
        idxs[0] = -2;
        idxs[1] = -2;
        delays[0] = 0;
        delays[1] = 0;
        prev_delays[0] = 0;
        prev_delays[1] = 0;                
        AudioConfiguration config = AudioSettings.GetConfiguration();
        CheckAudioSource();
        //if (config.dspBufferSize != 256 || config.sampleRate != 48000 || config.speakerMode != AudioSpeakerMode.Stereo)
        //{
        //    List<bool> tmp_status = new List<bool>();
        //    List<int> tmp_samples = new List<int>();
        //    foreach (var i in FindObjectsOfType<AudioSource>())
        //    {
        //        tmp_status.Add(i.isPlaying);
        //        tmp_samples.Add(i.timeSamples);

        //    }
        //    config.dspBufferSize = 256;
        //    config.sampleRate = 48000;
        //    config.speakerMode = AudioSpeakerMode.Stereo;
        //    config.numRealVoices = 512;
        //    config.numVirtualVoices = 1024;
        //    AudioSettings.Reset(config);
        //    //if (tmp) this.GetComponent<AudioSource>().Play();
        //    int c = 0;
        //    foreach (var i in FindObjectsOfType<AudioSource>())
        //    {
        //        if (tmp_status[c]) { i.PlayDelayed(0.005f); i.timeSamples = tmp_samples[c]; }
        //        c++;
        //    }
        //}

        //AudioMixerGroup mixer = Instantiate(audio_source.outputAudioMixerGroup);
        //mixer.name = "HRTFu[" + this.GetInstanceID().ToString() + "]";
        //audio_source.outputAudioMixerGroup = mixer;
        if (listener == null)
        {
            //Seek audio listeners in scene
            AudioListener[] listeners = UnityEngine.Object.FindObjectsOfType<AudioListener>();
            if (listeners.Length == 0)
            {
                //The sound doesn't make sense without no one to hear it
                Debug.LogWarning("No Listner founds in this scene.");
            }
            else
            {
                //Set a listener
                listener = listeners[0].gameObject;
            }
        }
        Eigen_HRTF.eigen_init();
        _isPlaying = audio_source.isPlaying;
        //positions = new List<List<float>>();
    }

    // Update is called once per frame
    void Update()
    {        
        //AudioConfiguration config = AudioSettings.GetConfiguration();
        float tmp_d = distance;
        float tmp_e = elevation;
        float tmp_a = azimuth;
        //Calculate distance between listener and sound source	
        distance = Mathf.Abs(Vector3.Distance(transform.position, listener.transform.position)) / scale;
        //Calculate diretion vector between listener and sound source	
        Vector3 dir = (transform.position - listener.transform.position).normalized;
        //Debug.DrawRay(listener.transform.position,dir*distance,Color.blue,0.1f,false); 
        //Calculate angle of elevation between listener and sound source
        //Vector3 cross = Vector3.Cross(dir, listener.transform.right);
        if (Vector3.Cross(listener.transform.right, Vector3.ProjectOnPlane(dir, listener.transform.up)) == Vector3.zero)
        {
            Vector3 dirE = Vector3.ProjectOnPlane(dir, listener.transform.forward);
            elevation = Vector3.SignedAngle(listener.transform.right, dirE, listener.transform.forward);
        }
        //if (Vector3.Cross(-listener.transform.right, Vector3.ProjectOnPlane(dir, listener.transform.up)) == Vector3.zero)
        //{
        //    Vector3 dirE = Vector3.ProjectOnPlane(dir, -listener.transform.right);
        //    elevation = -Vector3.SignedAngle(-listener.transform.right, dirE, listener.transform.forward);
        //    Debug.LogWarning("2 " + dirE.ToString());
        //}
        else
        {
            Vector3 dirE = Vector3.ProjectOnPlane(dir, listener.transform.right);
            elevation = -Vector3.SignedAngle(listener.transform.forward, dirE, listener.transform.right);
        }
        //elevation = Mathf.Acos(dir.y) * Mathf.Rad2Deg + 90;       
        elevation = elevation % 180 == 0 ? 0 : elevation;
        //Debug.Log(-Vector3.SignedAngle(listener.transform.forward, dirE, listener.transform.right));
        if (elevation < -90f)
        {
            elevation = -90 - (elevation % 90);
        }
        if (elevation > 90f)
        {
            elevation = 90 - (elevation % 90);
        }
        //Calculate angle of azimuth between listener and sound source
        Vector3 dirA = Vector3.ProjectOnPlane(dir, listener.transform.up);
        azimuth = Vector3.SignedAngle(listener.transform.forward, dirA, listener.transform.up);
        if (azimuth < 0f)
        {
            azimuth = 360f + azimuth;
        }
        ////Debug.Log ("E: "+elevation.ToString()+"\tA: "+azimuth.ToString());
        //Vector3 dir = transform.position- listener.transform.position;
        //////////Debug.Log(dir.ToString()+"\t"+idxs[0].ToString() + " " + idxs[1].ToString() + "\tD: " + distance.ToString() + "\tE: " + elevation.ToString() + "\tA: " + azimuth.ToString());
        //azimuth = dir.z!=0?Mathf.Atan(dir.x / dir.z)*Mathf.Rad2Deg:270;
        //if (azimuth < 0) azimuth=360 + azimuth;        
        //elevation = Mathf.Acos(dir.y/distance) * Mathf.Rad2Deg-90;
        _isPlaying = audio_source.isPlaying;
        if (tmp_d == distance && tmp_e == elevation && tmp_a == azimuth) return;
        //previous_delays = delays;
        Eigen_HRTF.get_filters(distance, elevation, azimuth, (int)pinna, filter_l, filter_r, delays, idxs);        
        //Debug.Log((int)pinna);
        ///positions.Add(new List<float>() {distance,elevation,azimuth,Time.time});
        //Debug.Log(idxs[0].ToString()+" "+idxs[1].ToString()+ "\tE: " + elevation.ToString() + "\tA: " + azimuth.ToString());
    }

    private void CheckAudioSource()
    {
        if (this.GetComponent<AudioSource>() == null)
            gameObject.AddComponent<AudioSource>();
        audio_source = this.GetComponent<AudioSource>();
    }
    void OnAudioFilterRead(float[] data, int channels)
    {
        if(_isPlaying)
        Eigen_HRTF.DSP(data, data.Length, filter_l, filter_r, buffer_l, buffer_r, prev_delays, delays);
        //Eigen_HRTF.DSP_delay(data, data.Length, first, filter_l, filter_r,buffer_signal, buffer_l, buffer_r, delays);
        //if (first) first = false;
        //for (int i = 0; i < data.Length; i = i + 2)
        //{
        //    samplesl.Add(data[i]);
        //    samplesr.Add(data[i + 1]);
        //}
    }
    private void OnDestroy()
    {
        //AudioClip recordl = AudioClip.Create("output", samplesl.Count, 1, 48000, false);
        //AudioClip recordr = AudioClip.Create("output", samplesl.Count, 1, 48000, false);
        //recordl.SetData(samplesl.ToArray(), 0);
        //recordr.SetData(samplesr.ToArray(), 0);
        //SavWav.Save("outputl", recordl);
        //SavWav.Save("outputr", recordr);

    }

}