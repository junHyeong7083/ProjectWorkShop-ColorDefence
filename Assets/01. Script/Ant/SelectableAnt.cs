using UnityEngine;

public class SelectableAnt : MonoBehaviour
{
    public bool IsSelected { get; private set; }
    private Renderer rend;
    private Color originalColor;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
       // Debug.Log($"{gameObject.name} selected: {selected}");
        rend.material.color = selected ? Color.green : originalColor;
    }
}
