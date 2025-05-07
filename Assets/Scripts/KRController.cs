using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KRController : MonoBehaviour
{
    // 回転速度
    public float rotationRate = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        // 初期化処理が必要であればここに記述
    }

    // Update is called once per frame
    void Update()
    {
        // 現在の回転角度を取得
        Vector3 currentRotation = transform.localEulerAngles;

        // 矢印キー上を押すとRotation_xを減少させる
        if (Input.GetKey(KeyCode.UpArrow))
        {
            currentRotation.x -= rotationRate * Time.deltaTime; // 100を掛けて調整
        }
        // 矢印キー下を押すとRotation_xを増加させる
        if (Input.GetKey(KeyCode.DownArrow))
        {
            currentRotation.x += rotationRate * Time.deltaTime; // 100を掛けて調整
        }

        // 回転角度を適用
        transform.localEulerAngles = currentRotation;
    }
}
