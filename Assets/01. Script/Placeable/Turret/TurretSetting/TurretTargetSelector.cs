using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(TurretBase))]
public class TurretTargetSelector : MonoBehaviour
{
    private TurretBase turret;
    public bool IsEnemyInRange { get; private set; }
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

        float range = turret.GetRange();
        float rangeSqr = range * range;
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

        IsEnemyInRange = bestTarget != null;
        return bestTarget;
    }
  /*  public GameObject FindClosestEnemy(GameObject exclude)
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
    }*/

  
}
