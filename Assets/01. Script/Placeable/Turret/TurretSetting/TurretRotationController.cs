using System.Collections;
using UnityEngine;

public class TurretRotationController : MonoBehaviour
{
    private Transform headToRotate;
    private float rotateSpeed;

    // 특정 위치를 향해 터렛 머리를 회전시킨다
    // - targetPos: 회전할 위치
    // - Y축은 고정하고 수평 회전만 처리
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

    // 회전 대상 및 회전 속도 설정
    // - GatlingTurret 같은 파생 클래스가 Awake에서 호출함
    public void SetRotationTarget(Transform target, float speed)
    {
        headToRotate = target;
        rotateSpeed = speed;
    }

}
