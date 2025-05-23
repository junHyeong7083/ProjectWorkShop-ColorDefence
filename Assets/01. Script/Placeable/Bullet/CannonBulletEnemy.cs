using UnityEngine;
using System;

/// <summary>
/// 대포 포탄 객체. 포물선 형태로 이동하며, 도착 지점에서 3x3 AoE 피해를 입힌 후 풀로 반환된다.
/// 중심 타일은 1배, 주변 8타일은 0.5배 피해를 준다.
/// </summary>
public class CannonBulletEnemy : MonoBehaviour
{
    // 베지어 궤적용 시작점, 제어점, 도착점
    Vector3 startPoint, controlPoint, endPoint;

    float duration; // 포탄 비행 시간
    float elapsed;                   // 비행 경과 시간
    int damage;                      // 포탄의 원본 데미지
    Action onHit;                    // 도착 시 외부 콜백 (카메라쉐이크, 이펙트 등)

    // 이펙트용 타입 정보 (터렛 종류 및 행동 종류)
    TurretType turretType = TurretType.Cannon;
    TurretActionType actionType = TurretActionType.AttackEnemy;

    /// <summary>
    /// 포탄 초기화: 발사 시작점, 도착점, 베지어 높이, 비행시간, 데미지, 도착 시 콜백 등록
    /// </summary>
    public void Init(Vector3 start, Vector3 end, float height, float flightTime, int _damage, Action callback)
    {
        startPoint = start;
        endPoint = end;
        controlPoint = (start + end) / 2 + Vector3.up * height; // 포물선 제어점은 중간 + y축 높이
        duration = flightTime;
        elapsed = 0f;
        damage = _damage;
        onHit = callback;

        gameObject.SetActive(true);
    }

    void Update()
    {
        // 시간 증가
        elapsed += Time.deltaTime;

        // 진행률 0~1
        float t = Mathf.Clamp01(elapsed / duration);

        // 현재 위치 = 베지어 계산 결과
        transform.position = GetQuadraticBezierPoint(startPoint, controlPoint, endPoint, t);

        // 도착 시점
        if (t >= 1f)
        {
            ApplyAoEDamage(endPoint);                          // 3x3 범위 피해 처리
            onHit?.Invoke();                                   // 외부 콜백 (이펙트, 쉐이크 등)
            EffectManager.Instance.PlayEffect(turretType, actionType, endPoint); // 폭발 이펙트
            ReturnToPool();                                    // 풀 반환
        }
    }

    /// <summary>
    /// 2차 베지어 곡선 계산 함수 (t: 0~1)
    /// </summary>
    Vector3 GetQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        return Vector3.Lerp(a, b, t);
    }

    /// <summary>
    /// 중심 기준으로 3x3 범위 내 적에게 데미지를 입힘
    /// 중심 타일은 1배, 주변 8방향은 0.5배 데미지
    /// </summary>
    void ApplyAoEDamage(Vector3 center)
    {
        Debug.Log("ApplyAoEDamage 호출됨");

        float cubeSize = TileGridManager.Instance.cubeSize;
        float radius = cubeSize * 1.5f; // 3x3 커버 범위

        // Enemy 레이어에 해당하는 적들을 감지
        Collider[] hits = Physics.OverlapSphere(center, radius, LayerMask.GetMask("Enemy"));

        foreach (var col in hits)
        {
            var hp = col.GetComponent<EnemyHealth>();
            if (hp == null) continue;

            // 중심과의 거리 → 타일 단위 오프셋 계산
            Vector3 offset = col.transform.position - center;

            // 중심 여부 판정 (정확히 중심 타일 안인지)
            bool isCenter = Mathf.Abs(offset.x) < cubeSize * 0.5f && Mathf.Abs(offset.z) < cubeSize * 0.5f;

            float damageMultiplier = isCenter ? 1f : 0.5f;
            int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

            hp.TakeDamage(finalDamage);

            Debug.Log($"[AOE] {col.name} → {finalDamage} damage (center: {isCenter})");
        }
    }

    /// <summary>
    /// 풀링 시스템으로 반환
    /// </summary>
    void ReturnToPool()
    {
        gameObject.SetActive(false);
        BulletPool.Instance.Return(this);
    }
}


