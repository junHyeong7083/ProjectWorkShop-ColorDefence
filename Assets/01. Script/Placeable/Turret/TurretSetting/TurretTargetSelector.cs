using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TurretBase))]
public class TurretTargetSelector : MonoBehaviour
{
    private TurretBase turret;

    private void Awake()
    {
        turret = GetComponent<TurretBase>();
    }

    // 가장 가까운 적 찾기
    public GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float bestScore = float.MaxValue;
        float rangeSqr = turret.GetRange() * turret.GetRange();
        Vector3 origin = transform.position;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null || health.currentHp <= 0 || !enemy.activeInHierarchy)
                continue;

            float distSqr = (enemy.transform.position - origin).sqrMagnitude;
            if (distSqr > rangeSqr) continue;

            if (distSqr < bestScore)
            {
                bestScore = distSqr;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    public GameObject FindClosestEnemy(GameObject exclude)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float bestScore = float.MaxValue;
        float rangeSqr = turret.GetRange() * turret.GetRange();
        Vector3 origin = transform.position;

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy == exclude) continue;


            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null || health.currentHp <= 0 || !enemy.activeInHierarchy)
                continue;

            float distSqr = (enemy.transform.position - origin).sqrMagnitude;
            if (distSqr > rangeSqr) continue;

            if (distSqr < bestScore)
            {
                bestScore = distSqr;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }


    // 가장 오래된 적 점령 타일 탐색
    public Tile FindBestEnemyTile()
    {
        float range = turret.GetRange();
        Vector3 center = transform.position;

        Tile bestTile = null;
        float bestScore = float.MaxValue;

        var tiles = TileGridManager.Instance.GetTilesInRange(center, range);
        foreach (var tile in tiles)
        {
            if (!IsValidTargetTile(tile)) continue;

            float score = EvaluateTileScore(tile);
            if (score < bestScore)
            {
                bestScore = score;
                bestTile = tile;
            }
        }

        if (bestTile != null)
        {
            bestTile.TargetingTurret = turret;
            bestTile.Reserve();
        }

        return bestTile;
    }


    // 타겟 가능한 Enemy 타일인지 검사
    private bool IsValidTargetTile(Tile tile)
    {
        if (tile.ColorState != TileColorState.Enemy) return false;
        if (tile.IsBumping || tile.IsReserved) return false;
        if (tile.TargetingTurret != null && tile.TargetingTurret != turret) return false;

        return true;
    }

    // 타일 우선순위 평가 (더 오래 점령된 타일일수록 점수 낮음 = 우선)
    private float EvaluateTileScore(Tile tile)
    {
        return -(Time.time - tile.LastChangedTime);
    }
}
