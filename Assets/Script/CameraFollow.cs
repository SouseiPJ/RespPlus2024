using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform astronaut; // �F����s�m��Transform
    public float leftOffset = -5f; // �F����s�m����ʂ̍����ɔz�u����邽�߂̃I�t�Z�b�g
    public float followSpeed = 2.0f; // �J�������Ǐ]���鑬�x

    void LateUpdate()
    {
        // �J������x���W�݂̂��F����s�m�̈ʒu�Ɋ�Â��Ĉړ�������
        // y����z���͌Œ�
        Vector3 targetPosition = new Vector3(astronaut.position.x + leftOffset, transform.position.y, transform.position.z);

        // �J���������炩�ɒǏ]������
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }


}
