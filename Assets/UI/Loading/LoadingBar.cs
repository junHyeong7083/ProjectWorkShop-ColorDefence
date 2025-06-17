using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingBar : MonoBehaviour
{
    public Slider slider;      // Inspector에서 연결
    public float duration = 2; // 채워지는 데 걸리는 시간(초)

    void Start()
    {
        StartCoroutine(FillSlider());
    }

    IEnumerator FillSlider()
    {
        float elapsed = 0f;
        slider.value = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        slider.value = 1f; // 마지막에 1로 고정
    }
}
