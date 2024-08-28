using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // 追加: TextMeshProの名前空間

public class ToggleMeterController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField] private MeterController meterController;
    [SerializeField] private MeterControllerOff meterControllerOff;
    [SerializeField] private Slider toggleSlider;
    [SerializeField] private Image background; // 背景のImageコンポーネント
    [SerializeField] private TextMeshProUGUI statusText; // 変更: TextからTextMeshProUGUIに変更

    [Header("アニメーション")]
    [SerializeField, Range(0, 1f)] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _animateSliderCoroutine;

    [Header("イベント")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;

    [Header("色設定")]
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;
    [SerializeField] private Color backgroundColorOn = Color.blue; // トグルオン時の背景色
    [SerializeField] private Color backgroundColorOff = Color.gray; // トグルオフ時の背景色

    private bool _currentValue;

    void Start()
    {
        if (toggleSlider != null)
        {
            toggleSlider.onValueChanged.AddListener(ToggleControllerState);
        }

        // 初期状態でMeterControllerOffを無効化
        meterControllerOff.enabled = false;
        UpdateSliderColor();
        UpdateBackgroundColor();
        UpdateStatusText(); // 追加: 初期状態のテキストを更新
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // スライダーの位置をリセット
        toggleSlider.value = _currentValue ? 1 : 0;
        // アニメーションを開始
        if (_animateSliderCoroutine != null)
            StopCoroutine(_animateSliderCoroutine);

        _animateSliderCoroutine = StartCoroutine(AnimateSlider());

        // スライダーの選択状態を解除
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void Toggle()
    {
        SetStateAndStartAnimation(!_currentValue);
    }

    private void ToggleControllerState(float value)
    {
        bool isMeterControllerActive = value <= 0.5f;
        meterController.enabled = isMeterControllerActive;
        meterControllerOff.enabled = !isMeterControllerActive;

        // マイクをリセット
        if (isMeterControllerActive)
        {
            meterController.ResetMic();
        }
        else
        {
            meterControllerOff.ResetMic();
        }

        UpdateSliderColor();
        UpdateBackgroundColor();
        UpdateStatusText(); // 追加: スライダーの状態に応じてテキストを更新
    }

    private void SetStateAndStartAnimation(bool state)
    {
        bool previousValue = _currentValue;
        _currentValue = state;

        if (previousValue != _currentValue)
        {
            if (_currentValue)
                onToggleOn?.Invoke();
            else
                onToggleOff?.Invoke();
        }

        if (_animateSliderCoroutine != null)
            StopCoroutine(_animateSliderCoroutine);

        _animateSliderCoroutine = StartCoroutine(AnimateSlider());
    }

    private IEnumerator AnimateSlider()
    {
        float startValue = toggleSlider.value;
        float endValue = _currentValue ? 1 : 0;

        float time = 0;
        if (animationDuration > 0)
        {
            while (time < animationDuration)
            {
                time += Time.deltaTime;

                float lerpFactor = slideEase.Evaluate(time / animationDuration);
                toggleSlider.value = Mathf.Lerp(startValue, endValue, lerpFactor);

                yield return null;
            }
        }

        toggleSlider.value = endValue;
        UpdateSliderColor();
        UpdateBackgroundColor();
        UpdateStatusText(); // 追加: アニメーション終了後にテキストを更新
    }

    private void UpdateSliderColor()
    {
        ColorBlock colors = toggleSlider.colors;
        colors.normalColor = _currentValue ? onColor : offColor;
        toggleSlider.colors = colors;
    }

    private void UpdateBackgroundColor()
    {
        if (background != null)
        {
            background.color = _currentValue ? backgroundColorOn : backgroundColorOff;
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = _currentValue ? "Collect" : "Generate";
        }
    }
}
