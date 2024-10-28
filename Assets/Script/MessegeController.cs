using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessegeController : MonoBehaviour
{
    public Text messageText; // 表示するTextコンポーネント
    private string[] messages = {
        "呼吸のトレーニングにおいて大事なポイント",
        "長く・力強く・リズムよく",
        "息を長く吐き出すことに注意しましょう"
    };
    public float displayDuration = 5.0f; // 表示する時間（秒）
    public float fadeDuration = 2.0f; // フェードイン/フェードアウトの時間（秒）

    private int currentIndex = 0; // 現在のメッセージインデックス

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DisplayMessages());
    }

    private IEnumerator DisplayMessages()
    {
        while (true)
        {
            // フェードイン
            yield return StartCoroutine(FadeInText());

            // 表示時間分待機
            yield return new WaitForSeconds(displayDuration);

            // フェードアウト
            yield return StartCoroutine(FadeOutText());

            // インデックスを次に進め、配列の最後まで行ったら最初に戻る
            currentIndex = (currentIndex + 1) % messages.Length;
        }
    }


    private IEnumerator FadeInText()
    {
        messageText.text = messages[currentIndex];
        messageText.enabled = true;
        Color color = messageText.color;
        color.a = 0; // 透明からスタート
        messageText.color = color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration); // 徐々に透明度を上げる
            messageText.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOutText()
    {
        Color color = messageText.color;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsedTime / fadeDuration)); // 徐々に透明度を下げる
            messageText.color = color;
            yield return null;
        }
        messageText.enabled = false;
    }


}
