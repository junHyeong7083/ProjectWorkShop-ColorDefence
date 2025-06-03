using UnityEngine;

[RequireComponent(typeof(TurretCombatLoop))]
[RequireComponent(typeof(TurretRotationController))]
[RequireComponent(typeof(TurretTargetSelector))]
[RequireComponent(typeof(TurretClickHandler))]
[RequireComponent(typeof(CannonShooter))]
public class CannonTurret : TurretBase
{
    [Header("Cannon Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform headToRotate;
    [SerializeField] float rotateSpeed = 10f;

    
    private void Awake()
    {
        var rotator = GetComponent<TurretRotationController>(); 
        rotator.SetRotationTarget(headToRotate, rotateSpeed);
    }


    public Transform GetFirePoint() => firePoint;
}
