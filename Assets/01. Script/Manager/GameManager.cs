using UnityEngine;
using UnityEngine.UI;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gold;
    public Text goldText;

    public GameSpeedMode CurrentSpeed { get; private set; } = GameSpeedMode.Normal;
    float currentTimeScaleMode = 1f;
    private void Awake()
    {
        instance = this;
        gold = 99999;
        goldText.text = gold.ToString();

        // 일단 시작값은 1
        SetSpeed(CurrentSpeed);
    }


    #region GameSpeed 조절 함수
    private void SetSpeed(GameSpeedMode mode)
    {
        CurrentSpeed = mode;
        Time.timeScale = GameSpeedUtility.GetTimeScale(mode);
        Debug.Log($"Speed changed to: {mode} / TimeScale: {Time.timeScale}");
    }


    public void IncreaseSpeed()
    {
        if (CurrentSpeed < GameSpeedMode.VeryFast)
        {
            CurrentSpeed++;
            SetSpeed(CurrentSpeed);
        }
    }

    public void DecreaseSpeed()
    {
        if (CurrentSpeed > GameSpeedMode.Normal)
        {
            CurrentSpeed--;
            SetSpeed(CurrentSpeed);
        }
    }

    #endregion



    #region Gold 판매 및 구매
    public bool SpendGold(int amount)
    {
        if (gold > amount)
        {
            gold -= amount;
            goldText.text = gold.ToString();
            return true;
        }
        return false;
    }

    public void AddGold(int amount) 
    {
        gold += amount;
        goldText.text = gold.ToString();
    }
    #endregion

    void Start()
    {
        #region CameraSetting 인게임 / UI 분리용
        var grid = TileGridManager.Instance;
        float w = grid.Width * grid.cubeSize;
        float h = grid.Height * grid.cubeSize;

        Camera.main.orthographic = true;
        Camera.main.orthographicSize = h * 0.5f;
        Camera.main.transform.position = new Vector3(w * 0.5f, 30f, h * 0.5f - 20f);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0f, 0f);

        Camera.main.rect = new Rect(0, 0.3f, 1, 0.7f);
        #endregion
    }
}
