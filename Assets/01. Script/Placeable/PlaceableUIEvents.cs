using UnityEngine;

public class PlaceableUIEvents : MonoBehaviour
{
    public void OnClickUpgrade()
    {
        PlaceableUIManager.Instance.RequestUpgrade();
    }

    public void OnClickSell()
    {
        PlaceableUIManager.Instance.RequestSell();
    }
}
