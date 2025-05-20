using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretUIButtonBinder : MonoBehaviour
{
    [SerializeField] private Button[] buttons; // UI 배치할 버튼들

    [System.Serializable]
    public class TurretEntry
    {
        public TurretData data;
        public GameObject prefab;
    }

    [System.Serializable]
    public class FenceEntry
    {
        public FenceData data;
        public GameObject prefab;
    }


    [SerializeField] private List<TurretEntry> turretList;
    [SerializeField] private List<FenceEntry> fenceList;
    void Start()
    {
        int totalTurretButtons = turretList.Count;

        for (int i = 0; i < turretList.Count; i++)
        {
            var handler = buttons[i].GetComponent<UIButtonTurret>();
            handler.InitTurret(turretList[i].data, turretList[i].prefab);
            buttons[i].onClick.AddListener(handler.OnClickTurret);
        }

        for (int i = 0; i < fenceList.Count; i++)
        {
            var handler = buttons[totalTurretButtons + i].GetComponent<UIButtonTurret>();
            handler.InitFence(fenceList[i].data, fenceList[i].prefab);
            buttons[totalTurretButtons + i].onClick.AddListener(handler.OnClickFence);
        }
    }
}
