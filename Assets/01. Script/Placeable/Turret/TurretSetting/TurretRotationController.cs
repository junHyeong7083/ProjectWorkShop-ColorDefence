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

    // 회전 대상 및 회전 속도 설정
    // - GatlingTurret 같은 파생 클래스가 Awake에서 호출함
    public void SetRotationTarget(Transform target, float speed)
    {
        headToRotate = target;
        rotateSpeed = speed;
    }

}
