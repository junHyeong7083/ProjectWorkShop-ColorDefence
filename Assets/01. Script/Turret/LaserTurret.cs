using UnityEngine;
using System.Collections;

public class LaserTurret : TurretBase
{
    [SerializeField] private Transform firePoint;         // ������ ���� ��ġ
    [SerializeField] private Transform headToRotate;       // ȸ�� ���
    [SerializeField] private LineRenderer lineRenderer;    // ������ �ð�ȭ
    [SerializeField] private float rotateSpeed = 10f;      // ȸ�� �ӵ�

    private Tile targetTile;                               // ���� Ÿ�� Ÿ��
    private float elapsed = 0f;                            // ���� �ð� (������ ���� �ð� ����)
    private bool isCoolingDown = false;                    // ���� �ð� ������ ����

    protected override IEnumerator PerformActionRoutine()
    {
        while (true)
        {
            // ���� �ð� ���̸� �ƹ��͵� �� ��
            if (isCoolingDown)
            {
                yield return null;
                continue;
            }

            if (turretData.actionType == TurretActionType.AttackTile)
            {
                // Ÿ���� ���ų�, Ÿ���� �� �̻� ��ȿ���� �ʴٸ� ���� ã��
                if (targetTile == null || !IsValidTarget(targetTile))
                {
                    targetTile = FindOldestEnemyTile();

                    if (targetTile == null)
                    {
                        lineRenderer.enabled = false;
                        yield return new WaitForSeconds(0.05f); // ��� ��� �� ��Ž��
                        continue;
                    }

                    yield return RotateToTarget(targetTile.CenterWorldPos);

                    // �ð������� ������ �����ֱ�
                    FireLaserToTarget(targetTile);
                    // ƽ ������ �Ǵ� ���� ���� ó��
                    AttackTile(targetTile);
                }

            }
            else if (turretData.actionType == TurretActionType.AttackEnemy) ;

            yield return new WaitForSeconds(turretData.laserTickInterval);
        }
    }

    // ��ȿ�� Ÿ������ Ȯ��
    private bool IsValidTarget(Tile tile)
    {
        return tile != null && tile.ColorState == TileColorState.Enemy;
    }

    // ������ �ð� ȿ��
    private void FireLaserToTarget(Tile tile)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, tile.CenterWorldPos);
    }

    // ƽ���� Ÿ�Ͽ� ������ �Ǵ� ���� ���� ó��
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

        // ���� �ð� ����
        elapsed += turretData.laserTickInterval;

        // ������ ���� �ð� �ʰ� �� Ÿ�� �ʱ�ȭ �� ���� ��� ����
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
    protected override void AttackTile() { /* ƽ ���������� ��� �� �� */ }
    // ���� ��� �ڷ�ƾ
    private IEnumerator CooldownDelay()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(3f); // ���� ���� �ð�
        isCoolingDown = false;
    }

    protected override void AttackEnemy() { /* �������� Enemy ��� �ƴ� */ }

    // Ÿ���� ���� �ε巴�� ȸ��
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
