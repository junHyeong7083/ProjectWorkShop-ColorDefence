using UnityEngine;

public class QueenAnt : MonoBehaviour, FogRevealFog
{
    [SerializeField] float viewRange;
    public void RevealFog()
    {
        FogOfWarSystem.Instance.RevealAreaGradient(transform.position, viewRange);
    }

    private void Start() => FogOfWarSystem.Instance?.Register(this);
    private void OnDisable() => FogOfWarSystem.Instance?.Unregister(this);
}
