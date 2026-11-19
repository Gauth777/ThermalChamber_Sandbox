using UnityEngine;
public class HeaterHeatSource : MonoBehaviour
{
    public ChamberController chamberController;
    [HideInInspector] public float heatOutput = 2.5f; // Legacy degrees/sec; intentionally not interpreted as watts.
    [Min(0)] public float powerWatts = 500f;
    [Min(0.01f)] public float heatingRadius = 6f;
    // ThermalSimulation distributes this total power across nearby cells, once per fixed step.
}
