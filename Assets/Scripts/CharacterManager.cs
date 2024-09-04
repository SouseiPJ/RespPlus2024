using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs; // ���ꂳ�ꂽ�����X�^�[��Prefab���X�g
    public Transform spawnArea; // �����X�^�[�����������͈́i��: 2D�̉�ʏ�̈ʒu�j
    public int maxMonsters = 70; // ��������郂���X�^�[�̍ő吔
    [SerializeField] private float spawnSpan = 100f; // �����X�p���i�v���y���̉�]���j

    private DateTime StartDatetime;
    private TimeSpan lastSpawnSpan; // �Ō�Ƀ����X�^�[���������ꂽ����
    private const string LastSpawnSpanKey = "LastSpawnSpan"; // PlayerPrefs�̃L�[
    private int currentMonsterCount = 0; // ���݂̃����X�^�[��
    private int collectedMonsterCount = 0; // ������������X�^�[�̐�

    private MeterController meterController; // MeterController�̎Q��
    private MeterControllerOff meterControllerOff; // MeterControllerOff�̎Q��
    private float lastRotationCount = 0f; // �Ō�Ƀ`�F�b�N������]��

    private List<GameObject> spawnedMonsters = new List<GameObject>(); // �������ꂽ�����X�^�[���Ǘ����郊�X�g

    private Dictionary<string, int> monsterCounts = new Dictionary<string, int>(); // �����X�^�[�̎�ނ��Ƃ̃J�E���g
    private Dictionary<string, int> collectedMonsterCounts = new Dictionary<string, int>(); // ������������X�^�[�̎�ނ��Ƃ̃J�E���g

    [SerializeField] private TextMeshProUGUI totalRotationText; // �݌v��]����\������TextMeshPro
    [SerializeField] private TextMeshProUGUI spanRotationText; // �����X�p�����ƂɃ��Z�b�g������]����\������TextMeshPro
    [SerializeField] private TextMeshProUGUI collectedMonsterText; // ������������X�^�[�̐���\������TextMeshPro
    [SerializeField] private TextMeshProUGUI monsterCountText; // �����X�^�[�̎�ނ��Ƃ̃J�E���g��\������TextMeshPro

    private Dictionary<string, TextMeshProUGUI> collectedMonsterTexts = new Dictionary<string, TextMeshProUGUI>();

    private Announce announce; // Announce�N���X�̎Q��

    void Start()
    {
        meterController = FindObjectOfType<MeterController>();
        meterControllerOff = FindObjectOfType<MeterControllerOff>();
        announce = FindObjectOfType<Announce>(); // Announce�N���X��������

        StartDatetime = DateTime.Now;
        LoadLastSpawnSpan();
        CalculateMissedSpawns();

        foreach (var prefab in monsterPrefabs)
        {
            string monsterType = prefab.name;
            TextMeshProUGUI textMeshPro = GameObject.Find($"{monsterType}CollectedText").GetComponent<TextMeshProUGUI>();
            collectedMonsterTexts[monsterType] = textMeshPro;
        }

        // �������R�[�h��ǉ�
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
            if (meterControllerOff.RecognizedSound == "�͂�")
            {
                CollectMonstersWithTag("HaMonster");
            }
            else if (meterControllerOff.RecognizedSound == "�ς�")
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
            Debug.Log("�����X�^�[�̍ő吔�ɒB���Ă��܂��B");
            announce.AddMessage("�����X�^�[�̐��������ς�����I�ߊl���悤�I", new Color(0.6f, 0.4f, 0.4f));
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

                Debug.Log("�����X�^�[��������܂����B");

                // Announce�Ƀ��b�Z�[�W�𑗐M
                if (announce != null)
                {
                    announce.AddMessage("�����X�^�[��ߊl�I�I�I������ˁI�I�I", new Color(0.4f, 0.6f, 0.4f)); // �����񂾗ΐF
                }
            }
            else
            {
                bool otherTagMonstersExist = spawnedMonsters.Exists(monster => monster.CompareTag(tag == "PaMonster" ? "HaMonster" : "PaMonster"));
                if (otherTagMonstersExist)
                {
                    string otherTagMessage = tag == "PaMonster" ? "�͂��Ƒ��𐁂������Ďc��̃����X�^�[��ߊl���悤�I" : "�ς��Ƒ��𐁂������Ďc��̃����X�^�[��ߊl���悤�I";
                    announce.AddMessage(otherTagMessage, new Color(0.6f, 0.4f, 0.4f)); // �����񂾐ԐF
                }
                else
                {
                    Debug.Log("������郂���X�^�[�����܂���B");
                    announce.AddMessage("�ߊl�ł��郂���X�^�[�͂��Ȃ���I�������悤�I", new Color(0.6f, 0.4f, 0.4f)); // �����񂾐ԐF
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
        // ���̃��\�b�h�͌��݂̕����ł͕s�v
    }

    public void OnMonsterDestroyed()
    {
        currentMonsterCount--;
        currentMonsterCount = Mathf.Max(currentMonsterCount, 0); // �J�E���g�����ɂȂ�̂�h��
    }

    private IEnumerator PlaySpawnAnimation(GameObject monster)
    {
        float duration = 0.5f; // �A�j���[�V�����̎�������
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = new Vector3(10f, 10f, 10f); // ���̑傫����10�{

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
        float duration = 1.0f; // �A�j���[�V�����̎�������
        Vector3 initialPosition = monster.transform.position;
        Vector3 finalPosition = initialPosition + Vector3.up * 50f; // ��Ɉړ�

        float elapsedTime = 0f;
        Renderer renderer = monster.GetComponent<Renderer>();
        Color initialColor = renderer.material.color;

        while (elapsedTime < duration)
        {
            // �ʒu���ړ�
            monster.transform.position = Vector3.Lerp(initialPosition, finalPosition, elapsedTime / duration);

            // �A���t�@�l��ύX���ď��X�ɓ����ɂ���
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            renderer.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �Ō�Ɋ��S�ɓ����ɂ���
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
