using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ResultManager : MonoBehaviour
{
    public Text resultText;  // 結果表示用のTextオブジェクト

    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefsからスコアを取得
        int collectedStars = PlayerPrefs.GetInt("CollectedStars");
        int totalStars = PlayerPrefs.GetInt("TotalStars");

        // スコアに応じてメッセージを表示
        if (collectedStars == totalStars)
        {
            resultText.text = "エクセレント！ " + collectedStars + "/" + totalStars;
        }
        else if (collectedStars >= 10 && collectedStars < totalStars)
        {
            resultText.text = "おしい！ " + collectedStars + "/" + totalStars;
        }
        else
        {
            resultText.text = collectedStars + "/" + totalStars + "ゲット";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   


}
