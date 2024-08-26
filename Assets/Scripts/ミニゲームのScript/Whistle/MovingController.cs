using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingController : MonoBehaviour
{
    public float jumpForce = 5f;  // �W�����v��
    public float moveForwardForce = 2f;  // �O�i�����

    private Rigidbody2D rb;
    private bool isGrounded = true;  // �Q�[���J�n���ɒn�ʂɂ���ꍇ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // �X�y�[�X�L�[�������ꂽ��W�����v����
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("�W�����v�I");
            rb.velocity = new Vector2(moveForwardForce, jumpForce);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("�n�ʂɒ��n�I");
            isGrounded = true;
        }
    }
}
