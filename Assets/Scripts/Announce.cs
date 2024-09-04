using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Announce : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI announcementText; // アナウンス用のTextMeshPro
    [SerializeField] private RectTransform announcementArea; // アナウンス表示エリア
    [SerializeField] private Slider announcementSlider; // スライダー

    private string defaultMessage = "息をふーと吹きかけて沢山召喚させよう！";
    private Queue<(string, Color)> messageQueue = new Queue<(string, Color)>();
    private bool isDisplayingMessage = false;
    private Coroutine currentMessageCoroutine;

    void Start()
    {
        // TextMeshProの透明度を設定
        if (announcementText != null)
        {
            Color textColor = announcementText.color;
            textColor.a = 1f; // Alpha値を1に設定して不透明にする
            announcementText.color = textColor;
        }

        // デフォルトメッセージを表示
        DisplayDefaultMessage();

        // スライダーの値が変更されたときに呼び出されるメソッドを設定
        if (announcementSlider != null)
        {
            announcementSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void Update()
    {
        if (!isDisplayingMessage && messageQueue.Count > 0)
        {
            var (message, color) = messageQueue.Dequeue();
            Debug.Log($"Displaying message: {message} with color: {color}");
            DisplayMessage(message, color);
        }
    }

    public void AddMessage(string message, Color color)
    {
        Debug.Log($"Message added: {message} with color: {color}");
        messageQueue.Enqueue((message, color));
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        currentMessageCoroutine = StartCoroutine(DisplayTemporaryMessage(message, color));
    }

    private void DisplayDefaultMessage()
    {
        if (!isDisplayingMessage)
        {
            DisplayMessage(defaultMessage, new Color(0.8f, 0.8f, 0.4f)); // くすみがかった黄色
        }
    }

    private void DisplayMessage(string message, Color color)
    {
        isDisplayingMessage = true;
        announcementText.text = message;
        announcementText.color = color;
        announcementText.gameObject.SetActive(true);
    }

    private IEnumerator DisplayTemporaryMessage(string message, Color color)
    {
        DisplayMessage(message, color);
        yield return new WaitForSeconds(5f);
        DisplayMessage("はっやぱっと息を吹き込んで捕獲しよう！", new Color(0.8f, 0.8f, 0.4f));
    }

    private void OnSliderValueChanged(float value)
    {
        if (value == 1)
        {
            AddMessage("はっやぱっと息を吹き込んで捕獲しよう！", new Color(0.8f, 0.8f, 0.4f));
        }
        else if (value == 0)
        {
            DisplayMessage(defaultMessage, new Color(0.8f, 0.8f, 0.4f));
        }
    }
}
