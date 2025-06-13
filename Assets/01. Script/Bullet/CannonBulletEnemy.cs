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

    [SerializeField] float startEngler = -45f;
    [SerializeField] float endEngler = -135f;


    float elapsed;
    int damage;
    Action onHit;

    TurretType turretType = TurretType.Cannon;
    TurretActionType actionType = TurretActionType.AttackEnemy;

    /// <summary>
    /// 포탄 발사 초기화
    /// </summary>
    public void Init(Vector3 start, Vector3 end, float maxHeight, float flightTime, int _damage, Action callback)
    {
        startPoint = start;
        endPoint = end;

        float distance = Vector3.Distance(start, end);

        float minCurveDistance = 3f;
        float maxCurveDistance = 10f;

        float heightT = Mathf.InverseLerp(minCurveDistance, maxCurveDistance, distance);
        float finalHeight = Mathf.Lerp(0.1f, maxHeight, heightT); // 너무 낮으면 0.1f 정도

        Vector3 mid = (start + end) * 0.5f;
        controlPoint = mid + Vector3.up * (finalHeight * 0.3f);

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

        // 그냥 t 사용 (너무 왜곡된 t*t 안 씀)
        Vector3 pos = GetQuadraticBezierPoint(startPoint, controlPoint, endPoint, t);
        transform.position = pos;

        // 궤도 따라 회전
        Vector3 next = GetQuadraticBezierPoint(startPoint, controlPoint, endPoint, Mathf.Min(1f, t + 0.01f));
        Vector3 dir = next - pos;
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        if (t >= 1f)
        {
            ApplyAoEDamage(endPoint);
            onHit?.Invoke();
            EffectManager.Instance.PlayEffect("BigExplosion", endPoint);
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

        float cubeSize = TileGridManager.Instance.cubeSize;
        float radius = cubeSize * 2;

        Collider[] hits = Physics.OverlapSphere(center, radius, LayerMask.GetMask("Enemy"));

        foreach (var col in hits)
        {
            var hp = col.GetComponent<BaseEnemy>();
            if (hp == null) continue;

            Vector3 offset = col.transform.position - center;
            bool isCenter = Mathf.Abs(offset.x) < cubeSize * 0.5f && Mathf.Abs(offset.z) < cubeSize * 0.5f;

            float damageMultiplier = isCenter ? 1f : 0.5f;
            int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);
           // Debug.Log("finalDamage : " + finalDamage);
            hp.TakeDamage(finalDamage);

           // Debug.Log($"[AOE] {col.name} → {finalDamage} damage (center: {isCenter})");
        }
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        BulletPool.Instance.Return(this);
    }

#if UNITY_EDITOR
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

        float radius = TileGridManager.Instance.cubeSize * 2;
        Gizmos.DrawWireSphere(endPoint, radius); // 대포 폭발 범위 확인용
    }
#endif
}
