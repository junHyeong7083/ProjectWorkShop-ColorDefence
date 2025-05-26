using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.Image;

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

        float range = turret.GetRange();            // ���� �Ÿ�
        float rangeSqr = range * range;             // ���� �Ÿ�
        Vector3 origin = transform.position;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null || health.currentHp <= 0 || !enemy.activeInHierarchy)
                continue;

            float distSqr = (enemy.transform.position - origin).sqrMagnitude;

            float tileDist = Mathf.Sqrt(distSqr) / TileGridManager.Instance.cubeSize;
           // Debug.Log($"[Ÿ������ �Ÿ�] ����: {Mathf.Sqrt(distSqr):F2} / Ÿ��: {tileDist:F2} / ��Ÿ�: {range / TileGridManager.Instance.cubeSize:F2}ĭ");

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
        float rangeSqr = turret.GetRange()* turret.GetRange();
        Debug.Log($"Current Turret Range : {rangeSqr} ");
        Vector3 origin = transform.position;

        foreach (var enemy in enemies)
        {
            if (!IsValidTargetEnemy(enemy, origin, rangeSqr)) continue;

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

    private bool IsValidTargetEnemy(GameObject enemy, Vector3 origin, float rangeSq)
    {
        if (enemy == null || !enemy.activeInHierarchy)
            return false;
    
        var health = enemy.GetComponent<EnemyHealth>();
        if (health == null || health.currentHp <= 0) return false;

        float distSq = (enemy.transform.position - origin).sqrMagnitude;
        return distSq <= rangeSq;
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
