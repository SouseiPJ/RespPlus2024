using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    // �F����s�m�ƏՓ˂����Ƃ��ɐ������
    void OnTriggerEnter(Collider other)
    {
        // �Փ˂����I�u�W�F�N�g���F����s�m�ł��邩�m�F
        if (other.CompareTag("Astronaut"))
        {
            // ����������āA���I�u�W�F�N�g���폜
            CollectStar();
        }
    }

    // ����������鏈��
    void CollectStar()
    {
        // �f�o�b�O���O�ŉ����\��
        Debug.Log("Star Collected!");

        // ���I�u�W�F�N�g���폜
        Destroy(gameObject);
    }
}
