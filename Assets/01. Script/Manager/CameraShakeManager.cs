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

        // 기존 쉐이크 중이면 중복 실행 방지
        if (shaking) return;

        shakeRoutine = StartCoroutine(ShakeCoroutine(intensity));
    }

    private IEnumerator ShakeCoroutine(float intensity)
    {
        shaking = true;

        CameraShake.Instance.Shake(intensity);  // 카메라 실제 흔들림 트리거

        yield return new WaitForSeconds(cooldown);  // 쿨타임 유지

        shaking = false;
    }
}
