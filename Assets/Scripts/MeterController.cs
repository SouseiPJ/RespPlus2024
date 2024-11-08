﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Average
using UnityEngine.UI;
using TMPro; // TextMeshPro

public class MeterController : MonoBehaviour
{
    // Variables
    // - Serialize
    [SerializeField] private string m_MicInDeviceName;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 1;
    [SerializeField] private GameObject m_meter;
    // [SerializeField] private GameObject Vol_object;
    [SerializeField] private GameObject propeller;
    [SerializeField] private GameObject RotationCount_object; // 回転数表示用のオブジェクトを追加
    [SerializeField] private GameObject parentObject; // 親オブジェクトを追加
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.05f;
    private int BUFFER_SIZE = Mathf.CeilToInt(SAMPLE_RATE * BUFFER_TIME);

    private int Q = (int)Mathf.Pow(2.0f, Mathf.FloorToInt(Mathf.Log(SAMPLE_RATE * BUFFER_TIME) / Mathf.Log(2.0f)));
    // - AudioSource
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    private float[] waveData;

    // Propeller
    private float speed = 50f;
    private float currentSpeed = 30f;
    private float targetSpeed = 0f;

    // 回転数をカウントする変数
    private float rotationCount = 0f;

    // 回転数を公開するプロパティを追加
    public float RotationCount
    {
        get { return rotationCount; }
    }

    // Awake
    private void Awake()
    {
        m_MicAudioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        if (!m_MicAudioSource.isPlaying)
            return;

        waveData = new float[BUFFER_SIZE];
        m_MicAudioSource.GetOutputData(waveData, 0);

        float audioLevel = waveData.Average(Mathf.Abs);
        m_meter.GetComponent<Image>().fillAmount = m_AmpGain * audioLevel;

        // 変動する変数に基づいて回転速度を調整
        targetSpeed = speed + Mathf.Pow(m_AmpGain * audioLevel, 2);

        // 現在の速度を徐々に目標速度に近づける
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5);

        // オブジェクトを回転させる
        propeller.transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);

        // 回転数をカウント
        rotationCount += currentSpeed * Time.deltaTime / 360f;

        // Vol_object の TextMeshProUGUI コンポーネントを更新
        //if (Vol_object != null)
        //{
        //TextMeshProUGUI Vol_text = Vol_object.GetComponent<TextMeshProUGUI>();
        //    if (Vol_text != null)
        //    {
        //        Vol_text.text = Mathf.RoundToInt(currentSpeed).ToString();
        //    }
        //    else
        //    {
        //        Debug.LogError("Vol_object does not have a TextMeshProUGUI component.");
        //    }
        //}
        //else
        //{
        //    Debug.LogError("Vol_object is not assigned.");
        //}

        // RotationCount_object の TextMeshProUGUI コンポーネントを更新
        if (RotationCount_object != null)
        {
            TextMeshProUGUI RotationCount_text = RotationCount_object.GetComponent<TextMeshProUGUI>();
            if (RotationCount_text != null)
            {
                RotationCount_text.text = Mathf.FloorToInt(rotationCount).ToString();
            }
            else
            {
                Debug.LogError("RotationCount_object does not have a TextMeshProUGUI component.");
            }
        }
        else
        {
            Debug.LogError("RotationCount_object is not assigned.");
        }

        if (parentObject != null)
        {
            // 親オブジェクトの全ての子オブジェクトを取得
            foreach (Transform child in parentObject.transform)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // 回転速度に基づいて明るさを調整
                    float brightness = Mathf.Clamp01(currentSpeed / 100f); // 0から1の範囲にクランプ
                    Color color = renderer.material.color;
                    color.a = brightness; // アルファ値を変更して明るさを調整
                    renderer.material.color = color;
                }
                else
                {
                    Debug.LogError($"{child.name} does not have a Renderer component.");
                }
            }
        }
        else
        {
            Debug.LogError("parentObject is not assigned.");
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

    public void ResetMic()
    {
        if (m_MicAudioSource.isPlaying)
        {
            m_MicAudioSource.Stop();
            Microphone.End(m_MicInDeviceName);
        }
        MicStart(m_MicInDeviceName);
    }
}
