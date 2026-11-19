using UnityEngine;

// Serialized legacy fields remain for scene/prefab compatibility.
// ThermalSimulation owns all integration; this component only holds state and displays it.
public class ThermalZone : MonoBehaviour
{
    public float temperature = 24f, ambientTemperature = 24f;
    public float maxTemperature = 32f, minTemperature = 20f;
    public float naturalCoolingRate = 0.08f, fanCoolingRate = 0.5f;
    public float fanDiffusionBoost = 1.5f, baseDiffusionRate = 0.25f;
    public float coldTemperature = 20f, neutralTemperature = 24f, warmTemperature = 30f;
    Renderer zoneRenderer;
    MaterialPropertyBlock block;
    public bool HeatmapVisible = true;
    static readonly Color[] Palette = {
        new Color(.35f,.55f,.70f), new Color(.52f,.63f,.69f),
        new Color(.76f,.75f,.70f), new Color(.79f,.71f,.58f),
        new Color(.85f,.67f,.43f), new Color(.88f,.58f,.34f),
        new Color(.89f,.47f,.26f)
    };
    void Awake() { zoneRenderer = GetComponent<Renderer>(); block = new MaterialPropertyBlock(); }
    void LateUpdate()
    {
        if (!zoneRenderer) return;
        zoneRenderer.enabled = HeatmapVisible;
        float t = Mathf.Clamp((temperature - 20f) / 2f, 0, 6);
        int i = Mathf.Min(Mathf.FloorToInt(t), 5);
        zoneRenderer.GetPropertyBlock(block);
        block.SetColor("_Color", Color.Lerp(Palette[i], Palette[i+1], t-i));
        zoneRenderer.SetPropertyBlock(block);
    }
    // Compatibility entry point, units explicitly Celsius. Do not use as an actuator power API.
    public void AddHeat(float deltaCelsius)
    {
        if (!float.IsNaN(deltaCelsius) && !float.IsInfinity(deltaCelsius)) temperature += deltaCelsius;
    }
}
