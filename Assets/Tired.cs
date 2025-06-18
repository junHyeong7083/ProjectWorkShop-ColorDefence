using UnityEngine;
using UnityEngine.UI;
public class Tired : MonoBehaviour
{
    [SerializeField] Text userNameText;

    private void Awake()
    {
        string name = PlayerPrefs.GetString("PlayerName", "Unknown");
        userNameText.text = $"UserName : {name}";
    }
}
