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

    // ���� ����� �� ã��
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


    // ���� ������ �� ���� Ÿ�� Ž��
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


    // Ÿ�� ������ Enemy Ÿ������ �˻�
    private bool IsValidTargetTile(Tile tile)
    {
        if (tile.ColorState != TileColorState.Enemy) return false;
        if (tile.IsBumping || tile.IsReserved) return false;
        if (tile.TargetingTurret != null && tile.TargetingTurret != turret) return false;

        return true;
    }

    // Ÿ�� �켱���� �� (�� ���� ���ɵ� Ÿ���ϼ��� ���� ���� = �켱)
    private float EvaluateTileScore(Tile tile)
    {
        return -(Time.time - tile.LastChangedTime);
    }
}
