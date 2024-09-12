// -*- coding: utf-8 -*-

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MovingController : MonoBehaviour
{
    public float jumpForce = 5f;  // ジャンプ力
    public float moveForwardForce = 2f;  // 前進する力

    private Rigidbody2D rb;
    private bool isGrounded = true;  // ゲーム開始時に地面にいる場合

    [SerializeField] private string m_MicInDeviceName;
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    private float[] waveData;
    private float[] AMDF;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 100;
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.05f;

    //  LenFFT: Fs*LenFrameを2のべき乗に丸めた点数
    private int LenFFT = (int)Mathf.Pow(2.0f, Mathf.FloorToInt(Mathf.Log(SAMPLE_RATE * BUFFER_TIME) / Mathf.Log(2.0f)));

    // 背景雑音の音圧レベル（この音量以上をトリガーに）
    private float SilentLevel = (float)0.0;
    // 音量（前進力）
    // ピッチ（ジャンプ力）

    private void Awake()
    {
        // 音源ハンドラ
        m_MicAudioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

        AMDF = new float[LenFFT];
        float[] y = new float[2];
        float pitch;

        AMDF = getAMDF(waveData);
        y = YIN(AMDF,0.3);
        if(y[1]>0){
            pitch = y[0];
        }else{
            pitch = 0;
        }

        // // Spectrum devided to {Low,Mid,High}-range SPL
        // waveSpectrum = new float[LenFFT];
        // m_MicAudioSource.GetSpectrumData(waveSpectrum, 0, FFTWindow.Hamming);

        // float lowFreqLevel = 0.0f;
        // float highFreqLevel = 0.0f;
        // for (var i = 0; i < LenFFT; i++)
        // {
        //     var sumSpectra = Mathf.Abs(waveSpectrum[i]);
        //     if (i < LenFFT / 2)
        //         lowFreqLevel += sumSpectra;
        //     else
        //         highFreqLevel += sumSpectra;
        // }
        // // いつでもジャンプ前進してしまうので，音量のしきい値などをつけよう．

        moveForwardForce = m_AmpGain * audioLevel;
        jumpForce = pitch;


        // if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        if(isGrounded){
            Debug.Log("ジャンプ！");
            rb.velocity = new Vector2(moveForwardForce, jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("地面に着地！");
            isGrounded = true;
        }
    }

    private void MicStart(string device)
    {
        if (device.Equals(""))
            device = Microphone.devices[0]; //マイクが指定されていなければ、システムを割り当てる

        m_MicAudioSource.clip = Microphone.Start(device, true, 1, SAMPLE_RATE);

        //マイクデバイスの準備ができるまで待つ
        while (!(Microphone.GetPosition("") > 0)) { }

        m_MicAudioSource.Play();
    }

    private unsafe float[] YIN(float[] d, double Th)
    {
        int tau = 2;
        while(tau < LenFFT){
            if(d[tau] < Th){
                while(tau+1 < LenFFT && d[tau+1] < d[tau]) tau ++;
                break;
            }
            tau++;
        }
        float[] ret = new float[2];
        float pitch = 1/tau;
        float clarity = 1-Mathf.Log(1+d[tau])/Mathf.Log(2);
        ret[0] = pitch;
        ret[1] = clarity;
        return ret;
    }

    private unsafe float[] getAMDF(float[] x)
    {
        float[] mad = new float[LenFFT];
        int ni;
        for(int i=0;i<LenFFT;i++){
            mad[i] = 0;
            ni = 0;
            for(int j=0;j<LenFFT;j++){
                if(j-i>0){
                    mad[i] += Mathf.Abs(x[j-i] - x[j]);
                    ni ++;
                }
            }
            mad[i] /= ni;
        }
        return mad;
    }
}
