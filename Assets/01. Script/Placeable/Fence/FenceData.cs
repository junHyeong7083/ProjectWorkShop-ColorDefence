using UnityEngine;

[CreateAssetMenu(menuName = "Game/FenceData")]
public class FenceData : ScriptableObject
{
    public int Width;
    public int Height;
    public int placementCost;

    public string description;
}
