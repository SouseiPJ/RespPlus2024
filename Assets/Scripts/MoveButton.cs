using UnityEngine;
using System.Collections;

public class MoveButton : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // �ړ����������I�u�W�F�N�g���C���X�y�N�^�[�Őݒ�
    [SerializeField] private float moveDistance = 240f; // �ړ��������C���X�y�N�^�[�Őݒ�
    [SerializeField] private float moveSpeed = 1f; // �ړ����x���C���X�y�N�^�[�Őݒ�

    private Vector3 originalPosition; // �I�u�W�F�N�g�̌��̈ʒu��ۑ�
    private bool isMovedDown = false; // �I�u�W�F�N�g�����Ɉړ��������ǂ�����ǐ�

    private void Start()
    {
        // �I�u�W�F�N�g�̌��̈ʒu��ۑ�
        originalPosition = targetObject.GetComponent<RectTransform>().anchoredPosition;
    }

    public void ToggleMove()
    {
        if (isMovedDown)
        {
            // ���̈ʒu�ɖ߂�
            StartCoroutine(MoveToPosition(originalPosition));
        }
        else
        {
            // ���Ɉړ�
            Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y - moveDistance, originalPosition.z);
            StartCoroutine(MoveToPosition(newPosition));
        }

        // �t���O�𔽓]
        isMovedDown = !isMovedDown;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = targetObject.GetComponent<RectTransform>().anchoredPosition;
        float elapsedTime = 0;

        while (elapsedTime < moveSpeed)
        {
            targetObject.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / moveSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetObject.GetComponent<RectTransform>().anchoredPosition = targetPosition;
    }
}
