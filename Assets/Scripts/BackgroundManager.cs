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
        // 初期設定
        ShowBackground(background_Garden);

        // ボタンのクリックイベントを設定
        Button_Garden.onClick.AddListener(() => ShowBackground(background_Garden));
        Button_Star.onClick.AddListener(() => ShowBackground(background_Star));
        Button_Sand.onClick.AddListener(() => ShowBackground(background_Sand));
        Button_Desert.onClick.AddListener(() => ShowBackground(background_Desert));
    }

    void ShowBackground(GameObject backgroundToShow)
    {
        // すべての背景を透明化
        background_Garden.SetActive(false);
        background_Star.SetActive(false);
        background_Sand.SetActive(false);
        background_Desert.SetActive(false);

        // 指定された背景を表示
        backgroundToShow.SetActive(true);
    }
}
