using UnityEngine;

public class EnvironmentSimulator : MonoBehaviour
{
    public ChamberController chamberController;

    [Header("Live Chamber Values")]
    public float currentTemperature = 24f;
    public float currentHumidity = 55f;
    public float currentAirVelocity = 0f;

    [Header("Air")]
    public float maximumAirVelocity = 1.2f;

    [Header("Humidity")]
    public float baseHumidity = 55f;
    public float ambientTemperature = 24f;
    public float humidityDropPerDegree = 1.5f;

    private ThermalZone[] thermalZones;
    private Renderer sensorRenderer;

    void Start()
    {
        // Find all thermal tiles in the chamber
        thermalZones = FindObjectsOfType<ThermalZone>();

        // Get renderer of the EnvironmentSensor cube
        sensorRenderer = GetComponent<Renderer>();

        // Automatically find ChamberController if not assigned manually
        if (chamberController == null)
        {
            chamberController = FindObjectOfType<ChamberController>();
        }

        // Initialize values
        currentTemperature = ambientTemperature;
        currentHumidity = baseHumidity;
        currentAirVelocity = 0f;
    }

    void Update()
    {
        UpdateAverageTemperature();
        UpdateAirVelocity();
        UpdateHumidity();
        UpdateSensorColor();
    }

    // Calculate average temperature of all thermal zones
    void UpdateAverageTemperature()
    {
        if (thermalZones == null || thermalZones.Length == 0)
            return;

        float totalTemperature = 0f;

        foreach (ThermalZone zone in thermalZones)
        {
            totalTemperature += zone.temperature;
        }

        currentTemperature =
            totalTemperature / thermalZones.Length;
    }

    // Fan speed controls displayed air velocity
    void UpdateAirVelocity()
    {
        if (chamberController == null)
            return;

        float fanStrength =
            chamberController.fanSpeed / 100f;

        currentAirVelocity =
            fanStrength * maximumAirVelocity;
    }

    // Approximate RH response to average chamber temperature
    void UpdateHumidity()
    {
        float temperatureDifference =
            currentTemperature - ambientTemperature;

        float targetHumidity =
            baseHumidity -
            temperatureDifference * humidityDropPerDegree;

        currentHumidity = Mathf.Lerp(
            currentHumidity,
            targetHumidity,
            0.5f * Time.deltaTime
        );

        currentHumidity = Mathf.Clamp(
            currentHumidity,
            0f,
            100f
        );
    }

    // Change sensor cube colour according to average temperature
    void UpdateSensorColor()
    {
        if (sensorRenderer == null)
            return;

        Color coolColor =
            new Color(0.35f, 0.55f, 0.70f);

        Color neutralColor =
            new Color(0.65f, 0.70f, 0.60f);

        Color warmColor =
            new Color(0.90f, 0.55f, 0.30f);

        Color targetColor;

        if (currentTemperature <= 24f)
        {
            float t = Mathf.InverseLerp(
                20f,
                24f,
                currentTemperature
            );

            targetColor = Color.Lerp(
                coolColor,
                neutralColor,
                t
            );
        }
        else
        {
            float t = Mathf.InverseLerp(
                24f,
                32f,
                currentTemperature
            );

            targetColor = Color.Lerp(
                neutralColor,
                warmColor,
                t
            );
        }

        sensorRenderer.material.color = targetColor;
    }
}