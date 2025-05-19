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
            // ���� �ð� ���̸� �ƹ��͵� �� ��
            if (isCoolingDown)
            {
                yield return null;
                continue;
            }

            // AttackTile �ͷ��϶�
            if (turretData.actionType == TurretActionType.AttackTile)
            {
                // Ÿ���� �� Ÿ���� ��������������
                if (targetTile == null || !IsValidTarget(targetTile))
                {
                    // Ÿ��ã��
                    targetTile = FindOldestEnemyTile();
                   // Debug.Log("Tile Found..");
                    // ����
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
        yield return new WaitForSeconds(3f); // ���� ���� �ð�
        isCoolingDown = false;
    }

    // ��ȿ�� Ÿ������ Ȯ��
    private bool IsValidTarget(Tile tile)
    {
        return tile != null && tile.ColorState == TileColorState.Enemy;
    }

    // ������ �ð� ȿ��
    private void FireLaserToTarget(Tile tile)
    {
        Vector3 startPos = firePoint.position + Vector3.up * 2f;
        Vector3 endPos = tile.CenterWorldPos + Vector3.up *2f;


        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }


    // ƽ���� Ÿ�Ͽ� ������ �Ǵ� ���� ���� ó��
    void AttackTile(Tile tile)
    {
        elapsed += Time.deltaTime; // ��ü ���ݽð� ��꺯��
        if (tile.ColorState == TileColorState.Enemy)
        {
            isTickTime += Time.deltaTime; // ƽ�� ����� ����
            if (isTickTime >= turretData.laserTickInterval) // ���� ƽ�ð��� attrate(���ݱ��� �ɸ��½ð�) 
            {
                // ����Ʈ ���
                EffectManager.Instance.PlayEffect( TurretType.Laser, TurretActionType.AttackTile,tile.CenterWorldPos);

                // Ÿ�� �� -> �÷��̾��
                tile.SetColor(TileColorState.Player);
                // Ÿ���� �ִϸ��̼����
                tile.AnimateBump();

                // ������ �������� Ÿ���� �ͷ��� null�� ����
                tile.TargetingTurret = null;
                targetTile = null;
                // ƽ ���� 0����
                isTickTime = 0;
            }
        }

        // ������ ���� �ð� �ʰ� �� Ÿ�� �ʱ�ȭ �� ���� ��� ����
        if (elapsed >= turretData.laserDuration)
        {
            tile.Release();
            tile.TargetingTurret = null;

            elapsed = 0f;
            lineRenderer.enabled = false;
            StartCoroutine(CooldownDelay());
        }
    }
    protected override void AttackTile() { /* ƽ ���������� ��� �� �� */ }
    // ���� ��� �ڷ�ƾ
 
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
