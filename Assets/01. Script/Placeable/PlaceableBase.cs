using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableBase : MonoBehaviour
{
    [HideInInspector]
    public List<TileData> occupiedTiles = new(); 

    public abstract void SetData(ScriptableObject data);

    public abstract void Sell();

    public virtual void Upgrade() => PlaceableUIManager.Instance.Select(this);

    protected virtual void OnMouseDown()
    {
        Debug.Log("Pass");
        //PlaceableUIManager.Instance.Select(this);
    }
}
