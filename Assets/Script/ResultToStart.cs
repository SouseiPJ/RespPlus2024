using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultToStart : MonoBehaviour
{
    // �{�^���������ꂽ���ɌĂяo�����֐�
    public void OnButtonPress()
    {
        // ���̃V�[���ɐ؂�ւ���B�V�[���������̃V�[�����ɒu�������Ă��������B
        SceneManager.LoadScene("StartScene�`");
    }

}
