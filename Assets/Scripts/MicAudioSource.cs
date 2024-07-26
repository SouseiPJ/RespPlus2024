using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Average
public class MicAudioSource : MonoBehaviour
{
    // Variables
    // - Serialize
    [SerializeField] private string m_MicInDeviceName;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 100;
    [SerializeField] private GameObject m_Cube;
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.05f;
    private int BUFFER_SIZE = Mathf.CeilToInt(SAMPLE_RATE * BUFFER_TIME);

    private int Q = (int)Mathf.Pow(2.0f,Mathf.FloorToInt(Mathf.Log(SAMPLE_RATE * BUFFER_TIME)/Mathf.Log(2.0f)));
    // - AudioSource
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    private float[] waveData;

    // Awake
    private void Awake()
    {
        m_MicAudioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        string targetDevice = "";
        foreach (var device in Microphone.devices) {
            Debug.Log($"Device Name: {device}");
            if (device.Equals(m_MicInDeviceName)) {
                targetDevice = device;
            }
        }

        Debug.Log($"=== Device Set: {targetDevice} ===");
        MicStart(targetDevice);
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_MicAudioSource.isPlaying)
            return;

        waveData = new float[BUFFER_SIZE];
        m_MicAudioSource.GetOutputData(waveData, 0);

        float audioLevel = waveData.Average(Mathf.Abs);
        //m_Cube.transform.localScale = new Vector3(1, 1 + m_AmpGain * audioLevel, 1);

        // Spectrum devided to {Low,Mid,High}-range SPL
        waveSpectrum = new float[Q];
        m_MicAudioSource.GetSpectrumData(waveSpectrum, 0, FFTWindow.Hamming);

        float lowFreqLevel = 0.0f;
        float midFreqLevel = 0.0f;
        float highFreqLevel = 0.0f;
        for(var i=0;i<Q;i++){
            var sumSpectra = Mathf.Abs(waveSpectrum[i]); 
            if(i < 1 * Q/3)
                lowFreqLevel += sumSpectra;
            else if(i < 2 * Q/3)
                midFreqLevel += sumSpectra;
            else if(i < 3 * Q/3)
                highFreqLevel += sumSpectra;
        }
        //lowFreqLevel = 10 * Mathf.Log(lowFreqLevel);
        //midFreqLevel = 10 * Mathf.Log(midFreqLevel);
        //highFreqLevel = 10 * Mathf.Log(highFreqLevel);
        m_Cube.transform.localScale = new Vector3(
            1+m_AmpGain*lowFreqLevel,
            1+m_AmpGain*midFreqLevel,
            1+m_AmpGain*highFreqLevel);
    }

    private void MicStart(string device) {
        if (device.Equals("")) 
            device = Microphone.devices[0]; //マイクが指定されていなければ、システムを割り当てる

        m_MicAudioSource.clip = Microphone.Start(device, true, 1, SAMPLE_RATE);

        //マイクデバイスの準備ができるまで待つ
        while (!(Microphone.GetPosition("") > 0)) { }

        m_MicAudioSource.Play();
    }
}
