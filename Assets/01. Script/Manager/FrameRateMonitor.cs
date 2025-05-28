using UnityEngine;
using UnityEngine.UI;
public class FrameRateMonitor : MonoBehaviour
{
    [Header("FPS Drop ���� ����")]
    
    public float fpsThreshold = 30f;     // �� ������ �������� ���
    public float checkInterval = 0.5f;   // üũ �ֱ� (��)

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
                Debug.LogWarning($"FPS Drop ������! ���� FPS: {fps:F1}");
                
            }


            // sibval?? 

            // �ʱ�ȭ
            frameCount = 0;
            timeSinceLastCheck = 0f;
        }
    }
}
