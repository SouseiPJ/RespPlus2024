using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs; // 統一されたモンスターのPrefabリスト
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

    private Dictionary<string, TextMeshProUGUI> collectedMonsterTexts = new Dictionary<string, TextMeshProUGUI>();

    private Announce announce; // Announceクラスの参照

    void Start()
    {
        meterController = FindObjectOfType<MeterController>();
        meterControllerOff = FindObjectOfType<MeterControllerOff>();
        announce = FindObjectOfType<Announce>(); // Announceクラスを見つける

        StartDatetime = DateTime.Now;
        LoadLastSpawnSpan();
        CalculateMissedSpawns();

        foreach (var prefab in monsterPrefabs)
        {
            string monsterType = prefab.name;
            TextMeshProUGUI textMeshPro = GameObject.Find($"{monsterType}CollectedText").GetComponent<TextMeshProUGUI>();
            collectedMonsterTexts[monsterType] = textMeshPro;
        }

        // 初期化コードを追加
        if (monsterCountText != null)
        {
            monsterCountText.text = "0\n0\n0";
        }
    }

    void Update()
    {
        if (meterController != null)
        {
            float currentRotationCount = meterController.RotationCount;
            float rotationDifference = currentRotationCount - lastRotationCount;

            if (totalRotationText != null)
            {
                totalRotationText.text = $" {Mathf.FloorToInt(currentRotationCount)}";
            }

            if (spanRotationText != null)
            {
                spanRotationText.text = $" {Mathf.FloorToInt(rotationDifference)}/{spawnSpan}";
            }

            if (rotationDifference >= spawnSpan)
            {
                int monstersToSpawn = Mathf.FloorToInt(rotationDifference / spawnSpan);
                for (int i = 0; i < monstersToSpawn; i++)
                {
                    SpawnRandomMonster();
                }
                lastRotationCount += monstersToSpawn * spawnSpan;
            }
        }

        if (meterControllerOff != null)
        {
            if (meterControllerOff.RecognizedSound == "はっ")
            {
                CollectMonstersWithTag("HaMonster");
            }
            else if (meterControllerOff.RecognizedSound == "ぱっ")
            {
                CollectMonstersWithTag("PaMonster");
            }
            meterControllerOff.ClearRecognizedSound();
        }

        UpdateMonsterCountText();
    }

    void SpawnRandomMonster()
    {
        if (currentMonsterCount >= maxMonsters)
        {
            Debug.Log("モンスターの最大数に達しています。");
            announce.AddMessage("モンスターの数がいっぱいだよ！捕獲しよう！", new Color(0.6f, 0.4f, 0.4f));
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, monsterPrefabs.Count);
        Vector3 randomPosition = GetRandomSpawnPosition();
        GameObject monster = Instantiate(monsterPrefabs[randomIndex], randomPosition, Quaternion.identity);

        monster.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        spawnedMonsters.Add(monster);

        string monsterType = monster.name.Replace("(Clone)", "").Trim();
        if (!monsterCounts.ContainsKey(monsterType))
        {
            monsterCounts[monsterType] = 0;
        }
        monsterCounts[monsterType]++;

        monster.AddComponent<RandomMovement>();
        StartCoroutine(PlaySpawnAnimation(monster));

        lastSpawnSpan = DateTime.Now - StartDatetime;
        SaveLastSpawnSpan();
        currentMonsterCount++;
    }

    void CollectMonstersWithTag(string tag)
    {
        int monstersToRemove = meterControllerOff.GetMonstersToRemove();
        for (int i = 0; i < monstersToRemove; i++)
        {
            GameObject monsterToRemove = spawnedMonsters.Find(monster => monster.CompareTag(tag));
            if (monsterToRemove != null)
            {
                spawnedMonsters.Remove(monsterToRemove);

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

                StartCoroutine(PlayCollectAnimation(monsterToRemove));

                currentMonsterCount--;
                collectedMonsterCount++;

                if (collectedMonsterText != null)
                {
                    collectedMonsterText.text = $" {collectedMonsterCount}";
                }

                if (collectedMonsterTexts.ContainsKey(monsterType))
                {
                    collectedMonsterTexts[monsterType].text = $"{monsterType}: {collectedMonsterCounts[monsterType]}";
                }

                Debug.Log("モンスターを回収しました。");

                // Announceにメッセージを送信
                if (announce != null)
                {
                    announce.AddMessage("モンスターを捕獲！！！やったね！！！", new Color(0.4f, 0.6f, 0.4f)); // くすんだ緑色
                }
            }
            else
            {
                bool otherTagMonstersExist = spawnedMonsters.Exists(monster => monster.CompareTag(tag == "PaMonster" ? "HaMonster" : "PaMonster"));
                if (otherTagMonstersExist)
                {
                    string otherTagMessage = tag == "PaMonster" ? "はっと息を吹きかけて残りのモンスターを捕獲しよう！" : "ぱっと息を吹きかけて残りのモンスターを捕獲しよう！";
                    announce.AddMessage(otherTagMessage, new Color(0.6f, 0.4f, 0.4f)); // くすんだ赤色
                }
                else
                {
                    Debug.Log("回収するモンスターがいません。");
                    announce.AddMessage("捕獲できるモンスターはいないよ！召喚しよう！", new Color(0.6f, 0.4f, 0.4f)); // くすんだ赤色
                }
            }
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
        Vector3 finalScale = new Vector3(10f, 10f, 10f); // 元の大きさの10倍

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
                monsterCountText.text += $"{kvp.Value}\n";
            }
        }
    }
}
