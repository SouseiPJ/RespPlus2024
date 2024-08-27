using System;
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

    private MeterController meterController; // MeterController�̎Q��
    private float lastRotationCount = 0f; // �Ō�Ƀ`�F�b�N������]��

    [SerializeField] private TextMeshProUGUI totalRotationText; // �݌v��]����\������TextMeshPro
    [SerializeField] private TextMeshProUGUI spanRotationText; // �����X�p�����ƂɃ��Z�b�g������]����\������TextMeshPro

    // Start is called before the first frame update
    void Start()
    {
        meterController = FindObjectOfType<MeterController>(); // MeterController��������

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
        Instantiate(monsterPrefabs[randomIndex], randomPosition, Quaternion.identity);
        lastSpawnSpan = DateTime.Now - StartDatetime;
        SaveLastSpawnSpan();
        currentMonsterCount++; // �����X�^�[���𑝉�
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
        // ���̃��\�b�h�͌��݂̕����ł͕s�v�ł�
    }

    public void OnMonsterDestroyed()
    {
        currentMonsterCount--;
        currentMonsterCount = Mathf.Max(currentMonsterCount, 0); // �J�E���g�����ɂȂ�̂�h��
    }
}
