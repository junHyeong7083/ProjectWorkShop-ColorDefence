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


    private void Start()
    {
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.widthCurve = AnimationCurve.Linear(0, 1, 1, 1);
    }
    float isTickTime = 0;
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

            // AttackTile 터렛일때
            if (turretData.actionType == TurretActionType.AttackTile)
            {
                // 타게팅 된 타일이 존재하지않을때
                if (targetTile == null || !IsValidTarget(targetTile))
                {
                    // 타일찾기
                    targetTile = FindOldestEnemyTile();
                   // Debug.Log("Tile Found..");
                    // 변수
                    elapsed = 0f;

                    if (targetTile == null)
                    {
                        lineRenderer.enabled = false;
                        yield return new WaitForSeconds(0.05f);
                        continue;
                    }

                    yield return RotateToTarget(targetTile.CenterWorldPos);
                }
               // Debug.Log("Tile Detect!");
                FireLaserToTarget(targetTile);
                AttackTile(targetTile);
            }

            else if (turretData.actionType == TurretActionType.AttackEnemy) { }

           // yield return new WaitForSeconds(turretData.laserTickInterval);
        }
    }
    private IEnumerator CooldownDelay()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(3f); // 고정 장전 시간
        isCoolingDown = false;
    }

    // 유효한 타겟인지 확인
    private bool IsValidTarget(Tile tile)
    {
        return tile != null && tile.ColorState == TileColorState.Enemy;
    }

    // 레이저 시각 효과
    private void FireLaserToTarget(Tile tile)
    {
        Vector3 startPos = firePoint.position + Vector3.up * 2f;
        Vector3 endPos = tile.CenterWorldPos + Vector3.up *2f;


        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }


    // 틱마다 타일에 데미지 또는 색상 변경 처리
    void AttackTile(Tile tile)
    {
        elapsed += Time.deltaTime; // 전체 공격시간 계산변수
        if (tile.ColorState == TileColorState.Enemy)
        {
            isTickTime += Time.deltaTime; // 틱을 계산할 변수
            if (isTickTime >= turretData.laserTickInterval) // 만약 틱시간이 attrate(공격까지 걸리는시간) 
            {
                // 이펙트 재생
                EffectManager.Instance.PlayEffect( TurretType.Laser, TurretActionType.AttackTile,tile.CenterWorldPos);

                // 타일 색 -> 플레이어로
                tile.SetColor(TileColorState.Player);
                // 타일의 애니메이션재생
                tile.AnimateBump();

                // 공격이 끝났으니 타게팅 터렛을 null로 비우고
                tile.TargetingTurret = null;
                targetTile = null;
                // 틱 값을 0으로
                isTickTime = 0;
            }
        }

        // 레이저 유지 시간 초과 시 타겟 초기화 및 장전 대기 시작
        if (elapsed >= turretData.laserDuration)
        {
            tile.Release();
            tile.TargetingTurret = null;

            elapsed = 0f;
            lineRenderer.enabled = false;
            StartCoroutine(CooldownDelay());
        }
    }
    protected override void AttackTile() { /* 틱 구조에서는 사용 안 함 */ }
    // 장전 대기 코루틴
 
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
