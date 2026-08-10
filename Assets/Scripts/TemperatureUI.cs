using UnityEngine;
using TMPro;

public class TemperatureUI : MonoBehaviour
{
    public TemperatureSimulator temperatureSimulator;

    private TMP_Text temperatureText;

    void Start()
    {
        temperatureText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        temperatureText.text =
            "Temperature: " +
            temperatureSimulator.currentTemperature.ToString("F1") +
            " °C";
    }
}