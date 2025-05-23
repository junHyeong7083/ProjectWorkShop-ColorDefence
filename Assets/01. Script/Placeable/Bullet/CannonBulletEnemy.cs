using UnityEngine;
using System;

/// <summary>
/// 대포 포탄 오브젝트. 베지어 곡선 궤적으로 이동하며, 도착 지점에서 3x3 범위 피해를 입힌 후 풀로 반환됨.
/// 중심은 1배, 주변은 0.5배 피해.
/// </summary>
public class CannonBulletEnemy : MonoBehaviour
{
    Vector3 startPoint, controlPoint, endPoint;
    [SerializeField] float duration;
    float elapsed;
    int damage;
    Action onHit;

    TurretType turretType = TurretType.Cannon;
    TurretActionType actionType = TurretActionType.AttackEnemy;

    /// <summary>
    /// 포탄 발사 초기화
    /// </summary>
    public void Init(Vector3 start, Vector3 end, float height, float flightTime, int _damage, Action callback)
    {
        startPoint = start;
        endPoint = end;

        //  꼭짓점을 enemy 쪽으로 치우치게 설정 (85:15 비율)
        controlPoint = Vector3.Lerp(start, end, 0.95f) + Vector3.up * height;

        duration = flightTime;
        elapsed = 0f;
        damage = _damage;
        onHit = callback;

        gameObject.SetActive(true);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        //  속도 가속 적용: 초기 느림, 후반 빠름
        float curvedT = t * t;

        transform.position = GetQuadraticBezierPoint(startPoint, controlPoint, endPoint, curvedT);

        if (t >= 1f)
        {
            ApplyAoEDamage(endPoint);
            onHit?.Invoke();
            EffectManager.Instance.PlayEffect(turretType, actionType, endPoint);
            ReturnToPool();
        }
    }

    Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }

    /// <summary>
    /// 중심 기준으로 3x3 범위에 적에게 피해 적용
    /// 중심은 1배, 주변은 0.5배 (그리드 기반)
    /// </summary>
    void ApplyAoEDamage(Vector3 center)
    {
        Debug.Log("ApplyAoEDamage 호출됨");

        float cubeSize = TileGridManager.Instance.cubeSize;
        float radius = cubeSize * 1.5f;

        Collider[] hits = Physics.OverlapSphere(center, radius, LayerMask.GetMask("Enemy"));

        foreach (var col in hits)
        {
            var hp = col.GetComponent<EnemyHealth>();
            if (hp == null) continue;

            Vector3 offset = col.transform.position - center;
            bool isCenter = Mathf.Abs(offset.x) < cubeSize * 0.5f && Mathf.Abs(offset.z) < cubeSize * 0.5f;

            float damageMultiplier = isCenter ? 1f : 0.5f;
            int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

            hp.TakeDamage(finalDamage);

            Debug.Log($"[AOE] {col.name} → {finalDamage} damage (center: {isCenter})");
        }
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        BulletPool.Instance.Return(this);
    }

#if UNITY_EDITOR
    //  SceneView에서 궤적 시각화용 디버그 라인
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Vector3 prev = startPoint;
        int resolution = 20;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            float curvedT = t * t; // 디버깅도 가속 적용
            Vector3 point = GetQuadraticBezierPoint(startPoint, controlPoint, endPoint, curvedT);
            Gizmos.DrawLine(prev, point);
            prev = point;
        }

        // 꼭짓점 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(controlPoint, 0.1f);
    }
#endif
}
