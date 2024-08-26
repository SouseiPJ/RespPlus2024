using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs; // モンスターのPrefabをリストで保持
    public Transform spawnArea; // モンスターが生成される範囲（例: 2Dの画面上の位置）
    public int maxMonsters = 70; // 生成されるモンスターの最大数

    private float spawnInterval = 10f; // モンスターが生成される間隔（秒）
    private DateTime StartDatetime;
    private TimeSpan lastSpawnSpan; // 最後にモンスターが生成された時刻
    private const string LastSpawnSpanKey = "LastSpawnSpan"; // PlayerPrefsのキー
    private int currentMonsterCount = 0; // 現在のモンスター数

    // Start is called before the first frame update
    void Start()
    {
        StartDatetime = DateTime.Now;
        LoadLastSpawnSpan();
        CalculateMissedSpawns();
        InvokeRepeating(nameof(SpawnRandomMonster), spawnInterval, spawnInterval);
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
    //CalculateMissedSpawns 最後にモンスターが生成された時間から現在までの経過時間を計算
    void CalculateMissedSpawns()
    {
        TimeSpan timeElapsed = lastSpawnSpan;
        int missedSpawns = Mathf.FloorToInt((float)timeElapsed.TotalSeconds / spawnInterval);

        for (int i = 0; i < missedSpawns; i++)
        {
            SpawnRandomMonster();
        }
    }

    public void OnMonsterDestroyed()
    {
        currentMonsterCount--;
        currentMonsterCount = Mathf.Max(currentMonsterCount, 0); // カウントが負になるのを防ぐ
    }

}
