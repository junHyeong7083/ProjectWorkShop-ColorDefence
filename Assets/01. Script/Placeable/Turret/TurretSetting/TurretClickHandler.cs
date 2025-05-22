using UnityEngine;

[RequireComponent(typeof(TurretBase))]
[RequireComponent(typeof(TurretRangeVisualizer))]
public class TurretClickHandler : MonoBehaviour
{
    // 터렛 본체 참조
    private TurretBase turret;
    // 사거리 시각화 참조
    private TurretRangeVisualizer visualizer;

    private void Awake()
    {
        turret = GetComponent<TurretBase>();
        visualizer = GetComponent<TurretRangeVisualizer>();
    }

    // 마우스 클릭시 호출
    private void OnMouseDown()
    {
        PlaceableUIManager.Instance.Select(turret);
        visualizer.Show(turret.GetRange());
    }


    // 클릭해제시 호출
    private void OnMouseUp()
    {
        visualizer.Hide();
    }
}
