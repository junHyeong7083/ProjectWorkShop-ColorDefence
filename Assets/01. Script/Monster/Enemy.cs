using UnityEngine;
using UnityEngine.UI;
public enum Direction
{
    Front, 
    Back,
    Right,
    Left
}



public class Enemy : MonoBehaviour
{
    public MonsterData Data;

    int currentHp;
    public Direction moveDirection = Direction.Front;
    Vector3 direction;


    [SerializeField] GameObject hpBarPrefab;
    GameObject hpBarInstance;
    Image hpBarFillImage;

    public GameObject OverlayPrefab;

    void Awake()
    {
        currentHp = Data.MaxHp;
        direction = GetDirectionVector(moveDirection);
    }

    private void Start()
    {
        Vector3 pos = transform.position;
        pos.y = TileGridManager.Instance.cubeSize; // 3d환경이다보니 기본값으로하면 캐릭터가 큐브에 가려짐
        transform.position = pos;

        //ShowOverlayAtSpawm();
        CreateHpBar();
    }
    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        if (currentHp <= 0)
        {
            Destroy(gameObject);
        }
    }

    void CreateHpBar()
    {
        if (hpBarPrefab == null) return;
        hpBarInstance = Instantiate(hpBarPrefab, this.transform);
        hpBarInstance.transform.localPosition = new Vector3(0, 2.5f, 0);

        hpBarFillImage = hpBarInstance.GetComponentInChildren<Image>();
        hpBarFillImage.fillAmount = 1f;

        Debug.Log(hpBarFillImage.fillAmount);
    }


    private void Update()
    {
        /// test
        Move();
        InfectTile();
    }

    void ShowOverlayAtSpawm()
    {
        int halfW = Data.Width / 2;
        int halfH = Data.Height / 2;


        for (int x = -halfW; x <= halfW; x++)
        {
            for (int z = -halfH; z <= halfH; z++)
            {
                GameObject overlay = Instantiate(OverlayPrefab, transform); // 부모 먼저 지정
                overlay.transform.localPosition = new Vector3(x, -0.5f, z);
            }
        }
    }

    void Move()
    {
        transform.Translate(direction * Data.Speed * Time.deltaTime);
    }


    void InfectTile()
    {
        Vector3 pos = this.transform.position;
        int centerX = Mathf.FloorToInt(pos.x / TileGridManager.Instance.cubeSize);
        int centerZ = Mathf.FloorToInt(pos.z / TileGridManager.Instance.cubeSize);

        int halfWidth = Data.Width / 2;
        int halfHeight = Data.Height / 2;

        for (int dx = -halfWidth; dx <= halfWidth; dx++)
        {
            for (int dz = -halfHeight; dz <= halfHeight; dz++)
            {
                int x = centerX + dx;
                int z = centerZ + dz;

                var tile = TileGridManager.Instance.GetTile(x, z);
                if (tile != null && tile.ColorState != Data.InfectColor)
                    tile.SetColor(Data.InfectColor);
            }
        }
    }

    Vector3 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Front: return Vector3.forward;
            case Direction.Back: return Vector3.back;
            case Direction.Left: return Vector3.left;
            case Direction.Right: return Vector3.right;
            default: return Vector3.forward;
        }
    }
}
