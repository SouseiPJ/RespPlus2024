using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectsceneTo : MonoBehaviour
{
    // ボタンが押された時に呼び出される関数
    public void OnButtonPress()
    {
        // 次のシーンに切り替える。シーン名を次のシーン名に置き換えてください。
        SceneManager.LoadScene("CollectScene 1");
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
