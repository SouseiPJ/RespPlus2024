using UnityEngine;

public class GarageMove : MonoBehaviour
{
    public float moveDistance = 200f; // ˆÚ“®‹——£
    public float moveSpeed = 20f; // ˆÚ“®‘¬“x
    private Vector3 originalPosition;
    private bool isMoving = false;
    private bool moveUp = true;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isMoving)
        {
            if (moveUp)
            {
                MoveUp();
            }
            else
            {
                MoveDown();
            }
        }
    }

    public void OnButtonPress()
    {
        if (!isMoving)
        {
            isMoving = true;
            moveUp = !moveUp;
        }
    }

    private void MoveUp()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition + Vector3.up * moveDistance, moveSpeed * Time.deltaTime);
        if (transform.localPosition == originalPosition + Vector3.up * moveDistance)
        {
            isMoving = false;
        }
    }

    private void MoveDown()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, moveSpeed * Time.deltaTime);
        if (transform.localPosition == originalPosition)
        {
            isMoving = false;
        }
    }
}