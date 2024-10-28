using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    public GraphLine[] monsterLines; // ピンク、青、黄色の3種類のモンスターのグラフライン
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
        SaveData(); // 必要であればデータを保存する処理を追加
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
        // データ保存処理を追加（必要に応じて）
    }

}
