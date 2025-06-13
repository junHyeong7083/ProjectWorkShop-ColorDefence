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
        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Vector3 targetEuler = targetRotation.eulerAngles;
       /* targetEuler.x = -25F;
        targetEuler.z = 0f;*/

        Quaternion targetRot = Quaternion.Euler(targetEuler);

        while (Quaternion.Angle(headToRotate.rotation, targetRot) > 1f)
        {
            headToRotate.rotation = Quaternion.Lerp(
                headToRotate.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );

            yield return null;
        }
        headToRotate.rotation = targetRot;
    }

    // ȸ�� ��� �� ȸ�� �ӵ� ����
    // - GatlingTurret ���� �Ļ� Ŭ������ Awake���� ȣ����
    public void SetRotationTarget(Transform target, float speed)
    {
        headToRotate = target;
        rotateSpeed = speed;
    }

}
