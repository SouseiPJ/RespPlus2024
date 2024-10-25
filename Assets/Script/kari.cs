using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kari : MonoBehaviour
{
    public float moveSpeed = 1.0f;          // 右への移動速度
    public float floatAmplitude = 0.5f;     // 上下の振幅
    public float floatFrequency = 1.0f;     // 上下の周波数
    public float verticalMoveSpeed = 10.0f; // W/Sキーによる上下移動の速度

    private Vector3 startPosition;
    private float verticalInputOffset = 0f; // W/Sキーによる上下の移動量

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 宇宙飛行士を右に移動させる
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        // 無重力感を出すために上下に揺れる (sin波)
        float floatY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // Wキーで上移動、Sキーで下移動
        if (Input.GetKey(KeyCode.W))
        {
            verticalInputOffset += verticalMoveSpeed * Time.deltaTime; // 上方向に移動
            Debug.Log("w!");
        }
        if (Input.GetKey(KeyCode.S))
        {
            verticalInputOffset -= verticalMoveSpeed * Time.deltaTime; // 下方向に移動
            Debug.Log("S!");
        }

        // 新しい位置に揺れとキーによる移動を反映
        float newY = startPosition.y + floatY + verticalInputOffset;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
