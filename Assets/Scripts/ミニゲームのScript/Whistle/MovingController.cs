using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingController : MonoBehaviour
{
    public float jumpForce = 5f;  // �W�����v��
    public float moveForwardForce = 2f;  // �O�i�����

    private Rigidbody2D rb;
    private bool isGrounded = true;  // �Q�[���J�n���ɒn�ʂɂ���ꍇ

    [SerializeField] private string m_MicInDeviceName;
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 100;
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.05f;
    private int BUFFER_SIZE = Mathf.CeilToInt(SAMPLE_RATE * BUFFER_TIME);

    private int Q = (int)Mathf.Pow(2.0f,Mathf.FloorToInt(Mathf.Log(SAMPLE_RATE * BUFFER_TIME)/Mathf.Log(2.0f)));

    private void Awake()
    {
        // �����n���h��
        m_MicAudioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

    void Update()
    {
        if (!m_MicAudioSource.isPlaying)
            return;

        // Spectrum devided to {Low,Mid,High}-range SPL
        waveSpectrum = new float[Q];
        m_MicAudioSource.GetSpectrumData(waveSpectrum, 0, FFTWindow.Hamming);

        float lowFreqLevel = 0.0f;
        float highFreqLevel = 0.0f;
        for(var i=0;i<Q;i++){
            var sumSpectra = Mathf.Abs(waveSpectrum[i]);
            if(i < Q/2)
                lowFreqLevel += sumSpectra;
            else
                highFreqLevel += sumSpectra;
        }

        // ���ł��W�����v�O�i���Ă��܂��̂ŁC���ʂ̂������l�Ȃǂ����悤�D
        
        moveForwardForce = m_AmpGain * lowFreqLevel;
        jumpForce = m_AmpGain * highFreqLevel;

        rb.velocity = new Vector2(moveForwardForce, jumpForce);

        // // �X�y�[�X�L�[�������ꂽ��W�����v����
        // if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        // {
        //     Debug.Log("�W�����v�I");
        //     rb.velocity = new Vector2(moveForwardForce, jumpForce);
        //     isGrounded = false;
        // }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("�n�ʂɒ��n�I");
            isGrounded = true;
        }
    }

    private void MicStart(string device) {
        if (device.Equals(""))
            device = Microphone.devices[0]; //�}�C�N���w�肳��Ă��Ȃ���΁A�V�X�e�������蓖�Ă�

        m_MicAudioSource.clip = Microphone.Start(device, true, 1, SAMPLE_RATE);

        //�}�C�N�f�o�C�X�̏������ł���܂ő҂�
        while (!(Microphone.GetPosition("") > 0)) { }

        m_MicAudioSource.Play();
    }

}
