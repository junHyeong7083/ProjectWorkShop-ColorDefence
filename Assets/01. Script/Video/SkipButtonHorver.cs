using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkipButtonHorver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Text text;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    int originfontSize;
    private void Start()
    {
        if (text != null)
            text.color = normalColor;

        originfontSize = text.fontSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text != null)
        {
            text.color = hoverColor;
            text.fontSize = originfontSize + 10;
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null)
        {
            text.color = normalColor;
            text.fontSize = originfontSize;
        }
            
    }
}
