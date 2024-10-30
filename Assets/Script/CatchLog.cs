using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatchLog : MonoBehaviour
{
    private const int MaxLogLines = 20; // �\�����郍�O�̍ő�s��
    private string logText = "";
    private GUIStyle guiStyle = new GUIStyle();
    private bool showLogInGame = true;

    private float lastLogTime = 0f;

    private void Start()
    {
        // ���O�̃e�L�X�g���X�^�C���ɐݒ�
        guiStyle.fontSize = 50;
        guiStyle.normal.textColor = Color.red;

        
    }

    private void OnGUI()
    {
        // �Q�[����ʒ��Ƀ��O��\���iWindows�̃r���h���̂ݗL�����G�f�B�^�Ŏ��s���Ă��Ȃ��ꍇ�̂ݗL������0�L�[�ŕ\��/��\����؂�ւ��j

        

        if (showLogInGame)
        {
            GUI.Label(new Rect(50, 500, Screen.width, Screen.height), logText, guiStyle);
        }

    }

    private void OnEnable()
    {
        // �f�o�b�O���O��\�����邽�߂̃C�x���g�n���h����o�^
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        // �C�x���g�n���h��������
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        // �Q�[����ʓ��̃��O�\�����L���ȏꍇ�̂݁A3�b���ƂɃ��O�̃e�L�X�g���N���A
        if (showLogInGame && Time.time - lastLogTime > 10f)
        {
            logText = "";
        }
        
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Debug.Log()�̃e�L�X�g��logText�ɒǉ�
        logText += logString + "\n";

        // �\�����郍�O�̍s����MaxLogLines�𒴂�����A�Â����O���폜
        string[] logLines = logText.Split('\n');
        if (logLines.Length > MaxLogLines)
        {
            logText = string.Join("\n", logLines, logLines.Length - MaxLogLines, MaxLogLines);
        }

        // �Ō�Ƀ��O��\�������������X�V
        lastLogTime = Time.time;


    }
}

