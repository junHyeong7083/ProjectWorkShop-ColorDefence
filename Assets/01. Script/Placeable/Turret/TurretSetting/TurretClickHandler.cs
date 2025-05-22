using UnityEngine;

[RequireComponent(typeof(TurretBase))]
[RequireComponent(typeof(TurretRangeVisualizer))]
public class TurretClickHandler : MonoBehaviour
{
    // �ͷ� ��ü ����
    private TurretBase turret;
    // ��Ÿ� �ð�ȭ ����
    private TurretRangeVisualizer visualizer;

    private void Awake()
    {
        turret = GetComponent<TurretBase>();
        visualizer = GetComponent<TurretRangeVisualizer>();
    }

    // ���콺 Ŭ���� ȣ��
    private void OnMouseDown()
    {
        PlaceableUIManager.Instance.Select(turret);
        visualizer.Show(turret.GetRange());
    }


    // Ŭ�������� ȣ��
    private void OnMouseUp()
    {
        visualizer.Hide();
    }
}
