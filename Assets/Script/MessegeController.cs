using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessegeController : MonoBehaviour
{
    public Text messageText; // �\������Text�R���|�[�l���g
    private string[] messages = {
        "�ċz�̃g���[�j���O�ɂ����đ厖�ȃ|�C���g",
        "�����E�͋����E���Y���悭",
        "���𒷂��f���o�����Ƃɒ��ӂ��܂��傤"
    };
    public float displayDuration = 5.0f; // �\�����鎞�ԁi�b�j
    public float fadeDuration = 2.0f; // �t�F�[�h�C��/�t�F�[�h�A�E�g�̎��ԁi�b�j

    private int currentIndex = 0; // ���݂̃��b�Z�[�W�C���f�b�N�X

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DisplayMessages());
    }

    private IEnumerator DisplayMessages()
    {
        while (true)
        {
            // �t�F�[�h�C��
            yield return StartCoroutine(FadeInText());

            // �\�����ԕ��ҋ@
            yield return new WaitForSeconds(displayDuration);

            // �t�F�[�h�A�E�g
            yield return StartCoroutine(FadeOutText());

            // �C���f�b�N�X�����ɐi�߁A�z��̍Ō�܂ōs������ŏ��ɖ߂�
            currentIndex = (currentIndex + 1) % messages.Length;
        }
    }


    private IEnumerator FadeInText()
    {
        messageText.text = messages[currentIndex];
        messageText.enabled = true;
        Color color = messageText.color;
        color.a = 0; // ��������X�^�[�g
        messageText.color = color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration); // ���X�ɓ����x���グ��
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
            color.a = Mathf.Clamp01(1 - (elapsedTime / fadeDuration)); // ���X�ɓ����x��������
            messageText.color = color;
            yield return null;
        }
        messageText.enabled = false;
    }


}
