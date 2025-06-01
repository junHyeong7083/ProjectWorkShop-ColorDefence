using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardBinderUI : MonoBehaviour
{
    [Header("CardData (CardAnimationController���� �Ҵ�)")]
    [HideInInspector]
    public CardData cardData;    // �ݵ�� public �Ǵ� [HideInInspector] public ���� �����ؾ� �ܺο��� �Ҵ� ����

    [Header("UI ���")]
    public Image iconImage;        // ī�� �������� ǥ���� Image
    public Text nameText;         // ī�� �̸��� ǥ���� Text
    public Text costText;         // ī�� ����� ǥ���� Text
    public Text descriptionText;  // ī�� ������ ǥ���� Text
    public Text starText;
    public Text damageText;
    public Text attackRateText;
    public Text attackRangeText;

    /// <summary>
    /// cardData�� ������� ��� UI(������, �̸�, ���, ����)�� ������Ʈ�մϴ�.
    /// </summary>
    public void Bind()
    {
        if (cardData == null)
        {
            // cardData�� �Ҵ���� �ʾ�����, UI�� ��� �ʱ�ȭ(���)
            if (iconImage != null) iconImage.enabled = false;
            if (nameText != null) nameText.text = "";
            if (costText != null) costText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            return;
        }

        // 1) ī�� ������ ����
        if (iconImage != null)
        {
            iconImage.sprite = cardData.cardIcon;
            iconImage.enabled = (cardData.cardIcon != null);
        }

        // 2) ī�� �̸� ����
        string displayName = cardData.scriptable != null
            ? cardData.cardName
            : cardData.cardName;

        if (nameText != null)
            nameText.text = "Name : " +displayName;

        // 3) ī�� ���(cost) ����
        if (costText != null)
        {
            // a) TurretData Ÿ������ �˻�
            if (cardData.scriptable is TurretData turretData)
            {
                costText.text = "Cost : " + turretData.placementCost.ToString();
            }
            // b) FenceData Ÿ������ �˻�
            else if (cardData.scriptable is FenceData fenceData)
            {
                costText.text = "Cost : " + fenceData.placementCost.ToString();
            }
            // c) �� �� �Ϲ� CardData�� cardData.cost ���
            else
            {
                costText.text = "Cost : " + cardData.cost.ToString();
            }
        }

        // 4) ī�� ����(description) ����
        if (descriptionText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData�� public string description; ���� �ʵ带 �־� �ξ��ٸ�
                descriptionText.text = "Descrition : " + tData.description;
            }
            else if (cardData.scriptable is FenceData fData)
            {
                descriptionText.text = "Descrition : \n" + fData.description;
            }
            else
            {
                descriptionText.text = "Descrition : " + ""; // �Ϲ� ī���� ������ ���ٸ� �� ���ڿ�
            }
        }

        if (damageText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData�� public string description; ���� �ʵ带 �־� �ξ��ٸ�
                damageText.text = "Damage : " + tData.baseDamage.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                damageText.text = "";
            }
            else
            {
                descriptionText.text = ""; // �Ϲ� ī���� ������ ���ٸ� �� ���ڿ�
            }
        }

        if (attackRateText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData�� public string description; ���� �ʵ带 �־� �ξ��ٸ�
                attackRateText.text = "Rate : " + tData.baseAttackRate.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                attackRateText.text = "";
            }
            else
            {
                attackRateText.text = ""; // �Ϲ� ī���� ������ ���ٸ� �� ���ڿ�
            }
        }

        if (attackRangeText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData�� public string description; ���� �ʵ带 �־� �ξ��ٸ�
                attackRangeText.text = "Range : " + tData.baseAttackRange.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                attackRangeText.text = "";
            }
            else
            {
                attackRangeText.text = ""; // �Ϲ� ī���� ������ ���ٸ� �� ���ڿ�
            }
        }
        if (starText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData�� public string description; ���� �ʵ带 �־� �ξ��ٸ�
                starText.text = "Star : " + tData.Star.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                starText.text = "";
            }
            else
            {
                starText.text = ""; // �Ϲ� ī���� ������ ���ٸ� �� ���ڿ�
            }
        }
    }
}
