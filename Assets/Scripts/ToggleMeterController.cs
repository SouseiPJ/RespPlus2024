using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // �ǉ�: TextMeshPro�̖��O���

public class ToggleMeterController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField] private MeterController meterController;
    [SerializeField] private MeterControllerOff meterControllerOff;
    [SerializeField] private Slider toggleSlider;
    [SerializeField] private Image background; // �w�i��Image�R���|�[�l���g
    [SerializeField] private TextMeshProUGUI statusText; // �ύX: Text����TextMeshProUGUI�ɕύX

    [Header("�A�j���[�V����")]
    [SerializeField, Range(0, 1f)] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _animateSliderCoroutine;

    [Header("�C�x���g")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;

    [Header("�F�ݒ�")]
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;
    [SerializeField] private Color backgroundColorOn = Color.blue; // �g�O���I�����̔w�i�F
    [SerializeField] private Color backgroundColorOff = Color.gray; // �g�O���I�t���̔w�i�F

    private bool _currentValue;

    void Start()
    {
        if (toggleSlider != null)
        {
            toggleSlider.onValueChanged.AddListener(ToggleControllerState);
        }

        // ������Ԃ�MeterControllerOff�𖳌���
        meterControllerOff.enabled = false;
        UpdateSliderColor();
        UpdateBackgroundColor();
        UpdateStatusText(); // �ǉ�: ������Ԃ̃e�L�X�g���X�V
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // �X���C�_�[�̈ʒu�����Z�b�g
        toggleSlider.value = _currentValue ? 1 : 0;
        // �A�j���[�V�������J�n
        if (_animateSliderCoroutine != null)
            StopCoroutine(_animateSliderCoroutine);

        _animateSliderCoroutine = StartCoroutine(AnimateSlider());

        // �X���C�_�[�̑I����Ԃ�����
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

        // �}�C�N�����Z�b�g
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
        UpdateStatusText(); // �ǉ�: �X���C�_�[�̏�Ԃɉ����ăe�L�X�g���X�V
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
        UpdateStatusText(); // �ǉ�: �A�j���[�V�����I����Ƀe�L�X�g���X�V
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
