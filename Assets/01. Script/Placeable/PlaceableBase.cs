using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableBase : MonoBehaviour
{
    [HideInInspector]
    public List<Tile> occupiedTiles = new();

    public abstract void SetData(ScriptableObject data);

    public abstract void Sell();

    public abstract void Upgrade();

    protected virtual void OnMouseDown()
    {
        Debug.Log("fucking");
        PlaceableUIManager.Instance.Select(this);
    }
}
