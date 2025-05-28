using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private Transform camTransform;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeAmount = 0.2f;

    Vector3 originalPos;
    Coroutine currentShake;

    private void Awake()
    {
        Instance = this;
        originalPos = camTransform.localPosition;
    }
    public void Shake(float intensity = 1f)
    {
        if (currentShake != null)
            StopCoroutine(currentShake);

        originalPos = camTransform.localPosition;

        currentShake = StartCoroutine(DoShake(intensity));
    }

    private IEnumerator DoShake(float intensity)
    {
        float duration = shakeDuration * intensity;
        float amount = shakeAmount * intensity;

        float t = 0f;
        while (t < duration)
        {
            camTransform.localPosition = originalPos + Random.insideUnitSphere * amount;
            t += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
        currentShake = null;
    }
}
