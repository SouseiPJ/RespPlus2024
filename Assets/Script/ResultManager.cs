using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ResultManager : MonoBehaviour
{
    public Text resultText;  // ���ʕ\���p��Text�I�u�W�F�N�g

    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefs����X�R�A���擾
        int collectedStars = PlayerPrefs.GetInt("CollectedStars");
        int totalStars = PlayerPrefs.GetInt("TotalStars");

        // �X�R�A�ɉ����ă��b�Z�[�W��\��
        if (collectedStars == totalStars)
        {
            resultText.text = "�G�N�Z�����g�I " + collectedStars + "/" + totalStars;
        }
        else if (collectedStars >= 10 && collectedStars < totalStars)
        {
            resultText.text = "�������I " + collectedStars + "/" + totalStars;
        }
        else
        {
            resultText.text = collectedStars + "/" + totalStars + "�Q�b�g";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   


}
