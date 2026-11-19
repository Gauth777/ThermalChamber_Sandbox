using UnityEngine;
using TMPro;
public class EnvironmentUI : MonoBehaviour
{
    public EnvironmentSimulator environmentSimulator;
    public TMP_Text temperatureText, humidityText, airVelocityText;
    void Update()
    {
        bool valid = environmentSimulator && environmentSimulator.HasReading;
        if (temperatureText) temperatureText.text = valid ? "Temperature: " + environmentSimulator.currentTemperature.ToString("F1") + " C" : "Temperature: unavailable";
        if (humidityText) humidityText.text = valid ? "Humidity: " + environmentSimulator.currentHumidity.ToString("F1") + " %" : "Humidity: unavailable";
        if (airVelocityText) airVelocityText.text = valid ? "Air velocity: " + environmentSimulator.currentAirVelocity.ToString("F2") + " m/s" : "Air velocity: unavailable";
    }
}
