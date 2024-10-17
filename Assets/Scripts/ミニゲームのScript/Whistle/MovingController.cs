// -*- coding: utf-8 -*-

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SoundAnalysis;
using System.IO;

public class MovingController : MonoBehaviour
{
    [SerializeField] private string m_MicInDeviceName;
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    private float[] waveData;
    private float[] AMDF;
    private float[] CMND;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 100;
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.05f;

    //  LenFFT: Fs*LenFrameを2のべき乗に丸めた点数
    private int LenFFT = (int)Mathf.Pow(2.0f, Mathf.FloorToInt(Mathf.Log(SAMPLE_RATE * BUFFER_TIME) / Mathf.Log(2.0f)));

    private int count;
    private float scale;
    private float chroma;
    private float pitch;

    private Transform tr;
    private void Awake()
    {
        // 音源ハンドラ
        m_MicAudioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        count = 0;
        tr = gameObject.GetComponent<Transform>();
        string targetDevice = "";
        foreach (var device in Microphone.devices)
        {
            Debug.Log($"Device Name: {device}");
            if (device.Equals(m_MicInDeviceName))
            {
                targetDevice = device;
            }
        }

        Debug.Log($"=== Device Set: {targetDevice} ===");
        MicStart(targetDevice);

    }

    void Update()
    {
        if (!m_MicAudioSource.isPlaying)
            return;

        waveData = new float[LenFFT];
        m_MicAudioSource.GetOutputData(waveData, 0);

        float audioLevel = waveData.Average(Mathf.Abs);

        float Level = m_AmpGain * audioLevel;
        Vector3 pos = tr.position;

        if (Level > 0.7f) // 音量が0.7より大きければscale計算
        {
            Debug.Log(LenFFT);
            AMDF = new float[LenFFT];
            CMND = new float[LenFFT];
            double[] y = new double[2];

            ToneHeights toneHeights = new ToneHeights();

            AMDF = toneHeights.getAMDF(waveData, LenFFT);
            CMND = toneHeights.cmnd(AMDF,LenFFT);
            File.WriteAllText(@"amdf.txt","");
            for (int i = 0; i < LenFFT; i++)
                File.AppendAllText(@"amdf.txt",$"{CMND[i]}\n");
            //Debug.Log($"AMDF[{i}]:{AMDF[i]}");
            y = toneHeights.YIN(CMND, LenFFT, 0.3);
            double clarity = y[1];
            if (clarity > 0.0f && y[0] > 0.0f)
            {
                pitch = SAMPLE_RATE * (float)y[0];
            }
            if (pitch > 0.0f){
                scale = toneHeights.Hz2Scale((float)pitch);
                chroma = toneHeights.Scale2Chroma(scale)[0];
                Debug.Log($"Pitch: {pitch}, Scale: {scale}, Chroma: {chroma}");
                Debug.Log($"Clarity: {clarity}");
                pos.y = (float)chroma; //scaleは音高(A1=1,A#1=2,B1=3,...,B#1=12)
                pos.x += (float)0.1; //前に進む
                tr.position = pos; // (x,y) = (scale,count)にキャラ移動
            }
        }
    }

    private void MicStart(string device)
    {
        if (device.Equals(""))
            device = Microphone.devices[0]; //マイクが指定されていなければ、システムを割り当てる

        m_MicAudioSource.clip = Microphone.Start(device, true, 1, SAMPLE_RATE);

        m_MicAudioSource.loop = true;
        //マイクデバイスの準備ができるまで待つ
        while (!(Microphone.GetPosition("") > 0)) { }

        m_MicAudioSource.Play();
    }

}
