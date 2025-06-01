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
    public Text nameText;         // 카드 이름을 표시할 Text
    public Text costText;         // 카드 비용을 표시할 Text
    public Text descriptionText;  // 카드 설명을 표시할 Text
    public Text starText;
    public Text damageText;
    public Text attackRateText;
    public Text attackRangeText;

    /// <summary>
    /// cardData를 기반으로 모든 UI(아이콘, 이름, 비용, 설명)를 업데이트합니다.
    /// </summary>
    public void Bind()
    {
        if (cardData == null)
        {
            // cardData가 할당되지 않았으면, UI를 모두 초기화(비움)
            if (iconImage != null) iconImage.enabled = false;
            if (nameText != null) nameText.text = "";
            if (costText != null) costText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            return;
        }

        // 1) 카드 아이콘 세팅
        if (iconImage != null)
        {
            iconImage.sprite = cardData.cardIcon;
            iconImage.enabled = (cardData.cardIcon != null);
        }

        // 2) 카드 이름 세팅
        string displayName = cardData.scriptable != null
            ? cardData.cardName
            : cardData.cardName;

        if (nameText != null)
            nameText.text = "Name : " +displayName;

        // 3) 카드 비용(cost) 세팅
        if (costText != null)
        {
            // a) TurretData 타입인지 검사
            if (cardData.scriptable is TurretData turretData)
            {
                costText.text = "Cost : " + turretData.placementCost.ToString();
            }
            // b) FenceData 타입인지 검사
            else if (cardData.scriptable is FenceData fenceData)
            {
                costText.text = "Cost : " + fenceData.placementCost.ToString();
            }
            // c) 그 외 일반 CardData는 cardData.cost 사용
            else
            {
                costText.text = "Cost : " + cardData.cost.ToString();
            }
        }

        // 4) 카드 설명(description) 세팅
        if (descriptionText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData가 public string description; 같은 필드를 넣어 두었다면
                descriptionText.text = "Descrition : " + tData.description;
            }
            else if (cardData.scriptable is FenceData fData)
            {
                descriptionText.text = "Descrition : \n" + fData.description;
            }
            else
            {
                descriptionText.text = "Descrition : " + ""; // 일반 카드라면 설명이 없다면 빈 문자열
            }
        }

        if (damageText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData가 public string description; 같은 필드를 넣어 두었다면
                damageText.text = "Damage : " + tData.baseDamage.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                damageText.text = "";
            }
            else
            {
                descriptionText.text = ""; // 일반 카드라면 설명이 없다면 빈 문자열
            }
        }

        if (attackRateText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData가 public string description; 같은 필드를 넣어 두었다면
                attackRateText.text = "Rate : " + tData.baseAttackRate.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                attackRateText.text = "";
            }
            else
            {
                attackRateText.text = ""; // 일반 카드라면 설명이 없다면 빈 문자열
            }
        }

        if (attackRangeText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData가 public string description; 같은 필드를 넣어 두었다면
                attackRangeText.text = "Range : " + tData.baseAttackRange.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                attackRangeText.text = "";
            }
            else
            {
                attackRangeText.text = ""; // 일반 카드라면 설명이 없다면 빈 문자열
            }
        }
        if (starText != null)
        {
            if (cardData.scriptable is TurretData tData)
            {
                // TurretData가 public string description; 같은 필드를 넣어 두었다면
                starText.text = "Star : " + tData.Star.ToString();
            }
            else if (cardData.scriptable is FenceData fData)
            {
                starText.text = "";
            }
            else
            {
                starText.text = ""; // 일반 카드라면 설명이 없다면 빈 문자열
            }
        }
    }
}
