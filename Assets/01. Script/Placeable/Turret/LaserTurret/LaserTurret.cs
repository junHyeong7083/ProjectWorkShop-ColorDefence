using UnityEngine;

[RequireComponent(typeof(TurretCombatLoop))]
[RequireComponent(typeof(TurretRotationController))]
[RequireComponent(typeof(TurretTargetSelector))]
[RequireComponent(typeof(TurretClickHandler))]
[RequireComponent(typeof(LaserShooter))]
public class LaserTurret : TurretBase
{
    [Header("Laser Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform headToRotate;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float rotateSpeed = 10f;

    private void Awake()
    {
        var rotator = GetComponent<TurretRotationController>();
        rotator.SetRotationTarget(headToRotate, rotateSpeed);

        var shooter = GetComponent<LaserShooter>();
        shooter.SetLaserReferences(firePoint, lineRenderer);
    }

    //public Transform GetFirePoint() => firePoint;
}
