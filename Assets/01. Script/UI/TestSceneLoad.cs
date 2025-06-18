using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    public void StartButton()
    {
        SoundManager.Instance.PlaySFXSound("beep1", 0.3f);
        SceneManager.LoadScene("NameScene");
    }

    public void ayncStartButton()
    {
        SoundManager.Instance.PlaySFXSound("beep2", 0.3f);
        LoadingSceneManager.LoadScene("GameScene");
    }
    public void TitleButton() 
    {
        SoundManager.Instance.PlaySFXSound("beep1", 0.3f);
        LoadingSceneManager.LoadScene("Title"); 
    }

    public void ExitButton()
    {

        Application.Quit();
    }
}
