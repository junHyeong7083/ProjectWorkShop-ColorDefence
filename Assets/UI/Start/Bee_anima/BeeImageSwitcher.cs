using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BeeImageSwitcher : MonoBehaviour
{
    public Sprite[] sprites;     // 벌 이미지 2개를 Inspector에서 할당
    public float interval = 0.2f; // 교체 간격(초)

    private Image uiImage;
    private int index = 0;

    void Start()
    {
        uiImage = GetComponent<Image>();
        StartCoroutine(SwitchImage());
    }

    IEnumerator SwitchImage()
    {
        while (true)
        {
            uiImage.sprite = sprites[index];
            index = (index + 1) % sprites.Length;
            yield return new WaitForSeconds(interval);
        }
    }
}
