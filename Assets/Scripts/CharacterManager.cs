using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs;
    public Transform spawnArea;
    public int maxMonsters = 70;
    [SerializeField] private float spawnSpan = 100f;

    private DateTime StartDatetime;
    private int currentMonsterCount = 0;
    private int collectedMonsterCount = 0;

    private MeterController meterController;
    private MeterControllerOff meterControllerOff;
    private float lastRotationCount = 0f;
    private List<GameObject> spawnedMonsters = new List<GameObject>();

    private Dictionary<string, int> monsterCounts = new Dictionary<string, int>();
    private Dictionary<string, int> collectedMonsterCounts = new Dictionary<string, int>();

    [SerializeField] private TextMeshProUGUI collectedMonsterText;

    private Dictionary<string, TextMeshProUGUI> collectedMonsterTexts = new Dictionary<string, TextMeshProUGUI>();
    private Announce announce;

    public GraphManager graphManager; // GraphManagerの参照を追加
    public Slider collectModeSlider; // モード切り替えスライダー
 // モード切り替えボタン

    private bool isCollectMode = false; // モードの状態管理

    void Start()
    {
        meterController = FindObjectOfType<MeterController>();
        meterControllerOff = FindObjectOfType<MeterControllerOff>();
        announce = FindObjectOfType<Announce>();

        collectModeSlider.onValueChanged.AddListener(ToggleCollectMode);

        foreach (var prefab in monsterPrefabs)
        {
            
            string monsterType = prefab.name;
          //  Debug.Log(GameObject.Find($"{monsterType}CollectedText"));
          //  TextMeshProUGUI textMeshPro = GameObject.Find($"{monsterType}CollectedText").GetComponent<TextMeshProUGUI>();
            
          //  collectedMonsterTexts[monsterType] = textMeshPro;
        }
    }

    void Update()
    {
        if (meterController != null)
        {
            float currentRotationCount = meterController.RotationCount;
            float rotationDifference = currentRotationCount - lastRotationCount;

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
    }

    // モード切り替えメソッド
    private void ToggleCollectMode(float value)
    {
        isCollectMode = !isCollectMode;

        if (isCollectMode)
        {
            graphManager.StartRecording();
        }
        else
        {
            graphManager.StopRecording();
        }
    }

    void SpawnRandomMonster()
    {
        if (currentMonsterCount >= maxMonsters)
        {
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

        currentMonsterCount++;
    }

    void CollectMonstersWithTag(string tag)
    {
        Debug.Log(3);
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

                currentMonsterCount--;
                collectedMonsterCount++;

                // カウントをグラフに更新
                if (isCollectMode)
                {
                    Debug.Log(2);
                    int monsterIndex = monsterPrefabs.FindIndex(prefab => prefab.name == monsterType);
                    graphManager.UpdateMonsterCount(monsterIndex, collectedMonsterCounts[monsterType]);
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
}
