using System;
using System.Collections;
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
    private int collectedMonsterCount = 0; // 回収したモンスターの数

    private MeterController meterController; // MeterControllerの参照
    private MeterControllerOff meterControllerOff; // MeterControllerOffの参照
    private float lastRotationCount = 0f; // 最後にチェックした回転数

    private List<GameObject> spawnedMonsters = new List<GameObject>(); // 生成されたモンスターを管理するリスト

    private Dictionary<string, int> monsterCounts = new Dictionary<string, int>(); // モンスターの種類ごとのカウント
    private Dictionary<string, int> collectedMonsterCounts = new Dictionary<string, int>(); // 回収したモンスターの種類ごとのカウント

    [SerializeField] private TextMeshProUGUI totalRotationText; // 累計回転数を表示するTextMeshPro
    [SerializeField] private TextMeshProUGUI spanRotationText; // 生成スパンごとにリセットされる回転数を表示するTextMeshPro
    [SerializeField] private TextMeshProUGUI collectedMonsterText; // 回収したモンスターの数を表示するTextMeshPro
    [SerializeField] private TextMeshProUGUI monsterCountText; // モンスターの種類ごとのカウントを表示するTextMeshPro

    // モンスターごとのTextMeshProの参照を保持する辞書
    private Dictionary<string, TextMeshProUGUI> collectedMonsterTexts = new Dictionary<string, TextMeshProUGUI>();

    // Start is called before the first frame update
    void Start()
    {
        meterController = FindObjectOfType<MeterController>(); // MeterControllerを見つける
        meterControllerOff = FindObjectOfType<MeterControllerOff>(); // MeterControllerOffを見つける

        StartDatetime = DateTime.Now;
        LoadLastSpawnSpan();
        CalculateMissedSpawns();

        // 各モンスターの種類ごとにTextMeshProの参照を設定
        foreach (var prefab in monsterPrefabs)
        {
            string monsterType = prefab.name;
            TextMeshProUGUI textMeshPro = GameObject.Find($"{monsterType}CollectedText").GetComponent<TextMeshProUGUI>();
            collectedMonsterTexts[monsterType] = textMeshPro;
        }
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

        // 音声識別によるモンスターの回収
        if (meterControllerOff != null)
        {
            if (meterControllerOff.RecognizedSound == "はっ" || meterControllerOff.RecognizedSound == "ぱっ")
            {
                int monstersToRemove = meterControllerOff.GetMonstersToRemove();
                for (int i = 0; i < monstersToRemove; i++)
                {
                    CollectMonster();
                }
                meterControllerOff.ClearRecognizedSound(); // 認識された音をクリア
            }
        }

        // モンスターの種類ごとのカウントを更新
        UpdateMonsterCountText();
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
        GameObject monster = Instantiate(monsterPrefabs[randomIndex], randomPosition, Quaternion.identity);
        monster.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // スケールを(0.3, 0.3, 0.3)に設定
        spawnedMonsters.Add(monster); // 生成されたモンスターをリストに追加

        // モンスターの種類ごとのカウントを更新
        string monsterType = monsterPrefabs[randomIndex].name;
        if (!monsterCounts.ContainsKey(monsterType))
        {
            monsterCounts[monsterType] = 0;
        }
        monsterCounts[monsterType]++;

        // 生成アニメーションを再生
        StartCoroutine(PlaySpawnAnimation(monster));

        lastSpawnSpan = DateTime.Now - StartDatetime;
        SaveLastSpawnSpan();
        currentMonsterCount++; // モンスター数を増加
    }

    void CollectMonster()
    {
        if (currentMonsterCount > 0 && spawnedMonsters.Count > 0)
        {
            GameObject monsterToRemove = spawnedMonsters[0];
            spawnedMonsters.RemoveAt(0); // リストから削除

            // モンスターの種類ごとのカウントを更新
            string monsterType = monsterToRemove.name.Replace("(Clone)", "").Trim();
            if (monsterCounts.ContainsKey(monsterType))
            {
                monsterCounts[monsterType]--;
                if (!collectedMonsterCounts.ContainsKey(monsterType))
                {
                    collectedMonsterCounts[monsterType] = 0;
                }
                collectedMonsterCounts[monsterType]++;
            }

            // 回収アニメーションを再生
            StartCoroutine(PlayCollectAnimation(monsterToRemove));

            currentMonsterCount--; // モンスター数を減少
            collectedMonsterCount++; // 回収したモンスター数を増加

            // 回収したモンスターの数を更新
            if (collectedMonsterText != null)
            {
                collectedMonsterText.text = $" {collectedMonsterCount}";
            }

            // モンスターごとのTextMeshProを更新
            if (collectedMonsterTexts.ContainsKey(monsterType))
            {
                collectedMonsterTexts[monsterType].text = $"{monsterType}: {collectedMonsterCounts[monsterType]}";
            }

            Debug.Log("モンスターを回収しました。");
        }
        else
        {
            Debug.Log("回収するモンスターがいません。");
        }
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
        // このメソッドは現在の文脈では不要
    }

    public void OnMonsterDestroyed()
    {
        currentMonsterCount--;
        currentMonsterCount = Mathf.Max(currentMonsterCount, 0); // カウントが負になるのを防ぐ
    }

    private IEnumerator PlaySpawnAnimation(GameObject monster)
    {
        float duration = 0.5f; // アニメーションの持続時間
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = new Vector3(20f, 20f, 20f); // 元の大きさの20倍

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            monster.transform.localScale = Vector3.Lerp(initialScale, finalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        monster.transform.localScale = finalScale;
    }

    private IEnumerator PlayCollectAnimation(GameObject monster)
    {
        float duration = 1.0f; // アニメーションの持続時間
        Vector3 initialPosition = monster.transform.position;
        Vector3 finalPosition = initialPosition + Vector3.up * 50f; // 上に移動

        float elapsedTime = 0f;
        Renderer renderer = monster.GetComponent<Renderer>();
        Color initialColor = renderer.material.color;

        while (elapsedTime < duration)
        {
            // 位置を移動
            monster.transform.position = Vector3.Lerp(initialPosition, finalPosition, elapsedTime / duration);

            // アルファ値を変更して徐々に透明にする
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            renderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最後に完全に透明にする
        renderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        Destroy(monster);
    }

    void UpdateMonsterCountText()
    {
        if (monsterCountText != null)
        {
            monsterCountText.text = "";
            foreach (var kvp in collectedMonsterCounts)
            {
                monsterCountText.text += $"{kvp.Key}: {kvp.Value}";
            }
        }
    }
}
