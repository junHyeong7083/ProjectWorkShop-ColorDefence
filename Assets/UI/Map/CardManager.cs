using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab; // 카드 프리팹
    public Transform cardContainer; // 카드 배치 부모 오브젝트
    public Sprite[] cardSprites; // 카드 이미지 배열

    private List<GameObject> activeCards = new List<GameObject>();

    void Start()
    {
        InitializeCards();
    }

    void InitializeCards()
    {
        // 카드 중 랜덤으로 6개 선택
        List<int> selectedIndices = new List<int>();

        while (selectedIndices.Count < 6)
        {
            int randomIndex = Random.Range(0, cardSprites.Length);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
            }
        }

        // 선택된 카드 배치
        for (int i = 0; i < selectedIndices.Count; i++)
        {
            GameObject card = Instantiate(cardPrefab, cardContainer);
            card.GetComponent<Image>().sprite = cardSprites[selectedIndices[i]];
            card.transform.localPosition = new Vector3(i * 110, -50, 0); // 하단에 카드 배치

            // 카드에 스크립트 추가
            CardMover cardMover = card.AddComponent<CardMover>();
            cardMover.Initialize(this);

            // 활성화된 카드에 추가
            activeCards.Add(card);
        }
    }

    public void ResetAllCards()
    {
        foreach (var card in activeCards)
        {
            card.transform.localPosition = new Vector3(card.transform.localPosition.x, -50, 0);
        }
    }
}
