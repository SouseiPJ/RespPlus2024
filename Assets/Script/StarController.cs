using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    // 宇宙飛行士と衝突したときに星を回収
    void OnTriggerEnter(Collider other)
    {
        // 衝突したオブジェクトが宇宙飛行士であるか確認
        if (other.CompareTag("Astronaut"))
        {
            // 星を回収して、星オブジェクトを削除
            CollectStar();
        }
    }

    // 星を回収する処理
    void CollectStar()
    {
        // デバッグログで回収を表示
        Debug.Log("Star Collected!");

        // 星オブジェクトを削除
        Destroy(gameObject);
    }
}
