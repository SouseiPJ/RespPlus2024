using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingController : MonoBehaviour
{
    public float jumpForce = 5f;  // ジャンプ力
    public float moveForwardForce = 2f;  // 前進する力

    private Rigidbody2D rb;
    private bool isGrounded = true;  // ゲーム開始時に地面にいる場合

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // スペースキーが押されたらジャンプ処理
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("ジャンプ！");
            rb.velocity = new Vector2(moveForwardForce, jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("地面に着地！");
            isGrounded = true;
        }
    }
}
