using UnityEngine;
using UnityEngine.UI;
public class FrameRateMonitor : MonoBehaviour
{
    [Header("FPS Drop 감지 설정")]
    
    public float fpsThreshold = 30f;     // 이 값보다 낮아지면 경고
    public float checkInterval = 0.5f;   // 체크 주기 (초)

    private float timeSinceLastCheck = 0f;
    private int frameCount = 0;

    [SerializeField] Text fpsText; 

    void Update()
    {
        frameCount++;
        timeSinceLastCheck += Time.unscaledDeltaTime;

        if (timeSinceLastCheck >= checkInterval)
        {
            float fps = frameCount / timeSinceLastCheck;
            fpsText.text = fps.ToString("F2");
            if (fps < fpsThreshold)
            {
                Debug.LogWarning($"FPS Drop 감지됨! 현재 FPS: {fps:F1}");
                
            }


            // sibval?? 

            // 초기화
            frameCount = 0;
            timeSinceLastCheck = 0f;
        }
    }
}
