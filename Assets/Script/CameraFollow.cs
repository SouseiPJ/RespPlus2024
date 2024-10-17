using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform astronaut; // 宇宙飛行士のTransform
    public float leftOffset = -5f; // 宇宙飛行士が画面の左側に配置されるためのオフセット
    public float followSpeed = 2.0f; // カメラが追従する速度

    void LateUpdate()
    {
        // カメラのx座標のみを宇宙飛行士の位置に基づいて移動させる
        // y軸とz軸は固定
        Vector3 targetPosition = new Vector3(astronaut.position.x + leftOffset, transform.position.y, transform.position.z);

        // カメラを滑らかに追従させる
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }


}
