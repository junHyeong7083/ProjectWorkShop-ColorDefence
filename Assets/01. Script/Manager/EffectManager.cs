using System.Collections.Generic;
using UnityEngine;
using System.Collections;


[System.Serializable]
public struct EffectEntry
{
    public TurretType turretType;
    public TurretActionType turretActionType;
    public GameObject effectPrefab;
    public int Count;
}
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [SerializeField] private List<EffectEntry> effectEntries;

    private Dictionary<(TurretType, TurretActionType), Queue<GameObject>> effectPools;
    private Dictionary<(TurretType, TurretActionType), GameObject> prefabDict;

    Transform ParticleParnet;
    private void Awake()
    {
        Instance = this;
        effectPools = new();
        prefabDict = new();

        GameObject parent = new GameObject("ParticleParent");
        ParticleParnet = parent.transform;

        foreach (var entry in effectEntries)
        {
            var key = (entry.turretType, entry.turretActionType);
            prefabDict[key] = entry.effectPrefab;
            effectPools[key] = new Queue<GameObject>();

            for (int i = 0; i < entry.Count; i++)
            {
                GameObject fx = Instantiate(entry.effectPrefab, ParticleParnet);
                fx.SetActive(false);
                effectPools[key].Enqueue(fx);
            }
        }
    }

    public void PlayEffect(TurretType turret, TurretActionType action, Vector3 position)
    {
        var key = (turret, action);
        if (!prefabDict.ContainsKey(key))
        {
            Debug.LogWarning($"이펙트 미등록: {turret}_{action}");
            return;
        }

        GameObject fx = null;

        if (effectPools[key].Count > 0)
        {
            fx = effectPools[key].Dequeue();
        }
        else
        {
            fx = Instantiate(prefabDict[key]);
            fx.transform.SetParent(ParticleParnet);
        }

        fx.transform.position = position + Vector3.up * 1.5f;
        fx.SetActive(true);

        // 파티클 플레이
        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(ReturnToPoolAfterSeconds(key, fx, ps.main.duration + ps.main.startLifetime.constant));
        }
        else
        {
            // 그냥 1.5초 후에 꺼버리기
            StartCoroutine(ReturnToPoolAfterSeconds(key, fx, 1.5f));
        }
    }

    IEnumerator ReturnToPoolAfterSeconds((TurretType, TurretActionType) key, GameObject fx, float delay)
    {
        yield return new WaitForSeconds(delay);
        fx.SetActive(false);
        effectPools[key].Enqueue(fx);
    }
}

