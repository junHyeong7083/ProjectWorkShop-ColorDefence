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

    [SerializeField] private List<TurretEntry> turretList;

    void Start()
    {

        int e = 0;
        foreach (var entry in turretList)
        {
            var button = buttons[e].GetComponent<Button>();
            var handler = buttons[e].GetComponent<UIButtonTurret>();

            handler.Init(entry.data, entry.prefab);
            button.onClick.AddListener(handler.OnClick);
            e++;
        }
    }
}
