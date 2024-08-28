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
    [SerializeField, Range(1, 10)] private int monstersToRemove = 1; // ��x�ɏ����郂���X�^�[�̐���ݒ�

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

    // �������ʗp�̕ϐ�
    private int samplingRange = 512;
    private int thousand_index = 24;
    private float[] hairetu;
    private double[] Fu_array = { 0.04663889, 0.08273548, 0.1707033, 0.1037671, 0.05075672, 0.02912107, 0.07029577, 0.02660729, 0.07701397, 0.07358543, 0.03582431, 0.034128, 0.06408661, 0.0669658, 0.04760632, 0.03390676, 0.04892594, 0.04976312, 0.0779156, 0.08174721, 0.05171812, 0.03128907, 0.03644949, 0.0191151 };
    private double[] Ha_array = { 0.09666242, 0.1010209, 0.04417039, 0.005689631, 0.005637815, 0.01925762, 0.03026361, 0.0365145, 0.0382, 0.01092663, 0.00179, 0.00457, 0.01878265, 0.055, 0.06935102, 0.0409, 0.0239, 0.0127, 0.027, 0.0401, 0.0292, 0.0226, 0.00922, 0.0389 };
    private double[] Pa_array = { 0.03245861, 0.04191279, 0.01490102, 0.02225155, 0.009038828, 0.01731531, 0.005591091, 0.02210945, 0.0117, 0.0108, 0.0292, 0.0925, 0.135, 0.139, 0.0511, 0.0134, 0.00949, 0.0122, 0.014, 0.0142, 0.0084, 0.00948, 0.0178, 0.00705 };

    // �������ʌ��ʂ�ێ�����v���p�e�B
    public string RecognizedSound { get; private set; }

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

        // �������ʏ���
        var spectrum = m_MicAudioSource.GetSpectrumData(samplingRange, 0, FFTWindow.Hamming);
        double Fu_euclid = 0;
        double Ha_euclid = 0;
        double Pa_euclid = 0;
        double max_amplitude = 0;
        int max_index = 0;

        for (int i = 0; i < thousand_index; i++)
        {
            Fu_euclid += (Fu_array[i] - spectrum[i]) * (Fu_array[i] - spectrum[i]);
            Ha_euclid += (Ha_array[i] - spectrum[i]) * (Ha_array[i] - spectrum[i]);
            Pa_euclid += (Pa_array[i] - spectrum[i]) * (Pa_array[i] - spectrum[i]);
            if (max_amplitude < spectrum[i])
            {
                max_amplitude = spectrum[i];
                max_index = i;
            }
        }

        double Fu_odds = 1 / (1 + Mathf.Sqrt((float)Fu_euclid));
        double Ha_odds = 1 / (1 + Mathf.Sqrt((float)Ha_euclid));
        double Pa_odds = 1 / (1 + Mathf.Sqrt((float)Pa_euclid));

        if ((Ha_odds >= 0.83) && (max_amplitude >= 0.05))
        {
            RecognizedSound = "�͂�";
            Debug.Log("�͂�  Fu=" + Fu_odds.ToString() + " Ha=" + Ha_odds.ToString());
        }
        else if ((Fu_odds >= 0.70) && (max_amplitude >= 0.05))
        {
            RecognizedSound = "�Ӂ[";
            Debug.Log("�Ӂ[  Fu=" + Fu_odds.ToString() + " Ha=" + Ha_odds.ToString());
        }
        else if ((Pa_odds >= 0.80) && (max_amplitude >= 0.05))
        {
            RecognizedSound = "�ς�";
            Debug.Log("�ς�  Fu=" + Fu_odds.ToString() + " Ha=" + Ha_odds.ToString() + " Pa=" + Pa_odds.ToString());
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

    public void ResetMic()
    {
        if (m_MicAudioSource.isPlaying)
        {
            m_MicAudioSource.Stop();
            Microphone.End(m_MicInDeviceName);
        }
        MicStart(m_MicInDeviceName);
    }

    public void ClearRecognizedSound()
    {
        RecognizedSound = null;
    }

    public int GetMonstersToRemove()
    {
        return monstersToRemove;
    }
}
