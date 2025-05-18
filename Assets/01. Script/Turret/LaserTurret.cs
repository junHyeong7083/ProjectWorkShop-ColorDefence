using UnityEngine;
using System.Collections;

public class LaserTurret : TurretBase
{
    [SerializeField] private Transform firePoint;         // 레이저 시작 위치
    [SerializeField] private Transform headToRotate;       // 회전 대상
    [SerializeField] private LineRenderer lineRenderer;    // 레이저 시각화
    [SerializeField] private float rotateSpeed = 10f;      // 회전 속도

    private Tile targetTile;                               // 현재 타겟 타일
    private float elapsed = 0f;                            // 누적 시간 (레이저 유지 시간 계산용)
    private bool isCoolingDown = false;                    // 장전 시간 중인지 여부

    protected override IEnumerator PerformActionRoutine()
    {
        while (true)
        {
            // 장전 시간 중이면 아무것도 안 함
            if (isCoolingDown)
            {
                yield return null;
                continue;
            }

            if (turretData.actionType == TurretActionType.AttackTile)
            {
                // 타겟이 없거나, 타겟이 더 이상 유효하지 않다면 새로 찾음
                if (targetTile == null || !IsValidTarget(targetTile))
                {
                    targetTile = FindOldestEnemyTile();

                    if (targetTile == null)
                    {
                        lineRenderer.enabled = false;
                        yield return new WaitForSeconds(0.05f); // 잠깐 대기 후 재탐색
                        continue;
                    }

                    yield return RotateToTarget(targetTile.CenterWorldPos);

                    // 시각적으로 레이저 보여주기
                    FireLaserToTarget(targetTile);
                    // 틱 데미지 또는 색상 변경 처리
                    AttackTile(targetTile);
                }

            }
            else if (turretData.actionType == TurretActionType.AttackEnemy) ;

            yield return new WaitForSeconds(turretData.laserTickInterval);
        }
    }

    // 유효한 타겟인지 확인
    private bool IsValidTarget(Tile tile)
    {
        return tile != null && tile.ColorState == TileColorState.Enemy;
    }

    // 레이저 시각 효과
    private void FireLaserToTarget(Tile tile)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, tile.CenterWorldPos);
    }

    // 틱마다 타일에 데미지 또는 색상 변경 처리
    void AttackTile(Tile tile)
    {
        if (tile.ColorState == TileColorState.Enemy)
        {
            tile.SetColor(TileColorState.Player);
            tile.AnimateBump();

            EffectManager.Instance.PlayEffect(
                TurretType.Laser,
                TurretActionType.AttackTile,
                tile.CenterWorldPos
            );
        }

        // 누적 시간 증가
        elapsed += turretData.laserTickInterval;

        // 레이저 유지 시간 초과 시 타겟 초기화 및 장전 대기 시작
        if (elapsed >= turretData.laserDuration)
        {
            tile.Release();
            tile.TargetingTurret = null;
            elapsed = 0f;
            targetTile = null;
            lineRenderer.enabled = false;
            StartCoroutine(CooldownDelay());
        }
    }
    protected override void AttackTile() { /* 틱 구조에서는 사용 안 함 */ }
    // 장전 대기 코루틴
    private IEnumerator CooldownDelay()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(3f); // 고정 장전 시간
        isCoolingDown = false;
    }

    protected override void AttackEnemy() { /* 레이저는 Enemy 대상 아님 */ }

    // 타겟을 향해 부드럽게 회전
    private IEnumerator RotateToTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - headToRotate.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(headToRotate.rotation, targetRot) > 1f)
        {
            headToRotate.rotation = Quaternion.Slerp(
                headToRotate.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );

            yield return null;
        }
    }
}
