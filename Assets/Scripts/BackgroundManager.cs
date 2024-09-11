using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    public GameObject background_Garden;
    public GameObject background_Star;
    public GameObject background_Sand;
    public GameObject background_Desert;

    public Button Button_Garden;
    public Button Button_Star;
    public Button Button_Sand;
    public Button Button_Desert;

    void Start()
    {
        // �����ݒ�
        ShowBackground(background_Garden);

        // �{�^���̃N���b�N�C�x���g��ݒ�
        Button_Garden.onClick.AddListener(() => ShowBackground(background_Garden));
        Button_Star.onClick.AddListener(() => ShowBackground(background_Star));
        Button_Sand.onClick.AddListener(() => ShowBackground(background_Sand));
        Button_Desert.onClick.AddListener(() => ShowBackground(background_Desert));
    }

    void ShowBackground(GameObject backgroundToShow)
    {
        // ���ׂĂ̔w�i�𓧖���
        background_Garden.SetActive(false);
        background_Star.SetActive(false);
        background_Sand.SetActive(false);
        background_Desert.SetActive(false);

        // �w�肳�ꂽ�w�i��\��
        backgroundToShow.SetActive(true);
    }
}
