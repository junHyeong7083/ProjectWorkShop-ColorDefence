using UnityEngine;
using UnityEngine.UI;

public class GradientSlider : MonoBehaviour
{
    public Image fillImage;         // Slider의 Fill Area > Fill 오브젝트의 Image
    public Gradient gradient;       // Inspector에서 그라데이션 직접 설정

    void UpdateColor(float value)
    {
        // 슬라이더의 value는 0~1, gradient.Evaluate도 0~1
        if (fillImage != null && gradient != null)
        {
            fillImage.color = gradient.Evaluate(value);
        }
    }
}
