using UnityEngine;
using System.Collections;
using UnityEditor.Rendering;
public class LaserShooter : MonoBehaviour, ITurretShooter
{
    private Transform firePoint;
    private LineRenderer lineRenderer;
    private TurretBase turret;

    private GameObject currentEnemy;
    private Tile currentTile;

    private float elapsed;
    private float tickElapsed;
    private bool isCoolingDown;

    private void Awake() => turret = GetComponent<TurretBase>();

    public void SetLaserReferences(Transform firePoint, LineRenderer lineRenderer)
    {
        this.firePoint = firePoint;
        this.lineRenderer = lineRenderer;

        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.widthCurve = AnimationCurve.Linear(0, 1, 1, 1);
        lineRenderer.enabled = false;
    }

    public void ShootAtEnemy(GameObject enemy)
    {
        if (isCoolingDown || enemy == null)
        {
            DisableLaser();
            return;
        }

        var health = enemy.GetComponent<EnemyHealth>();



        currentEnemy = enemy;
        elapsed += Time.deltaTime;
        tickElapsed += Time.deltaTime;

        ShowLaserToEnemy(enemy);

        if (tickElapsed >= turret.turretData.laserTickInterval)
        {
            int checking = health.currentHp -= turret.GetDamage();
            health.TakeDamage(turret.GetDamage());
            if (checking <= 0)
                DisableLaser();
            EffectManager.Instance.PlayEffect(
                turret.turretData.turretType,
                TurretActionType.AttackEnemy,
                enemy.transform.position
            );

            tickElapsed = 0f;
        }

        if (elapsed >= turret.turretData.laserDuration)
        {
            DisableLaser();
            StartCoroutine(CooldownRoutine());
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
        elapsed += Time.deltaTime;
        tickElapsed += Time.deltaTime;
        ShowLaserToTile(tile);

        if (tickElapsed >= turret.turretData.laserTickInterval)
        {
            EffectManager.Instance.PlayEffect(turret.turretData.turretType, TurretActionType.AttackTile, tile.CenterWorldPos);
            tile.SetColor(TileColorState.Player);
            tile.AnimateBump();
            tile.TargetingTurret = null;
            tickElapsed = 0f;
        }

        if (elapsed >= turret.turretData.laserDuration)
        {
            tile.Release();
            tile.TargetingTurret = null;
            DisableLaser();
            StartCoroutine(CooldownRoutine());
        }
    }
    private void ShowLaserToEnemy(GameObject enemy)
    {
        Vector3 startPos = firePoint.position + Vector3.up * 2f;
        Vector3 endPos = enemy.transform.position + Vector3.up * 1.5f;

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
    private void ShowLaserToTile(Tile tile)
    {
        Vector3 startPos = firePoint.position + Vector3.up * 2f;
        Vector3 endPos = tile.CenterWorldPos + Vector3.up * 2f;

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    private void DisableLaser()
    {
        lineRenderer.enabled = false;
        elapsed = 0f;
        tickElapsed = 0f;
        currentTile = null;
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(3f);
        isCoolingDown = false;
    }
}
