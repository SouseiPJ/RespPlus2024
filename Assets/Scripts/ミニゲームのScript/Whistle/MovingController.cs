// -*- coding: utf-8 -*-

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingController : MonoBehaviour
{
    public float jumpForce = 5f;  // ジャンプ力
    public float moveForwardForce = 2f;  // 前進する力

    private Rigidbody2D rb;
    private bool isGrounded = true;  // ゲーム開始時に地面にいる場合

    [SerializeField] private string m_MicInDeviceName;
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 100;
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.05f;

//  LenFFT: Fs*LenFrameを2のべき乗に丸めた点数
    private int LenFFT = (int)Mathf.Pow(2.0f, Mathf.FloorToInt(Mathf.Log(SAMPLE_RATE * BUFFER_TIME) / Mathf.Log(2.0f)));

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

        // Spectrum devided to {Low,Mid,High}-range SPL
        waveSpectrum = new float[LenFFT];
        m_MicAudioSource.GetSpectrumData(waveSpectrum, 0, FFTWindow.Hamming);

        float lowFreqLevel = 0.0f;
        float highFreqLevel = 0.0f;
        for (var i = 0; i < LenFFT; i++)
        {
            var sumSpectra = Mathf.Abs(waveSpectrum[i]);
            if (i < LenFFT / 2)
                lowFreqLevel += sumSpectra;
            else
                highFreqLevel += sumSpectra;
        }

        // いつでもジャンプ前進してしまうので，音量のしきい値などをつけよう．

        moveForwardForce = m_AmpGain * lowFreqLevel;
        jumpForce = m_AmpGain * highFreqLevel;

        rb.velocity = new Vector2(moveForwardForce, jumpForce);

        // // スペースキーが押されたらジャンプ処理
        // if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.Log("ジャンプ！");
        //     rb.velocity = new Vector2(moveForwardForce, jumpForce);
        //     isGrounded = false;
        // }
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

}
