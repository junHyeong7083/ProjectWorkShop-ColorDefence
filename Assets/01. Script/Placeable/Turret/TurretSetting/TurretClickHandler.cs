using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TurretBase))]
[RequireComponent(typeof(TurretRangeVisualizer))]
public class TurretClickHandler : MonoBehaviour
{
    // 터렛 본체 참조
    private TurretBase turret;
    // 사거리 시각화 참조
    private TurretRangeVisualizer visualizer;

    bool isSelected = false;

    private void Awake()
    {
        turret = GetComponent<TurretBase>();
        visualizer = GetComponent<TurretRangeVisualizer>();
    }

    // 마우스 클릭시 호출
    private void OnMouseDown()
    {
        PlaceableUIManager.Instance.Select(turret);

            //if (turret != null) turret.ShowSelectBox();
        
        // 사거리 들어가고
        visualizer.Show(turret.GetRange());
    }

    // 클릭해제시 호출
    private void OnMouseUp()
    {
       // if (turret != null)
          //  turret.HideSelectBox();
        visualizer.Hide();
    }
}
