using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private RenderTexture minimapTexture;

    private void Start()
    {
        minimapImage.texture = minimapTexture;
    }
}
