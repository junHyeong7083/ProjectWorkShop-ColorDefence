using UnityEngine;
using System.Collections;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    [SerializeField] private float cooldown = 0.1f;

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

        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        shakeRoutine = StartCoroutine(ShakeCoroutine(intensity));
    }

    private IEnumerator ShakeCoroutine(float intensity)
    {
        CameraShake.Instance.Shake(intensity);

        // YieldCache 사용으로 GC 방지
        yield return YieldCache.WaitForSeconds(cooldown);

        shakeRoutine = null;
    }
}
