using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float changeDirectionInterval = 2f;
    private Vector3 moveDirection;

    void Start()
    {
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            moveDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            yield return new WaitForSeconds(changeDirectionInterval);
        }
    }
}
