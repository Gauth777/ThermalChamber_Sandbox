using UnityEngine;
public class EnvironmentSimulator : MonoBehaviour
{
    public ChamberController chamberController;
    public float currentTemperature = 24, currentHumidity = 55, currentAirVelocity;
    public float maximumAirVelocity = 1.2f, baseHumidity = 55, ambientTemperature = 24, humidityDropPerDegree = 1.5f;
    public bool HasReading { get; private set; }
    void Start() { if (!chamberController) chamberController = FindObjectOfType<ChamberController>(); }
    void Update()
    {
        var sim = chamberController ? chamberController.Simulation : null;
        HasReading = sim && sim.HasReading;
        currentTemperature = HasReading ? (float)sim.Reading.TemperatureCelsius : float.NaN;
        currentHumidity = HasReading ? (float)sim.Reading.RelativeHumidityPercent : float.NaN;
        currentAirVelocity = HasReading ? (float)sim.Reading.AirVelocityMetresPerSecond : float.NaN;
    }
}
