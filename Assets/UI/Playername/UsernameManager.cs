using UnityEngine;
using UnityEngine.UI;

public class UsernameManager : MonoBehaviour
{
   // public InputField usernameInput;
    public TMPro.TMP_InputField usernameInput;
// Inspector에서 InputField 연결
    public string userName;

    // 버튼 클릭 등으로 호출
    public void StoreUsername()
    {
        userName = usernameInput.text;
        Debug.Log("사용자 이름: " + userName);
    }
}
