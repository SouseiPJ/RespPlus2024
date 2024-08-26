using System.Collections;
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
    [SerializeField] private GameObject Vol_object;
    [SerializeField] private GameObject propeller;
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
        targetSpeed = speed + Mathf.Pow(m_AmpGain * audioLevel, 5);

        // 現在の速度を徐々に目標速度に近づける
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5); // 2は調整可能なスムーズさの係数

        // オブジェクトを回転させる
        propeller.transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);

        // Vol_object の TextMeshProUGUI コンポーネントを更新
        if (Vol_object != null)
        {
            TextMeshProUGUI Vol_text = Vol_object.GetComponent<TextMeshProUGUI>();
            if (Vol_text != null)
            {
                Vol_text.text = Mathf.RoundToInt(currentSpeed).ToString();
            }
            else
            {
                Debug.LogError("Vol_object does not have a TextMeshProUGUI component.");
            }
        }
        else
        {
            Debug.LogError("Vol_object is not assigned.");
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
