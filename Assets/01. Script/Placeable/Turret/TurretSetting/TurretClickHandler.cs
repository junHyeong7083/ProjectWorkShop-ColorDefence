using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TurretBase))]
[RequireComponent(typeof(TurretRangeVisualizer))]
public class TurretClickHandler : MonoBehaviour
{
    // �ͷ� ��ü ����
    private TurretBase turret;
    // ��Ÿ� �ð�ȭ ����
    private TurretRangeVisualizer visualizer;

    bool isSelected = false;

    private void Awake()
    {
        turret = GetComponent<TurretBase>();
        visualizer = GetComponent<TurretRangeVisualizer>();
    }

    // ���콺 Ŭ���� ȣ��
    private void OnMouseDown()
    {
        PlaceableUIManager.Instance.Select(turret);

            //if (turret != null) turret.ShowSelectBox();
        
        // ��Ÿ� ����
        visualizer.Show(turret.GetRange());
    }

    // Ŭ�������� ȣ��
    private void OnMouseUp()
    {
       // if (turret != null)
          //  turret.HideSelectBox();
        visualizer.Hide();
    }
}
