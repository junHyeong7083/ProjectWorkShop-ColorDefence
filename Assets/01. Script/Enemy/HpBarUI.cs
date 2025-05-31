using FischlWorks_FogWar;
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
       // bool visible = csFogWar.Instance.CheckVisibility(transform.position, 0);
       // hpBarCanvasGroup.alpha = visible ? 1f : 0f;
    }
    public void SetFill(float _amount)
    {
        if(fillImage != null)
            fillImage.fillAmount = _amount;
    }
}
