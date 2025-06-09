using UnityEngine;
using System.Collections;

public class LaserShooter : MonoBehaviour, ITurretShooter
{
    private Transform firePoint;
    private Transform laserBeamObject; // Cylinder 오브젝트
    private TurretBase turret;

    private GameObject currentEnemy;

   // private float elapsed;
    private float tickElapsed;
    private bool isCoolingDown;
    private Coroutine checkRoutine;


    public bool IsReloading => false;
    private void Awake() => turret = GetComponent<TurretBase>();

    public void SetLaserReferences(Transform firePoint, Transform laserBeamObject)
    {
        this.firePoint = firePoint;
        this.laserBeamObject = laserBeamObject;

        laserBeamObject.gameObject.SetActive(false);
    }

    public void ShootAtEnemy(GameObject enemy)
    {
        if (isCoolingDown || enemy == null || !enemy.activeInHierarchy)
        {
            DisableLaser();
            return;
        }

        var health = enemy.GetComponent<BaseEnemy>();
        if (health == null || health.GetCurrentHp() <= 0)
        {
            DisableLaser();
            return;
        }

        Vector3 turretPos = turret.transform.position;
        Vector3 enemyPos = enemy.transform.position;
        float distSqr = (enemyPos - turretPos).sqrMagnitude;
        
        float range = turret.GetRange();
        float rangeSqr = range * range;

      //  Debug.Log($"Range : {rangeSqr} -- dist : {distSqr}");

        if (distSqr >= rangeSqr)
        {
            DisableLaser();
            return;
        }

        currentEnemy = enemy;
        tickElapsed += Time.deltaTime;

        ShowLaserToEnemy(enemy); //

        if (tickElapsed >= turret.turretData.laserTickInterval)
        {
           // elapsed += tickElapsed;
            health.TakeDamage(turret.GetDamage());

            EffectManager.Instance.PlayEffect(
              "Laser_Enemy_HitFX",
                enemy.transform.position
            );

            tickElapsed = 0f;
        }

        if (health.GetCurrentHp() <= 0)
        {
            DisableLaser();   // 
            currentEnemy = null;
            return;
        }

       /* if (elapsed >= turret.turretData.laserDuration)
        {
            DisableLaser();
            StartCoroutine(CooldownRoutine());
        }*/

        if (checkRoutine == null)
            checkRoutine = StartCoroutine(CheckLaserTarget());
    }


    private IEnumerator CheckLaserTarget()
    {
        while (currentEnemy != null)
        {
            var health = currentEnemy.GetComponent<BaseEnemy>();
            if (health == null || health.GetCurrentHp() <= 0 || !currentEnemy.activeInHierarchy)
            {
                DisableLaser();
                yield break;
            }

            yield return null;
        }
    }

    private void ShowLaserToEnemy(GameObject enemy)
    {
        Vector3 start = firePoint.position;
        Vector3 end = enemy.transform.position + Vector3.up * 1.0f;

        Vector3 dir = end - start;
        Vector3 magoffset = new Vector3(dir.x,0, dir.z);
        float length = magoffset.magnitude;

        laserBeamObject.gameObject.SetActive(true);
     //   Debug.Log("laserBeamObj : " + laserBeamObject.gameObject);
        laserBeamObject.position = start;
        laserBeamObject.rotation = Quaternion.LookRotation(dir);
        laserBeamObject.localScale = new Vector3(0.3f, 0.3f, length * 0.15f);
    }


    public void DisableLaser()
    {
        if (laserBeamObject != null)
            laserBeamObject.gameObject.SetActive(false);

        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
    }

}
