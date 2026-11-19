using UnityEngine;
public class OccupantHeat : MonoBehaviour
{
    [HideInInspector] public float heatOutput = 0.6f; // Legacy rate, retained for serialization.
    [Min(0)] public float sensibleHeatWatts = 80f;
    [Min(0.01f)] public float heatingRadius = 2f;
}
