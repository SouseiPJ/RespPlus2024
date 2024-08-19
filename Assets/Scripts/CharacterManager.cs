using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public List<GameObject> monsterPrefabs; // �����X�^�[��Prefab�����X�g�ŕێ�
    public Transform spawnArea; // �����X�^�[�����������͈́i��: 2D�̉�ʏ�̈ʒu�j
    public int maxMonsters = 70; // ��������郂���X�^�[�̍ő吔

    private float spawnInterval = 10f; // �����X�^�[�����������Ԋu�i�b�j
    private DateTime lastSpawnTime; // �Ō�Ƀ����X�^�[���������ꂽ����
    private const string LastSpawnTimeKey = "LastSpawnTime"; // PlayerPrefs�̃L�[
    private int currentMonsterCount = 0; // ���݂̃����X�^�[��

    // Start is called before the first frame update
    void Start()
    {
        LoadLastSpawnTime();
        CalculateMissedSpawns();
        InvokeRepeating(nameof(SpawnRandomMonster), spawnInterval, spawnInterval);
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
        lastSpawnTime = DateTime.Now;
        SaveLastSpawnTime();
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

    void SaveLastSpawnTime()
    {
        PlayerPrefs.SetString(LastSpawnTimeKey, lastSpawnTime.ToString());
        PlayerPrefs.Save();
    }

    void LoadLastSpawnTime()
    {
        if (PlayerPrefs.HasKey(LastSpawnTimeKey))
        {
            lastSpawnTime = DateTime.Parse(PlayerPrefs.GetString(LastSpawnTimeKey));
        }
        else
        {
            lastSpawnTime = DateTime.Now;
        }
    }
    //CalculateMissedSpawns �Ō�Ƀ����X�^�[���������ꂽ���Ԃ��猻�݂܂ł̌o�ߎ��Ԃ��v�Z
    void CalculateMissedSpawns()
    {
        TimeSpan timeElapsed = DateTime.Now - lastSpawnTime;
        int missedSpawns = Mathf.FloorToInt((float)timeElapsed.TotalSeconds / spawnInterval);

        for (int i = 0; i < missedSpawns; i++)
        {
            SpawnRandomMonster();
        }
    }

    public void OnMonsterDestroyed()
    {
        currentMonsterCount--;
        currentMonsterCount = Mathf.Max(currentMonsterCount, 0); // �J�E���g�����ɂȂ�̂�h��
    }

}
