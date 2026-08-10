using UnityEngine;

public class EnvironmentSimulator : MonoBehaviour
{
    public ChamberController chamberController;

    [Header("Temperature")]
    public float ambientTemperature = 24f;
    public float currentTemperature = 24f;

    public float heaterTemperatureGain = 8f;
    public float fanCoolingEffect = 3f;
    public float temperatureResponseSpeed = 0.3f;

    [Header("Humidity")]
    public float baseHumidity = 55f;
    public float currentHumidity = 55f;
    public float humidityDropPerDegree = 1.5f;

    [Header("Air Velocity")]
    public float currentAirVelocity = 0f;
    public float maximumAirVelocity = 1.2f;
    public float airVelocityResponseSpeed = 2f;

    private Renderer sensorRenderer;

    void Start()
    {
        sensorRenderer = GetComponent<Renderer>();

        currentTemperature = ambientTemperature;
        currentHumidity = baseHumidity;
    }

    void Update()
    {
        if (chamberController == null)
            return;

        UpdateTemperature();
        UpdateAirVelocity();
        UpdateHumidity();
        UpdateVisual();
    }

    void UpdateTemperature()
    {
        float targetTemperature = ambientTemperature;

        if (chamberController.heaterOn)
        {
            targetTemperature += heaterTemperatureGain;
        }

        float fanCooling =
            (chamberController.fanSpeed / 100f)
            * fanCoolingEffect;

        targetTemperature -= fanCooling;

        currentTemperature = Mathf.MoveTowards(
            currentTemperature,
            targetTemperature,
            temperatureResponseSpeed * Time.deltaTime
        );
    }

    void UpdateAirVelocity()
    {
        float targetVelocity =
            (chamberController.fanSpeed / 100f)
            * maximumAirVelocity;

        currentAirVelocity = Mathf.Lerp(
            currentAirVelocity,
            targetVelocity,
            airVelocityResponseSpeed * Time.deltaTime
        );
    }

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
    }

    void UpdateVisual()
    {
        if (sensorRenderer == null)
            return;

        if (currentTemperature < 23f)
            sensorRenderer.material.color = Color.blue;

        else if (currentTemperature > 27f)
            sensorRenderer.material.color = Color.red;

        else
            sensorRenderer.material.color = Color.green;
    }
}