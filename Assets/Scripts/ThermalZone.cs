using UnityEngine;
using System.Collections.Generic;

public class ThermalZone : MonoBehaviour
{
    [Header("Temperature")]
    public float temperature = 24f;
    public float ambientTemperature = 24f;
    public float maxTemperature = 32f;
    public float minTemperature = 20f;

    [Header("Natural Cooling")]
    public float naturalCoolingRate = 0.08f;

    [Header("Fan Effect")]
    public float fanCoolingRate = 0.5f;
    public float fanDiffusionBoost = 1.5f;

    [Header("Heat Diffusion")]
    public float baseDiffusionRate = 0.25f;

    [Header("Colour Range")]
    public float coldTemperature = 20f;
    public float neutralTemperature = 24f;
    public float warmTemperature = 30f;

    private Renderer zoneRenderer;
    private ChamberController chamberController;

    private bool occupantInside;
    private OccupantHeat occupantHeat;

    private readonly List<ThermalZone> neighbors =
        new List<ThermalZone>();

    void Start()
    {
        zoneRenderer = GetComponent<Renderer>();

        chamberController =
            FindObjectOfType<ChamberController>();

        FindNeighbors();
    }

    void Update()
    {
        ApplyOccupantHeat();
        ApplyNaturalCooling();
        ApplyFanCooling();
        DiffuseHeat();

        temperature = Mathf.Clamp(
            temperature,
            minTemperature,
            maxTemperature
        );

        UpdateColor();
    }

    void ApplyOccupantHeat()
    {
        if (occupantInside && occupantHeat != null)
        {
            temperature +=
                occupantHeat.heatOutput * Time.deltaTime;
        }
    }

    void ApplyNaturalCooling()
    {
        // Slowly returns toward ambient temperature
        temperature = Mathf.MoveTowards(
            temperature,
            ambientTemperature,
            naturalCoolingRate * Time.deltaTime
        );
    }

    void ApplyFanCooling()
    {
        if (chamberController == null)
            return;

        float fanStrength =
            chamberController.fanSpeed / 100f;

        if (fanStrength <= 0f)
            return;

        // Fan brings heated zones back toward ambient faster
        temperature = Mathf.MoveTowards(
            temperature,
            ambientTemperature,
            fanCoolingRate *
            fanStrength *
            Time.deltaTime
        );
    }

    void FindNeighbors()
    {
        ThermalZone[] allZones =
            FindObjectsOfType<ThermalZone>();

        foreach (ThermalZone zone in allZones)
        {
            if (zone == this)
                continue;

            float dx = Mathf.Abs(
                transform.position.x -
                zone.transform.position.x
            );

            float dz = Mathf.Abs(
                transform.position.z -
                zone.transform.position.z
            );

            bool neighborX =
                dx > 0.1f &&
                dx < 2.2f &&
                dz < 0.2f;

            bool neighborZ =
                dz > 0.1f &&
                dz < 2.2f &&
                dx < 0.2f;

            if (neighborX || neighborZ)
            {
                neighbors.Add(zone);
            }
        }
    }

    void DiffuseHeat()
    {
        if (neighbors.Count == 0)
            return;

        float averageNeighborTemperature = 0f;

        foreach (ThermalZone zone in neighbors)
        {
            averageNeighborTemperature +=
                zone.temperature;
        }

        averageNeighborTemperature /=
            neighbors.Count;

        float diffusionRate =
            baseDiffusionRate;

        // Fan mixes the chamber air faster
        if (chamberController != null)
        {
            float fanStrength =
                chamberController.fanSpeed / 100f;

            diffusionRate +=
                fanStrength * fanDiffusionBoost;
        }

        temperature = Mathf.Lerp(
            temperature,
            averageNeighborTemperature,
            diffusionRate * Time.deltaTime
        );
    }

    public void AddHeat(float amount)
    {
        temperature += amount;

        temperature = Mathf.Clamp(
            temperature,
            minTemperature,
            maxTemperature
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            occupantInside = true;

            occupantHeat =
                other.GetComponent<OccupantHeat>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            occupantInside = false;
            occupantHeat = null;
        }
    }

    void UpdateColor()
    {
        if (zoneRenderer == null)
            return;

        // Muted colours — not unrealistic bright red
        Color coldColor =
            new Color(0.35f, 0.55f, 0.70f);

        Color neutralColor =
            new Color(0.76f, 0.75f, 0.68f);

        Color warmColor =
            new Color(0.90f, 0.55f, 0.30f);

        Color targetColor;

        if (temperature <= neutralTemperature)
        {
            float t = Mathf.InverseLerp(
                coldTemperature,
                neutralTemperature,
                temperature
            );

            targetColor = Color.Lerp(
                coldColor,
                neutralColor,
                t
            );
        }
        else
        {
            float t = Mathf.InverseLerp(
                neutralTemperature,
                warmTemperature,
                temperature
            );

            targetColor = Color.Lerp(
                neutralColor,
                warmColor,
                t
            );
        }

        zoneRenderer.material.color =
            targetColor;
    }
}