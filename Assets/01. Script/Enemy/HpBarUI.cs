using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    public Image fillImage;

    public void SetFill(float _amount)
    {
        if(fillImage != null)
            fillImage.fillAmount = _amount;
    }
}
