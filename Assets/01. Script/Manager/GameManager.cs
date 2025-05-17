using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int gold;
    public Text goldText;
    private void Awake()
    {
        instance = this;
        gold = 99999;
        goldText.text = gold.ToString();
    }

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


    void Start()
    {
        #region CameraSetting
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
