using UnityEngine;
using TMPro;

public class EnvironmentUI : MonoBehaviour
{
    public EnvironmentSimulator environmentSimulator;

    public TMP_Text temperatureText;
    public TMP_Text humidityText;
    public TMP_Text airVelocityText;

    void Update()
    {
        if (environmentSimulator == null)
            return;

        temperatureText.text =
            "Temperature: " +
            environmentSimulator.currentTemperature.ToString("F1") +
            " °C";

        humidityText.text =
            "Humidity: " +
            environmentSimulator.currentHumidity.ToString("F1") +
            " %";

        airVelocityText.text =
            "Air Velocity: " +
            environmentSimulator.currentAirVelocity.ToString("F2") +
            " m/s";
    }
}