using UnityEngine;
using UnityEngine.UI;

public class OptionSound : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("SoundVolume", 0.5f);
        bgmSlider.value = savedVolume;

        bgmSlider.onValueChanged.AddListener (OnBGMVolumeChanged);
        SoundManager.Instance.UpdateBGMVolume();
    }

    private void OnBGMVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SoundVolume", value);
        PlayerPrefs.Save();

        SoundManager.Instance.UpdateBGMVolume();
    }
}
