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
            Debug.LogWarning("�̸��� ����ֽ��ϴ�.");
            return;
        }

        // ���� �Ǵ� ó�� ����
        Debug.Log("�÷��̾� �̸� ��ϵ�: " + _playerName);

        // ��: PlayerPrefs�� ����
        PlayerPrefs.SetString("PlayerName", _playerName);
        PlayerPrefs.Save();

        SoundManager.Instance.PlaySFXSound("beep1", 0.3f);
        SceneManager.LoadScene(2);
    }
}
