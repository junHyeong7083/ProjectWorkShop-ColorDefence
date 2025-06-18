using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class NameManager : MonoBehaviour
{
    [SerializeField] private InputField inputField;

    public void RegisterPlayerName()
    {
        string _playerName = inputField.text;

        if (string.IsNullOrWhiteSpace(_playerName))
        {
            Debug.LogWarning("이름이 비어있습니다.");
            return;
        }

        // 저장 또는 처리 로직
        Debug.Log("플레이어 이름 등록됨: " + _playerName);

        // 예: PlayerPrefs에 저장
        PlayerPrefs.SetString("PlayerName", _playerName);
        PlayerPrefs.Save();

        SoundManager.Instance.PlaySFXSound("beep1", 0.3f);
        SceneManager.LoadScene(2);
    }
}
