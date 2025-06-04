using UnityEngine;

public class QueenAnt : MonoBehaviour, IFogRevealer
{
    public float viewRange => 15f;

    public int FogRevealerIndex { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public void RevealFog()
    {
        FogOfWarSystem.Instance.RevealAreaGradient(transform.position, viewRange);
    }

    private void Start() => FogOfWarSystem.Instance?.Register(this);
    private void OnDisable() => FogOfWarSystem.Instance?.Unregister(this);
}
