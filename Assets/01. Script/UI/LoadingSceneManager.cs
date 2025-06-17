using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// LoadingSceneManager - �������Ʈ�� ���̰� ����Ͻø� �˴ϴ� 
public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene;

    // ������ ���� slider�����ø�˴ϴ�
    [SerializeField] Image progressBar;
    public Gradient gradient;       // Inspector���� �׶��̼� ���� ����

    void UpdateColor(float value)
    {
        // �����̴��� value�� 0~1, gradient.Evaluate�� 0~1
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

        // ���� ���Ӿ�
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


    /// -------------------------- ��뿹�� -------------------------- ///
    /*
     �ؿ� �ּ����� �ۼ��� StartButton()�� ���� ��ũ��Ʈ�� �״�� �ۼ�, ��ư�� �����ص��Ǵµ� ���� 
     ���ο� ��ũ��Ʈ�� ����� �۾��� �����ߴµ� ���ϽŴ�� �۾����ֽø�˴ϴ�!
     
     Ÿ��Ʋ ---> �ε��� ---> ���Ӿ�

    ������ �̷��ٰ� �������� ��
    Ÿ��Ʋ���� ���۹�ư�� ������ �ε��� -> ���Ӿ����� ����Ǿ���ϴµ�


    ( TestSceneLoad.cs)
    public void StartButton() => LoadingSceneManager.LoadScene("���Ӿ��̸�"); 
      
    ���� TestSceneLoad.cs�� �������Ʈ�� �ٿ��� �����ϰ�

    Ÿ��Ʋ�� ���۹�ư�� StartButton()�� �����Ͻø� �˴ϴ�.
   
     */

}
