using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabSceneTo : MonoBehaviour
{
    // �{�^���������ꂽ���ɌĂяo�����֐�
    public void OnButtonPress()
    {
        // ���̃V�[���ɐ؂�ւ���B�V�[���������̃V�[�����ɒu�������Ă��������B
        SceneManager.LoadScene("LabScene");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
