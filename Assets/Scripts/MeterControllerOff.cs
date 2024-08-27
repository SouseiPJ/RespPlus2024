using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Average
using UnityEngine.UI;
using TMPro; // TextMeshPro

public class MeterControllerOff : MonoBehaviour
{
    // Variables
    // - Serialize
    [SerializeField] private string m_MicInDeviceName;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 1;
    [SerializeField] private GameObject m_meter;
    // [SerializeField] private GameObject Vol_object;
    [SerializeField] private GameObject propeller;
    [SerializeField] private GameObject RotationCount_object; // ��]���\���p�̃I�u�W�F�N�g��ǉ�
    [SerializeField] private GameObject brightnessObject; // ���邳��ύX����I�u�W�F�N�g��ǉ�
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

    // ��]�����J�E���g����ϐ�
    private float rotationCount = 0f;

    // ��]�������J����v���p�e�B��ǉ�
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

        // �ϓ�����ϐ��Ɋ�Â��ĉ�]���x�𒲐�
        targetSpeed = speed + Mathf.Pow(m_AmpGain * audioLevel, 2);

        // ���݂̑��x�����X�ɖڕW���x�ɋ߂Â���
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5);

        // �I�u�W�F�N�g����]������
        propeller.transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);

        // ��]�����J�E���g
        rotationCount += currentSpeed * Time.deltaTime / 360f;

        // Vol_object �� TextMeshProUGUI �R���|�[�l���g���X�V
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

        // RotationCount_object �� TextMeshProUGUI �R���|�[�l���g���X�V
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

        // ���邳��ύX����I�u�W�F�N�g�� Renderer �R���|�[�l���g���擾
        if (brightnessObject != null)
        {
            Renderer renderer = brightnessObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                // ��]���x�Ɋ�Â��Ė��邳�𒲐�
                float brightness = Mathf.Clamp01(currentSpeed / 100f); // 0����1�͈̔͂ɃN�����v
                Color color = renderer.material.color;
                color.a = brightness; // �A���t�@�l��ύX���Ė��邳�𒲐�
                renderer.material.color = color;
            }
            else
            {
                Debug.LogError("brightnessObject does not have a Renderer component.");
            }
        }
        else
        {
            Debug.LogError("brightnessObject is not assigned.");
        }
    }

    private void MicStart(string device)
    {
        if (device.Equals(""))
            device = Microphone.devices[0]; //�}�C�N���w�肳��Ă��Ȃ���΁A�V�X�e�������蓖�Ă�

        m_MicAudioSource.clip = Microphone.Start(device, true, 1, SAMPLE_RATE);

        //�}�C�N�f�o�C�X�̏������ł���܂ő҂�
        while (!(Microphone.GetPosition("") > 0)) { }

        m_MicAudioSource.Play();
    }
}
