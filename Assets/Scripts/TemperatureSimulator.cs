using UnityEngine;

public class TemperatureSimulator : MonoBehaviour
{
    [Header("Temperature")]
    public float baseTemperature = 24f;
    public float temperatureAmplitude = 3f;
    public float temperatureSpeed = 0.5f;
    public float currentTemperature;

    [Header("Humidity")]
    public float baseHumidity = 55f;
    public float humidityAmplitude = 10f;
    public float humiditySpeed = 0.25f;
    public float currentHumidity;

    [Header("Air Velocity")]
    public float baseAirVelocity = 0.3f;
    public float airVelocityAmplitude = 0.2f;
    public float airVelocitySpeed = 0.8f;
    public float currentAirVelocity;

    private Renderer cubeRenderer;

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        currentTemperature =
            baseTemperature +
            Mathf.Sin(Time.time * temperatureSpeed) *
            temperatureAmplitude;

        currentHumidity =
            baseHumidity +
            Mathf.Sin(Time.time * humiditySpeed) *
            humidityAmplitude;

        currentAirVelocity =
            baseAirVelocity +
            Mathf.Sin(Time.time * airVelocitySpeed) *
            airVelocityAmplitude;

        if (currentTemperature < 23f)
        {
            cubeRenderer.material.color = Color.blue;
        }
        else if (currentTemperature > 25f)
        {
            cubeRenderer.material.color = Color.red;
        }
        else
        {
            cubeRenderer.material.color = Color.green;
        }
    }
}