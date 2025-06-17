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
  /*  public Text nameText;         // ī�� �̸��� ǥ���� Text
    public Text costText;         // ī�� ����� ǥ���� Text
    public Text descriptionText;  // ī�� ������ ǥ���� Text
    public Text starText;
    public Text damageText;
    public Text attackRateText;
    public Text attackRangeText;*/

    

 /*   void ClearUI()
    {
        // cardData�� �Ҵ���� �ʾ�����, UI�� ��� �ʱ�ȭ(���)
        if (iconImage != null) iconImage.enabled = false;
        if (nameText != null) nameText.text = "";
        if (costText != null) costText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }
    void BindTurretCard()
    {
        if (cardData.scriptable is TurretData tData)
        {
            starText.text = $"Star : {tData.Star}";
            damageText.text = $"Damage : {tData.baseDamage}";
            attackRateText.text = $"Rate : {tData.baseAttackRate}";
            attackRangeText.text = $"Range : {tData.baseAttackRange}";
            descriptionText.text = $"Description : {tData.description}";
        }
    }
    void BindFenceCard()
    {
        if (cardData.scriptable is FenceData fData)
        {
            starText.text = "";
            damageText.text = "";
            attackRateText.text = "";
            attackRangeText.text = "";
            descriptionText.text = $"Description : {fData.description}";
        }
    }


    void BindUpgradeCard()
    {
        starText.text = "";
        damageText.text = "";
        attackRateText.text = "";
        attackRangeText.text = "";

        descriptionText.text = $"Description : Ư�� Ÿ���� ��ȭ�ϴ� ī���Դϴ�.";
    }
*/

    /// <summary>
    /// cardData�� ������� ��� UI(������, �̸�, ���, ����)�� ������Ʈ�մϴ�.
    /// </summary>
    public void Bind()
    {
        if (cardData == null)
        {
         //   ClearUI();
            return;
        }

        // ���� ó��
        iconImage.sprite = cardData.cardIcon;
        iconImage.enabled = (cardData.cardIcon != null);
      //  nameText.text = $"Name : {cardData.cardName}";
     //   costText.text = $"Cost : {cardData.cost}";

     /*   // ī�� Ÿ�Կ� ���� ó�� �б�
        switch (cardData.cardType)
        {
            case CardType.TURRET:
                BindTurretCard();
                break;
            case CardType.FENCE:
                BindFenceCard();
                break;
            case CardType.UPGRADE:
                BindUpgradeCard();
                break;
        }*/
    }
}
