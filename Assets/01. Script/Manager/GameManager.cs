using UnityEngine;
using UnityEngine.UI;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gold;
    public Text goldText;

    public GameSpeedMode CurrentSpeed { get; private set; } = GameSpeedMode.Normal;
    private void Awake()
    {
        instance = this;
        gold = 99999;
        goldText.text = gold.ToString();

        // �ϴ� ���۰��� 1
        SetSpeed(CurrentSpeed);
    }


    #region GameSpeed ���� �Լ�
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



    #region Gold �Ǹ� �� ����
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
}
