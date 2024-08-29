using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Announce : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI announcementText; // �A�i�E���X�p��TextMeshPro
    [SerializeField] private RectTransform announcementArea; // �A�i�E���X�\���G���A
    [SerializeField] private Slider announcementSlider; // �X���C�_�[

    private string defaultMessage = "�����Ӂ[�Ɛ��������đ�R���������悤�I";
    private Queue<(string, Color)> messageQueue = new Queue<(string, Color)>();
    private bool isDisplayingMessage = false;
    private Coroutine currentMessageCoroutine;

    void Start()
    {
        // TextMeshPro�̓����x��ݒ�
        if (announcementText != null)
        {
            Color textColor = announcementText.color;
            textColor.a = 1f; // Alpha�l��1�ɐݒ肵�ĕs�����ɂ���
            announcementText.color = textColor;
        }

        // �f�t�H���g���b�Z�[�W��\��
        DisplayDefaultMessage();

        // �X���C�_�[�̒l���ύX���ꂽ�Ƃ��ɌĂяo����郁�\�b�h��ݒ�
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
            DisplayMessage(defaultMessage, new Color(0.8f, 0.8f, 0.4f)); // �����݂����������F
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
        DisplayMessage("�͂���ς��Ƒ��𐁂�����ŕߊl���悤�I", new Color(0.8f, 0.8f, 0.4f));
    }

    private void OnSliderValueChanged(float value)
    {
        if (value == 1)
        {
            AddMessage("�͂���ς��Ƒ��𐁂�����ŕߊl���悤�I", new Color(0.8f, 0.8f, 0.4f));
        }
        else if (value == 0)
        {
            DisplayMessage(defaultMessage, new Color(0.8f, 0.8f, 0.4f));
        }
    }
}
