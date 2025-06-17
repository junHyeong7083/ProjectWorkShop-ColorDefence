using UnityEngine;
using UnityEngine.UI;

public class ImagePopup : MonoBehaviour
{
    public GameObject popupPanel; // PopupPanel 오브젝트 연결
    public Image popupImage;      // PopupPanel 내의 Image 오브젝트 연결
    public Sprite imageToShow;    // 팝업에 띄울 이미지

    public void ShowPopup()
    {
        popupImage.sprite = imageToShow;
        popupPanel.SetActive(true);
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }
}
