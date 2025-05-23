using UnityEngine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [SerializeField] private float cooldown = 0.1f;

    bool shaking = false;
    float lastShakeTime = -10f;
    Coroutine shakeRoutine;

    private void Awake()
    {
        Instance = this;
    }

    public void RequestShake(float intensity)
    {
        if (Time.time - lastShakeTime < cooldown)
            return;

        lastShakeTime = Time.time;

        // ���� ����ũ ���̸� �ߺ� ���� ����
        if (shaking) return;

        shakeRoutine = StartCoroutine(ShakeCoroutine(intensity));
    }

    private IEnumerator ShakeCoroutine(float intensity)
    {
        shaking = true;

        CameraShake.Instance.Shake(intensity);  // ī�޶� ���� ��鸲 Ʈ����

        yield return new WaitForSeconds(cooldown);  // ��Ÿ�� ����

        shaking = false;
    }
}
