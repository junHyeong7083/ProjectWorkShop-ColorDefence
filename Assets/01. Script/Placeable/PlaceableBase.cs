using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableBase : MonoBehaviour
{
    [HideInInspector]
    public List<Tile> occupiedTiles = new();

    public abstract void SetData(ScriptableObject data);

    public abstract void Sell();

    public virtual void Upgrade() => PlaceableUIManager.Instance.Select(this);

    protected virtual void OnMouseDown()
    {
        Debug.Log("fucking");
        PlaceableUIManager.Instance.Select(this);
    }
}
