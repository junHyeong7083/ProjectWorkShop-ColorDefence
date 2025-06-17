using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardBinderUI : MonoBehaviour
{
    [Header("CardData (CardAnimationController에서 할당)")]
    [HideInInspector]
    public CardData cardData;    // 반드시 public 또는 [HideInInspector] public 으로 선언해야 외부에서 할당 가능

    [Header("UI 요소")]
    public Image iconImage;        // 카드 아이콘을 표시할 Image
  /*  public Text nameText;         // 카드 이름을 표시할 Text
    public Text costText;         // 카드 비용을 표시할 Text
    public Text descriptionText;  // 카드 설명을 표시할 Text
    public Text starText;
    public Text damageText;
    public Text attackRateText;
    public Text attackRangeText;*/

    

 /*   void ClearUI()
    {
        // cardData가 할당되지 않았으면, UI를 모두 초기화(비움)
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

        descriptionText.text = $"Description : 특정 타워를 강화하는 카드입니다.";
    }
*/

    /// <summary>
    /// cardData를 기반으로 모든 UI(아이콘, 이름, 비용, 설명)를 업데이트합니다.
    /// </summary>
    public void Bind()
    {
        if (cardData == null)
        {
         //   ClearUI();
            return;
        }

        // 공통 처리
        iconImage.sprite = cardData.cardIcon;
        iconImage.enabled = (cardData.cardIcon != null);
      //  nameText.text = $"Name : {cardData.cardName}";
     //   costText.text = $"Cost : {cardData.cost}";

     /*   // 카드 타입에 따라 처리 분기
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
