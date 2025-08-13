using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Tooltip("Max Frame Per Second")]
    public int targetFrameRate = 60;
    void Start()
    {
        QualitySettings.vSyncCount = 0;

        // 设置目标帧率
        Application.targetFrameRate = targetFrameRate;
    }
}
