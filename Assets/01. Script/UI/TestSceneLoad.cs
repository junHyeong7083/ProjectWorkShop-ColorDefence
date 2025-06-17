using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    public void StartButton() => SceneManager.LoadScene("NameScene");

      public void ayncStartButton() => LoadingSceneManager.LoadScene("GameScene");
    public void TitleButton() => LoadingSceneManager.LoadScene("Title");

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif


        Application.Quit();
    }
}
