using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// LoadingSceneManager - 빈오브젝트에 붙이고 사용하시면 됩니다 
public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    // 기존에 만든 slider넣으시면됩니다
    [SerializeField] Image progressBar;
    public Gradient gradient;       // Inspector에서 그라데이션 직접 설정

    void UpdateColor(float value)
    {
        // 슬라이더의 value는 0~1, gradient.Evaluate도 0~1
        if (progressBar != null && gradient != null)
        {
            progressBar.color = gradient.Evaluate(value);
        }
    }
    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadScene()
    {
        yield return null;

        // 실제 게임씬
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        float timer = 0.0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
                UpdateColor(progressBar.fillAmount);
                if (progressBar.fillAmount >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);

                UpdateColor(progressBar.fillAmount);
                if (progressBar.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }


    /// -------------------------- 사용예제 -------------------------- ///
    /*
     밑에 주석으로 작성한 StartButton()를 현재 스크립트에 그대로 작성, 버튼에 연결해도되는데 저는 
     새로운 스크립트를 만들고 작업을 진행했는데 편하신대로 작업해주시면됩니다!
     
     타이틀 ---> 로딩씬 ---> 게임씬

    구성이 이렇다고 가정했을 때
    타이틀에서 시작버튼을 누르면 로딩씬 -> 게임씬으로 진행되어야하는데


    ( TestSceneLoad.cs)
    public void StartButton() => LoadingSceneManager.LoadScene("게임씬이름"); 
      
    현재 TestSceneLoad.cs를 빈오브젝트에 붙였다 가정하고

    타이틀의 시작버튼에 StartButton()를 참조하시면 됩니다.
   
     */

}
