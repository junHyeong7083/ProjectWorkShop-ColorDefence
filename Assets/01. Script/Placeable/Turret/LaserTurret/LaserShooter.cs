using UnityEngine;
using System.Collections;

public class LaserShooter : MonoBehaviour, ITurretShooter
{
    private Transform firePoint;
    private Transform laserBeamObject; // Cylinder 오브젝트
    private TurretBase turret;

    private GameObject currentEnemy;
    private Tile currentTile;

   // private float elapsed;
    private float tickElapsed;
    private bool isCoolingDown;
    private Coroutine checkRoutine;

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

        var health = enemy.GetComponent<EnemyHealth>();
        if (health == null || health.currentHp <= 0)
        {
            DisableLaser();
            return;
        }

        Vector3 turretPos = turret.transform.position;
        Vector3 enemyPos = enemy.transform.position;
        float distSqr = (enemyPos - turretPos).sqrMagnitude;
        
        float range = turret.GetRange();
        float rangeSqr = range * range;

        Debug.Log($"Range : {rangeSqr} -- dist : {distSqr}");

        if (distSqr >= rangeSqr)
        {
            Debug.Log("sibal");
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
                turret.turretData.turretType,
                TurretActionType.AttackEnemy,
                enemy.transform.position
            );

            tickElapsed = 0f;
        }

        if (health.currentHp <= 0)
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
            var health = currentEnemy.GetComponent<EnemyHealth>();
            if (health == null || health.currentHp <= 0 || !currentEnemy.activeInHierarchy)
            {
                DisableLaser();
                yield break;
            }

            yield return null;
        }
    }

    public void ShootAtTile(Tile tile)
    {
        if (isCoolingDown || tile == null || tile.ColorState != TileColorState.Enemy)
        {
            DisableLaser();
            return;
        }

        currentTile = tile;
       // elapsed += Time.deltaTime;
        tickElapsed += Time.deltaTime;
        ShowLaserToTile(tile);

        if (tickElapsed >= turret.turretData.laserTickInterval)
        {
            EffectManager.Instance.PlayEffect(
                turret.turretData.turretType,
                TurretActionType.AttackTile,
                tile.CenterWorldPos
            );

            tile.SetColor(TileColorState.Player);
            tile.AnimateBump();
            tile.TargetingTurret = null;
            tickElapsed = 0f;
        }

       /* if (elapsed >= turret.turretData.laserDuration)
        {
            tile.Release();
            tile.TargetingTurret = null;
            DisableLaser();
            StartCoroutine(CooldownRoutine());
        }*/
    }

    private void ShowLaserToEnemy(GameObject enemy)
    {
        Vector3 start = firePoint.position;
        Vector3 end = enemy.transform.position + Vector3.up * 1.0f;

        Vector3 dir = end - start;
        Vector3 magoffset = new Vector3(dir.x,0, dir.z);
        float length = magoffset.magnitude;

        laserBeamObject.gameObject.SetActive(true);
        laserBeamObject.position = start;
        laserBeamObject.rotation = Quaternion.LookRotation(dir);
        laserBeamObject.localScale = new Vector3(0.3f, 0.3f, length * 0.15f);
    }

    private void ShowLaserToTile(Tile tile)
    {
        // 1. 계산
        Vector3 start = firePoint.position;
        Vector3 end = tile.CenterWorldPos + Vector3.up * 1.0f;

        Vector3 dir = end - start;
        float length = dir.magnitude;

        // 2. 적용
        laserBeamObject.gameObject.SetActive(true);
        laserBeamObject.position = (start + end) * 0.5f; //  시작+끝 사이 중심점
        laserBeamObject.rotation = Quaternion.LookRotation(dir);
        laserBeamObject.localScale = new Vector3(0.2f, 0.2f, length / 2); //  길이 그대로
    }

    public void DisableLaser()
    {
        if (laserBeamObject != null)
            laserBeamObject.gameObject.SetActive(false);

        currentTile = null;

        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
    }

    GameObject chargingEffectInstance;
  /*  private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;

        // 1. 이펙트 활성화
        chargingEffectInstance = EffectManager.Instance.GetDynamicEffect(
            turret.turretData.turretType,
            TurretActionType.Both
        );

        float duration = 3f;
        float chargingTime = 0f;

        while (chargingTime < duration)
        {
            // 2. 실시간 위치 업데이트
            if (chargingEffectInstance != null)
                chargingEffectInstance.transform.position = new Vector3( firePoint.position.x, firePoint.position.y, firePoint.position.z + 1f);

            chargingTime += Time.deltaTime;
            yield return null;
        }

        // 3. 이펙트 반환 및 초기화
        EffectManager.Instance.ReturnDynamicEffect(
            turret.turretData.turretType,
            TurretActionType.Both,
            chargingEffectInstance
        );
        chargingEffectInstance = null;

       // elapsed = 0f;
        tickElapsed = 0f;
        isCoolingDown = false;
    }*/
}
