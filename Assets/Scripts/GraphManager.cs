using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    public GraphLine[] monsterLines; // �s���N�A�A���F��3��ނ̃����X�^�[�̃O���t���C��
    private bool isRecording = false;
    private float startTime = 0f;

    public void StartRecording()
    {
        isRecording = true;
        startTime = Time.time;
        foreach (var line in monsterLines)
        {
            line.StartNewLine();
        }
    }

    public void StopRecording()
    {
        isRecording = false;
        SaveData(); // �K�v�ł���΃f�[�^��ۑ����鏈����ǉ�
    }

    public void UpdateMonsterCount(int monsterType, int count)
    {
        if (isRecording && monsterType >= 0 && monsterType < monsterLines.Length)
        {
            float elapsedTime = Time.time - startTime;
            monsterLines[monsterType].AddDataPoint(elapsedTime, count);
        }
    }

    private void SaveData()
    {
        // �f�[�^�ۑ�������ǉ��i�K�v�ɉ����āj
    }

}
