using UnityEngine;
using System.Collections;

public class MoveButton : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // 移動させたいオブジェクトをインスペクターで設定
    [SerializeField] private float moveDistance = 240f; // 移動距離をインスペクターで設定
    [SerializeField] private float moveSpeed = 1f; // 移動速度をインスペクターで設定

    private Vector3 originalPosition; // オブジェクトの元の位置を保存
    private bool isMovedDown = false; // オブジェクトが下に移動したかどうかを追跡

    private void Start()
    {
        // オブジェクトの元の位置を保存
        originalPosition = targetObject.GetComponent<RectTransform>().anchoredPosition;
    }

    public void ToggleMove()
    {
        if (isMovedDown)
        {
            // 元の位置に戻す
            StartCoroutine(MoveToPosition(originalPosition));
        }
        else
        {
            // 下に移動
            Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y - moveDistance, originalPosition.z);
            StartCoroutine(MoveToPosition(newPosition));
        }

        // フラグを反転
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
