using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs; // モンスターのPrefabをリストで保持
    public Transform spawnArea; // モンスターが生成される範囲（例: 2Dの画面上の位置）
    public int maxMonsters = 70; // 生成されるモンスターの最大数
    [SerializeField] private float spawnSpan = 100f; // 生成スパン（プロペラの回転数）

    private DateTime StartDatetime;
    private TimeSpan lastSpawnSpan; // 最後にモンスターが生成された時刻
    private const string LastSpawnSpanKey = "LastSpawnSpan"; // PlayerPrefsのキー
    private int currentMonsterCount = 0; // 現在のモンスター数

    private MeterController meterController; // MeterControllerの参照
    private float lastRotationCount = 0f; // 最後にチェックした回転数

    [SerializeField] private TextMeshProUGUI totalRotationText; // 累計回転数を表示するTextMeshPro
    [SerializeField] private TextMeshProUGUI spanRotationText; // 生成スパンごとにリセットされる回転数を表示するTextMeshPro

    // Start is called before the first frame update
    void Start()
    {
        meterController = FindObjectOfType<MeterController>(); // MeterControllerを見つける

        StartDatetime = DateTime.Now;
        LoadLastSpawnSpan();
        CalculateMissedSpawns();
    }

    void Update()
    {
        if (meterController != null)
        {
            float currentRotationCount = meterController.RotationCount;
            float rotationDifference = currentRotationCount - lastRotationCount;

            // 累計回転数を更新
            if (totalRotationText != null)
            {
                totalRotationText.text = $" {Mathf.FloorToInt(currentRotationCount)}";
            }

            // 生成スパンごとにリセットされる回転数を更新
            if (spanRotationText != null)
            {
                spanRotationText.text = $" {Mathf.FloorToInt(rotationDifference)}/{spawnSpan}";
            }

            // 生成スパンごとにモンスターを生成
            if (rotationDifference >= spawnSpan)
            {
                int monstersToSpawn = Mathf.FloorToInt(rotationDifference / spawnSpan);
                for (int i = 0; i < monstersToSpawn; i++)
                {
                    SpawnRandomMonster();
                }
                lastRotationCount += monstersToSpawn * spawnSpan; // 生成した分の回転数を加算
            }
        }
    }

    void SpawnRandomMonster()
    {
        if (currentMonsterCount >= maxMonsters)
        {
            Debug.Log("モンスターの最大数に達しています。");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, monsterPrefabs.Count);
        Vector3 randomPosition = GetRandomSpawnPosition();
        Instantiate(monsterPrefabs[randomIndex], randomPosition, Quaternion.identity);
        lastSpawnSpan = DateTime.Now - StartDatetime;
        SaveLastSpawnSpan();
        currentMonsterCount++; // モンスター数を増加
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (spawnArea == null)
        {
            return Vector3.zero;
        }
        Vector3 min = spawnArea.GetComponent<Collider>().bounds.min;
        Vector3 max = spawnArea.GetComponent<Collider>().bounds.max;
        return new Vector3(
            UnityEngine.Random.Range(min.x, max.x),
            UnityEngine.Random.Range(min.y, max.y),
            UnityEngine.Random.Range(min.z, max.z)
        );
    }

    void SaveLastSpawnSpan()
    {
        PlayerPrefs.SetString(LastSpawnSpanKey, lastSpawnSpan.ToString());
        PlayerPrefs.Save();
    }

    void LoadLastSpawnSpan()
    {
        if (PlayerPrefs.HasKey(LastSpawnSpanKey))
        {
            lastSpawnSpan = TimeSpan.Parse(PlayerPrefs.GetString(LastSpawnSpanKey));
        }
        else
        {
            lastSpawnSpan = DateTime.Now - StartDatetime;
        }
    }

    void CalculateMissedSpawns()
    {
        // このメソッドは現在の文脈では不要です
    }

    public void OnMonsterDestroyed()
    {
        currentMonsterCount--;
        currentMonsterCount = Mathf.Max(currentMonsterCount, 0); // カウントが負になるのを防ぐ
    }
}
