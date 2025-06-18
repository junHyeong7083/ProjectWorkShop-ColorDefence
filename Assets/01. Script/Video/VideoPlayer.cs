using UnityEngine;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private UnityEngine.Video.VideoPlayer videoPlayer;
    [SerializeField] private Button skipButton;

    [SerializeField] private GameObject videoUIRoot;

    private void Start()
    {
        rawImage.raycastTarget = false;
        skipButton.onClick.AddListener(SkipVideo);

        videoPlayer.loopPointReached += OnVideoFinished;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PauseBGM();
        }

        videoPlayer.Play();
    }

    private void OnVideoFinished(UnityEngine.Video.VideoPlayer vp)
    {
        EndVideo();
    }

    private void SkipVideo()
    {
        videoPlayer.Stop();
        EndVideo();
    }

    private void EndVideo()
    {
        videoUIRoot.SetActive(false); // ���� UI ��ü ��Ȱ��ȭ
                                      // �� ��ȯ �Ǵ� ���� ���� ���� �߰� ó�� ����

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM();
        }
    }
}
