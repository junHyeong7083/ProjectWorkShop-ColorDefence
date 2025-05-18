using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct EffectEntry
{
    public TurretType turretType;
    public TurretActionType turretActionType;
    public GameObject effectPrefab;
}
public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [SerializeField] private List<EffectEntry> effectEntries;

    private Dictionary<(TurretType, TurretActionType), GameObject> effectDict;

    private void Awake()
    {
        Instance = this;
        effectDict = new Dictionary<(TurretType, TurretActionType), GameObject> ();

        foreach( var entry in effectEntries)
        {
            effectDict[(entry.turretType, entry.turretActionType)] = entry.effectPrefab;
        }
    }

    public void PlayEffect(TurretType turret, TurretActionType target, Vector3 position)
    {
        Vector3 offset = new Vector3(position.x, 1.5f, position.z);
        if (effectDict.TryGetValue((turret, target), out GameObject fxPrefab))
        {
            GameObject go = Instantiate(fxPrefab, offset, Quaternion.identity);
            Destroy(go, 1.5f);
        }
        else
        {
            Debug.LogWarning($"이펙트 미등록: {turret}_{target}");
        }
    }
}
