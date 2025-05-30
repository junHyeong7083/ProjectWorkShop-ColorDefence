using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    public Image fillImage;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        transform.forward = mainCam.transform.forward;
    }
    public void SetFill(float _amount)
    {
        if(fillImage != null)
            fillImage.fillAmount = _amount;
    }
}
