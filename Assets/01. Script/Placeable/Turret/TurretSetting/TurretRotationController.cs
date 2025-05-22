using System.Collections;
using UnityEngine;

public class TurretRotationController : MonoBehaviour
{
    private Transform headToRotate;
    private float rotateSpeed;

    // Ư�� ��ġ�� ���� �ͷ� �Ӹ��� ȸ����Ų��
    // - targetPos: ȸ���� ��ġ
    // - Y���� �����ϰ� ���� ȸ���� ó��
    public IEnumerator RotateTo(Vector3 targetPos)
    {
        if (headToRotate == null)
            yield break;

        Vector3 direction = targetPos - headToRotate.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(headToRotate.rotation, targetRotation) > 1f)
        {
            headToRotate.rotation = Quaternion.Slerp(
                headToRotate.rotation,
                targetRotation,
                Time.deltaTime * rotateSpeed
            );

            yield return null;
        }
    }

    // ȸ�� ��� �� ȸ�� �ӵ� ����
    // - GatlingTurret ���� �Ļ� Ŭ������ Awake���� ȣ����
    public void SetRotationTarget(Transform target, float speed)
    {
        headToRotate = target;
        rotateSpeed = speed;
    }

}
