using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using SoundAnalysis;
using System.IO;

public class AstronautMovement : MonoBehaviour
{
    public int totalStars = 14;  // 星の合計数
    private int collectedStars = 0;  // 獲得した星の数

    public Text scoreText;  // UIのスコア表示用
    

                            //public float moveSpeed = 2f; // 右への移動速度
                            // public float floatAmplitude = 0.5f; // 上下の振幅
                            //public float floatFrequency = 1.0f; // 上下の周波数

    // private Vector3 startPosition;



    [SerializeField] private string m_MicInDeviceName;
    private AudioSource m_MicAudioSource;
    private float[] waveSpectrum;
    private float[] waveData;
    private float[] AMDF;
    private float[] CMND;
    [SerializeField, Range(10, 300)] private float m_AmpGain = 100;
    // - Const
    private const int SAMPLE_RATE = 44100;
    private const float BUFFER_TIME = 0.01f;

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





    // Start is called before the first frame update
    void Start()
    {
        // スコアの表示を更新
        scoreText.text = "Stars: " + collectedStars + "/" + totalStars;

        //startPosition = transform.position;



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

    

    // Update is called once per frame
    void Update()
    {
        // 宇宙飛行士を右に移動させる
       //transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        // 無重力感を出すために上下に揺れる
       //float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
      // transform.position = new Vector3(transform.position.x, newY, transform.position.z);




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
            CMND = toneHeights.cmnd(AMDF, LenFFT);
            File.WriteAllText(@"amdf.txt", "");
            for (int i = 0; i < LenFFT; i++)
                File.AppendAllText(@"amdf.txt", $"{CMND[i]}\n");
            //Debug.Log($"AMDF[{i}]:{AMDF[i]}");
            y = toneHeights.YIN(CMND, LenFFT, 0.3);
            double clarity = y[1];
            if (clarity > 0.0f && y[0] > 0.0f)
            {
                pitch = SAMPLE_RATE * (float)y[0];
            }
            if (pitch > 0.0f)
            {
                scale = toneHeights.Hz2Scale((float)pitch);
                chroma = toneHeights.Scale2Chroma(scale)[0];
                Debug.Log($"Pitch: {pitch}, Scale: {scale}, Chroma: {chroma}");
                Debug.Log($"Clarity: {clarity}");
                pos.y = (float)chroma; //scaleは音高(A1=1,A#1=2,B1=3,...,B#1=12)
                pos.x += (float)1.0; //前に進む
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Star")) // 星のタグがついたオブジェクトに衝突したかを確認
        {
            collectedStars++;  // スコアを増やす
            Destroy(other.gameObject);  // 星を消す
            UpdateScore();
        }
        else if (other.gameObject.CompareTag("Trigger")) // 透明キューブに衝突した場合
        {
            SaveScoreAndLoadResult();  // スコアを保存し、結果シーンに遷移
        }
    }

    void UpdateScore()
    {
        scoreText.text = "Stars: " + collectedStars + "/" + totalStars;
    }

    // スコアを保存して結果シーンに遷移する
    void SaveScoreAndLoadResult()
    {
        // PlayerPrefsを使ってスコアを保存
        PlayerPrefs.SetInt("CollectedStars", collectedStars);
        PlayerPrefs.SetInt("TotalStars", totalStars);

        // 結果表示用シーンに遷移
        SceneManager.LoadScene("ResultSceneチ"); // シーン名を結果シーンの名前に置き換えてください
    }

}
