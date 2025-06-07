using UnityEditor;
using UnityEngine;

public class TestSceneLoad : MonoBehaviour
{
    public void StartButton() => LoadingSceneManager.LoadScene("GameScene");
    public void TitleButton() => LoadingSceneManager.LoadScene("Title");

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif


        Application.Quit();
    }
}
