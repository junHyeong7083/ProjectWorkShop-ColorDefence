using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        var grid = TileGridManager.Instance;
        float w = grid.Width * grid.cubeSize;
        float h = grid.Height * grid.cubeSize;

        Camera.main.orthographic = true;
        Camera.main.orthographicSize = h * 0.5f;
        Camera.main.transform.position = new Vector3(w * 0.5f, 30f, h * 0.5f - 20f);
        Camera.main.transform.rotation = Quaternion.Euler(45, 0f, 0f);

         Camera.main.rect = new Rect(0, 0.3f, 1, 0.7f);
    }
}
