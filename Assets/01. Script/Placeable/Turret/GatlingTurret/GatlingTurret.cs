using UnityEngine;

[RequireComponent(typeof(TurretCombatLoop))]
[RequireComponent(typeof(TurretRotationController))]
[RequireComponent(typeof(TurretTargetSelector))]
[RequireComponent(typeof(TurretClickHandler))]
public class GatlingTurret : TurretBase
{
    [Header("Gatling Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform headToRotate;
    [SerializeField] private float rotateSpeed = 10f;

    private void Awake()
    {
        // 설정값을 회전 컴포넌트에 연결
        var rotator = GetComponent<TurretRotationController>();
        rotator.SetRotationTarget(headToRotate, rotateSpeed);
    }

    public Transform GetFirePoint() => firePoint;
}
