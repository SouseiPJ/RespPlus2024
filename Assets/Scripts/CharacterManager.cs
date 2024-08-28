using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs; // �����X�^�[��Prefab�����X�g�ŕێ�
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

    // �����X�^�[���Ƃ�TextMeshPro�̎Q�Ƃ�ێ����鎫��
    private Dictionary<string, TextMeshProUGUI> collectedMonsterTexts = new Dictionary<string, TextMeshProUGUI>();

    // Start is called before the first frame update
    void Start()
    {
        meterController = FindObjectOfType<MeterController>(); // MeterController��������
        meterControllerOff = FindObjectOfType<MeterControllerOff>(); // MeterControllerOff��������

        StartDatetime = DateTime.Now;
        LoadLastSpawnSpan();
        CalculateMissedSpawns();

        // �e�����X�^�[�̎�ނ��Ƃ�TextMeshPro�̎Q�Ƃ�ݒ�
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

            // �݌v��]�����X�V
            if (totalRotationText != null)
            {
                totalRotationText.text = $" {Mathf.FloorToInt(currentRotationCount)}";
            }

            // �����X�p�����ƂɃ��Z�b�g������]�����X�V
            if (spanRotationText != null)
            {
                spanRotationText.text = $" {Mathf.FloorToInt(rotationDifference)}/{spawnSpan}";
            }

            // �����X�p�����ƂɃ����X�^�[�𐶐�
            if (rotationDifference >= spawnSpan)
            {
                int monstersToSpawn = Mathf.FloorToInt(rotationDifference / spawnSpan);
                for (int i = 0; i < monstersToSpawn; i++)
                {
                    SpawnRandomMonster();
                }
                lastRotationCount += monstersToSpawn * spawnSpan; // �����������̉�]�������Z
            }
        }

        // �������ʂɂ�郂���X�^�[�̉��
        if (meterControllerOff != null)
        {
            if (meterControllerOff.RecognizedSound == "�͂�" || meterControllerOff.RecognizedSound == "�ς�")
            {
                int monstersToRemove = meterControllerOff.GetMonstersToRemove();
                for (int i = 0; i < monstersToRemove; i++)
                {
                    CollectMonster();
                }
                meterControllerOff.ClearRecognizedSound(); // �F�����ꂽ�����N���A
            }
        }

        // �����X�^�[�̎�ނ��Ƃ̃J�E���g���X�V
        UpdateMonsterCountText();
    }

    void SpawnRandomMonster()
    {
        if (currentMonsterCount >= maxMonsters)
        {
            Debug.Log("�����X�^�[�̍ő吔�ɒB���Ă��܂��B");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, monsterPrefabs.Count);
        Vector3 randomPosition = GetRandomSpawnPosition();
        GameObject monster = Instantiate(monsterPrefabs[randomIndex], randomPosition, Quaternion.identity);
        monster.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // �X�P�[����(0.3, 0.3, 0.3)�ɐݒ�
        spawnedMonsters.Add(monster); // �������ꂽ�����X�^�[�����X�g�ɒǉ�

        // �����X�^�[�̎�ނ��Ƃ̃J�E���g���X�V
        string monsterType = monsterPrefabs[randomIndex].name;
        if (!monsterCounts.ContainsKey(monsterType))
        {
            monsterCounts[monsterType] = 0;
        }
        monsterCounts[monsterType]++;

        // �����A�j���[�V�������Đ�
        StartCoroutine(PlaySpawnAnimation(monster));

        lastSpawnSpan = DateTime.Now - StartDatetime;
        SaveLastSpawnSpan();
        currentMonsterCount++; // �����X�^�[���𑝉�
    }

    void CollectMonster()
    {
        if (currentMonsterCount > 0 && spawnedMonsters.Count > 0)
        {
            GameObject monsterToRemove = spawnedMonsters[0];
            spawnedMonsters.RemoveAt(0); // ���X�g����폜

            // �����X�^�[�̎�ނ��Ƃ̃J�E���g���X�V
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

            // ����A�j���[�V�������Đ�
            StartCoroutine(PlayCollectAnimation(monsterToRemove));

            currentMonsterCount--; // �����X�^�[��������
            collectedMonsterCount++; // ������������X�^�[���𑝉�

            // ������������X�^�[�̐����X�V
            if (collectedMonsterText != null)
            {
                collectedMonsterText.text = $" {collectedMonsterCount}";
            }

            // �����X�^�[���Ƃ�TextMeshPro���X�V
            if (collectedMonsterTexts.ContainsKey(monsterType))
            {
                collectedMonsterTexts[monsterType].text = $"{monsterType}: {collectedMonsterCounts[monsterType]}";
            }

            Debug.Log("�����X�^�[��������܂����B");
        }
        else
        {
            Debug.Log("������郂���X�^�[�����܂���B");
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
        Vector3 finalScale = new Vector3(20f, 20f, 20f); // ���̑傫����20�{

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
                monsterCountText.text += $"{kvp.Key}: {kvp.Value}";
            }
        }
    }
}
