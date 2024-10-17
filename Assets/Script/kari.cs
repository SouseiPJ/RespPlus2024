using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kari : MonoBehaviour
{
    public float moveSpeed = 1.0f; // �E�ւ̈ړ����x
    public float floatAmplitude = 0.5f; // �㉺�̐U��
    public float floatFrequency = 1.0f; // �㉺�̎��g��
    public float verticalMoveSpeed = 10.0f; // W/S�L�[�ɂ��㉺�ړ��̑��x

    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // �F����s�m���E�Ɉړ�������
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        // ���d�͊����o�����߂ɏ㉺�ɗh���
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        // W�L�[�ŏ�ړ��AS�L�[�ŉ��ړ�
        if (Input.GetKey(KeyCode.W))
        {
            newY += verticalMoveSpeed * Time.deltaTime; // ������ɉ��Z
        }
        if (Input.GetKey(KeyCode.S))
        {
            newY -= verticalMoveSpeed * Time.deltaTime; // �������ɉ��Z
        }

        // �V�����ʒu�ɏ㉺�̗h��ƃL�[�ɂ��ړ��𔽉f
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
