using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EffectEntry
{
    public GameObject effectPrefab;
    public int Count;
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [SerializeField] private List<EffectEntry> effectEntries;

    private Dictionary<string, Queue<GameObject>> effectPools;
    private Dictionary<string, GameObject> prefabDict;

    Transform particleParent;

    private void Awake()
    {
        Instance = this;
        effectPools = new();
        prefabDict = new();

        GameObject parent = new GameObject("ParticleParent");
        particleParent = parent.transform;

        foreach (var entry in effectEntries)
        {
            string key = entry.effectPrefab.name;

            prefabDict[key] = entry.effectPrefab;
            effectPools[key] = new Queue<GameObject>();

            for (int i = 0; i < entry.Count; i++)
            {
                GameObject fx = Instantiate(entry.effectPrefab, particleParent);
                fx.SetActive(false);
                effectPools[key].Enqueue(fx);
            }
        }
    }

    public void PlayEffect(string effectName, Vector3 position)
    {
        if (!prefabDict.ContainsKey(effectName))
        {
            Debug.LogWarning($"이펙트 미등록: {effectName}");
            return;
        }

        GameObject fx = null;

        if (effectPools[effectName].Count > 0)
        {
            fx = effectPools[effectName].Dequeue();
        }
        else
        {
            fx = Instantiate(prefabDict[effectName], particleParent);
        }

        fx.transform.position = position + Vector3.up * 1.5f;
        fx.SetActive(true);

        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(ReturnToPoolAfterSeconds(effectName, fx, ps.main.duration + ps.main.startLifetime.constant));
        }
        else
        {
            StartCoroutine(ReturnToPoolAfterSeconds(effectName, fx, 1.5f));
        }
    }

    public void PlayEffect(string effectName, Vector3 position, bool isDynamicSize)
    {
        if (!prefabDict.ContainsKey(effectName))
        {
            Debug.LogWarning($"이펙트 미등록: {effectName}");
            return;
        }

        GameObject fx = null;

        if (effectPools[effectName].Count > 0)
            fx = effectPools[effectName].Dequeue();
        else
            fx = Instantiate(prefabDict[effectName], particleParent);

        fx.transform.position = position + Vector3.up * 1.5f;
        fx.SetActive(true);

        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            if (isDynamicSize)
            {
                float dynamicSize = transform.localScale.magnitude * 0.5f; // 적당히 조절
                var main = ps.main;
                main.startSize = new ParticleSystem.MinMaxCurve(dynamicSize);
            }

            ps.Play();
            StartCoroutine(ReturnToPoolAfterSeconds(effectName, fx, ps.main.duration + ps.main.startLifetime.constant));
        }
        else
        {
            StartCoroutine(ReturnToPoolAfterSeconds(effectName, fx, 1.5f));
        }
    }

    IEnumerator ReturnToPoolAfterSeconds(string effectName, GameObject fx, float delay)
    {
        yield return YieldCache.WaitForSeconds(delay);
        fx.SetActive(false);
        if (!effectPools[effectName].Contains(fx))
            effectPools[effectName].Enqueue(fx);
    }

}
